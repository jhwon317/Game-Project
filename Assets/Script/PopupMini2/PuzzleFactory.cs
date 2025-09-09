using UnityEngine;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

namespace PopupMini
{
    public struct PuzzleInstance
    {
        public GameObject Root;
        public Camera Cam;
        public IPuzzleController Controller;
    }

    public static class PuzzleLayerUtil
    {
        public static void SetLayerRecursive(GameObject go, int layer)
        {
            if (!go) return;
            go.layer = layer;
            foreach (Transform c in go.transform)
                SetLayerRecursive(c.gameObject, layer);
        }
    }

    public class PuzzleFactory
    {
        readonly string _layerName;
        int _layer = int.MinValue;

        public PuzzleFactory(string miniLayerName = "MiniWorld") { _layerName = miniLayerName; }

        int GetLayer()
        {
            if (_layer == int.MinValue)
            {
                _layer = string.IsNullOrEmpty(_layerName) ? -1 : LayerMask.NameToLayer(_layerName);
                if (_layer < 0) Debug.LogWarning($"[PuzzleFactory] Layer '{_layerName}' not found. Skipping layer assign.");
            }
            return _layer;
        }

        public PuzzleInstance Create(PuzzleDefinition def)
        {
            if (!def || !def.Prefab) { Debug.LogError("[PuzzleFactory] Definition/Prefab missing"); return default; }

            var go = Object.Instantiate(def.Prefab);

            // ★ 레이어 분리(옵션)
            var layer = GetLayer();
            if (layer >= 0) PuzzleLayerUtil.SetLayerRecursive(go, layer);

            // ★ 필수 구성 요소 찾기
            var cam = go.GetComponentInChildren<Camera>(true);
            var ctrl = go.GetComponentInChildren<IPuzzleController>(true);

            if (!cam) { Debug.LogError("[PuzzleFactory] Camera not found in prefab"); Object.Destroy(go); return default; }
            if (ctrl == null) { Debug.LogError("[PuzzleFactory] IPuzzleController not found in prefab"); Object.Destroy(go); return default; }

#if UNITY_RENDER_PIPELINE_UNIVERSAL
            var urp = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urp) { urp.renderType = CameraRenderType.Base; urp.cameraStack.Clear(); }
#endif
            // ★ 메인 화면 덮지 않도록 기본은 꺼둔다
            cam.enabled = false;
            cam.targetTexture = null;
            cam.tag = "Untagged"; // MainCamera 금지

            if (def.ShadowsOff)
            {
                foreach (var li in go.GetComponentsInChildren<Light>(true))
                    li.shadows = LightShadows.None;
            }

            return new PuzzleInstance { Root = go, Cam = cam, Controller = ctrl };
        }

        public void Destroy(PuzzleInstance inst)
        {
            if (inst.Root) Object.Destroy(inst.Root);
        }
    }
}