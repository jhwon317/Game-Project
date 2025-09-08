using UnityEngine;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

namespace PopupMini
{
    public static class PuzzleCamSanitizer
    {
        public static void Apply(Camera cam, int miniLayer = -1, Color? bg = null)
        {
            if (!cam) return;
            if (cam.CompareTag("MainCamera")) cam.tag = "Untagged";
            var al = cam.GetComponent<AudioListener>(); if (al) Object.Destroy(al);
#if UNITY_RENDER_PIPELINE_UNIVERSAL
            var urp = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urp){ urp.renderType = CameraRenderType.Base; urp.cameraStack.Clear(); }
#endif
            if (miniLayer >= 0) cam.cullingMask = 1 << miniLayer;
            cam.depth = -100;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg ?? new Color(0, 0, 0, 0);
        }

        public static void SetLayerRecursive(GameObject root, int layer)
        {
            if (!root) return;
            root.layer = layer;
            foreach (Transform t in root.transform) SetLayerRecursive(t.gameObject, layer);
        }
    }
}