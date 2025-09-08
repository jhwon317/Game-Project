using UnityEngine;
using UnityEngine.UI;

namespace PopupMini
{
    public struct PuzzleInstance
    {
        public GameObject Root;
        public Camera Cam;
        public IPuzzleController Controller;
    }

    /// <summary>
    /// 퍼즐 프리팹 생성/정리 담당:
    /// - 프리팹 Instantiate
    /// - 레이어 MiniWorld 일괄 적용
    /// - 퍼즐 카메라 탐색/위생 처리
    /// - 프리팹 내부 Canvas를 퍼즐 카메라에 연결(월드/스크린-카메라)
    /// - GraphicRaycaster 보장(내부 UI 클릭 가능)
    /// - IPuzzleController 탐색(인터페이스 안전 스캔)
    /// - ShadowsOff 옵션 적용
    /// </summary>
    public class PuzzleFactory
    {
        readonly int _miniLayer;
        readonly string _miniLayerName;

        public PuzzleFactory(string miniLayerName = "MiniWorld")
        {
            _miniLayerName = miniLayerName;
            _miniLayer = LayerMask.NameToLayer(miniLayerName);
            if (_miniLayer < 0)
                Debug.LogWarning($"[PuzzleFactory] Layer '{miniLayerName}' not found. Layer isolation will be skipped.");
        }

        public PuzzleInstance Create(PuzzleDefinition def)
        {
            if (!def || !def.Prefab)
            {
                Debug.LogError("[PuzzleFactory] Definition/Prefab missing");
                return default;
            }

            // 1) 인스턴스 생성
            var go = Object.Instantiate(def.Prefab);
            go.name = $"{def.Prefab.name} (PuzzleInstance)";

            // 2) 레이어 일괄 적용 (선택)
            if (_miniLayer >= 0) PuzzleCamSanitizer.SetLayerRecursive(go, _miniLayer);

            // 3) 카메라 탐색(이름 우선 → 첫 카메라)
            var cam = FindPuzzleCamera(go);
            if (!cam)
            {
                Debug.LogError("[PuzzleFactory] Camera not found in prefab");
                Object.Destroy(go);
                return default;
            }

            // 4) 카메라 위생 처리 (URP Base, AudioListener 제거, CullingMask, BG 투명)
            PuzzleCamSanitizer.Apply(cam, _miniLayer, def.BackgroundColor);

            // 5) 내부 Canvas를 퍼즐 카메라에 연결(보이기 + 클릭 가능 준비)
            BindInternalCanvases(go, cam);

            // 6) 컨트롤러 안전 스캔(인터페이스 캐스팅)
            var ctrl = FindController(go);
            if (ctrl == null)
            {
                Debug.LogError("[PuzzleFactory] IPuzzleController not found in prefab");
                Object.Destroy(go);
                return default;
            }

            // 7) 섀도우 옵션
            if (def.ShadowsOff)
            {
                foreach (var li in go.GetComponentsInChildren<Light>(true))
                    li.shadows = LightShadows.None;
            }

            // 8) 요약 로그
            Debug.Log($"[PuzzleFactory] Created: root={go.name}, cam={cam.name}, ctrl={ctrl.GetType().Name}");

            return new PuzzleInstance { Root = go, Cam = cam, Controller = ctrl };
        }

        public void Destroy(PuzzleInstance inst)
        {
            if (inst.Root) Object.Destroy(inst.Root);
        }

        // ----------------- helpers -----------------

        Camera FindPuzzleCamera(GameObject root)
        {
            var cams = root.GetComponentsInChildren<Camera>(true);
            // 이름 "PuzzleCam" 우선
            foreach (var c in cams) if (c.name == "PuzzleCam") return c;
            return cams.Length > 0 ? cams[0] : null;
        }

        IPuzzleController FindController(GameObject root)
        {
            // 인터페이스 타입은 GetComponentInChildren<T>()가 상황에 따라 실패할 수 있으므로 안전 스캔
            var mbs = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in mbs)
            {
                if (mb is IPuzzleController ctrl) return ctrl;
            }
            return null;
        }

        void BindInternalCanvases(GameObject root, Camera cam)
        {
            var canvases = root.GetComponentsInChildren<Canvas>(true);
            foreach (var cv in canvases)
            {
                // RenderMode 보정: Overlay는 퍼즐 카메라로 렌더되지 않으므로 ScreenSpaceCamera로 전환
                if (cv.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    cv.renderMode = RenderMode.ScreenSpaceCamera;
                }

                if (cv.renderMode == RenderMode.ScreenSpaceCamera || cv.renderMode == RenderMode.WorldSpace)
                {
                    cv.worldCamera = cam; // ★ 퍼즐 카메라로 고정
                }

                // 그래픽 레이캐스터 보장(내부 UI 클릭용)
                if (!cv.TryGetComponent<GraphicRaycaster>(out _))
                {
                    cv.gameObject.AddComponent<GraphicRaycaster>();
                }

                // 레이어 통일(카메라 CullingMask와 일치)
                if (_miniLayer >= 0) SetLayerIfNeeded(cv.gameObject, _miniLayer);
            }
        }

        void SetLayerIfNeeded(GameObject go, int layer)
        {
            if (go.layer != layer) go.layer = layer;
            foreach (Transform t in go.transform)
                SetLayerIfNeeded(t.gameObject, layer);
        }
    }
}
