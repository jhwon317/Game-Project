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
    /// ���� ������ ����/���� ���:
    /// - ������ Instantiate
    /// - ���̾� MiniWorld �ϰ� ����
    /// - ���� ī�޶� Ž��/���� ó��
    /// - ������ ���� Canvas�� ���� ī�޶� ����(����/��ũ��-ī�޶�)
    /// - GraphicRaycaster ����(���� UI Ŭ�� ����)
    /// - IPuzzleController Ž��(�������̽� ���� ��ĵ)
    /// - ShadowsOff �ɼ� ����
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

            // 1) �ν��Ͻ� ����
            var go = Object.Instantiate(def.Prefab);
            go.name = $"{def.Prefab.name} (PuzzleInstance)";

            // 2) ���̾� �ϰ� ���� (����)
            if (_miniLayer >= 0) PuzzleCamSanitizer.SetLayerRecursive(go, _miniLayer);

            // 3) ī�޶� Ž��(�̸� �켱 �� ù ī�޶�)
            var cam = FindPuzzleCamera(go);
            if (!cam)
            {
                Debug.LogError("[PuzzleFactory] Camera not found in prefab");
                Object.Destroy(go);
                return default;
            }

            // 4) ī�޶� ���� ó�� (URP Base, AudioListener ����, CullingMask, BG ����)
            PuzzleCamSanitizer.Apply(cam, _miniLayer, def.BackgroundColor);

            // 5) ���� Canvas�� ���� ī�޶� ����(���̱� + Ŭ�� ���� �غ�)
            BindInternalCanvases(go, cam);

            // 6) ��Ʈ�ѷ� ���� ��ĵ(�������̽� ĳ����)
            var ctrl = FindController(go);
            if (ctrl == null)
            {
                Debug.LogError("[PuzzleFactory] IPuzzleController not found in prefab");
                Object.Destroy(go);
                return default;
            }

            // 7) ������ �ɼ�
            if (def.ShadowsOff)
            {
                foreach (var li in go.GetComponentsInChildren<Light>(true))
                    li.shadows = LightShadows.None;
            }

            // 8) ��� �α�
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
            // �̸� "PuzzleCam" �켱
            foreach (var c in cams) if (c.name == "PuzzleCam") return c;
            return cams.Length > 0 ? cams[0] : null;
        }

        IPuzzleController FindController(GameObject root)
        {
            // �������̽� Ÿ���� GetComponentInChildren<T>()�� ��Ȳ�� ���� ������ �� �����Ƿ� ���� ��ĵ
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
                // RenderMode ����: Overlay�� ���� ī�޶�� �������� �����Ƿ� ScreenSpaceCamera�� ��ȯ
                if (cv.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    cv.renderMode = RenderMode.ScreenSpaceCamera;
                }

                if (cv.renderMode == RenderMode.ScreenSpaceCamera || cv.renderMode == RenderMode.WorldSpace)
                {
                    cv.worldCamera = cam; // �� ���� ī�޶�� ����
                }

                // �׷��� ����ĳ���� ����(���� UI Ŭ����)
                if (!cv.TryGetComponent<GraphicRaycaster>(out _))
                {
                    cv.gameObject.AddComponent<GraphicRaycaster>();
                }

                // ���̾� ����(ī�޶� CullingMask�� ��ġ)
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
