using UnityEngine;

namespace PopupMini
{
    public enum PaddingMode { PixelsUI, PercentOfPanel }

    [DisallowMultipleComponent]
    public class PopupAutoLayout : MonoBehaviour
    {
        public PopupHost host;

        [Header("Panel Padding ¡æ ContentRoot.offset")]
        public bool usePadding = true;
        public PaddingMode paddingMode = PaddingMode.PixelsUI;
        public Vector4 padding = new Vector4(24, 24, 24, 24);            // L,R,T,B
        public Vector4 paddingPercent = new Vector4(0.05f, 0.05f, 0.05f, 0.05f); // L,R,T,B (0~1)

        [Header("ContentRoot Scale")]
        [Min(0.01f)] public float contentScale = 1f;

        [Header("Auto Fit PuzzleCam")]
        public bool autoFitCamera = true;
        public FitMode fitMode = FitMode.Contain;
        public float referenceAspect = 16f / 9f;
        public float referenceOrthoSize = 5f;
        [Range(0, 0.4f)] public float cameraPaddingPct = 0.08f;

        Canvas root;

        void Reset() { host = GetComponent<PopupHost>(); }

        public void ApplyBeforeOpen()
        {
            if (!host || !host.ContentRoot) return;
            if (!root) root = host.GetComponentInParent<Canvas>()?.rootCanvas;

            if (usePadding) ApplyPadding(host.ContentRoot);
            host.ContentRoot.localScale = new Vector3(contentScale, contentScale, 1f);
            KeepViewportStretch();
        }

        public void ApplyAfterBind(PuzzleInstance inst)
        {
            if (!autoFitCamera || inst.Cam == null || host?.Viewport == null) return;
            var vp = host.Viewport.GetComponent<RectTransform>();
            if (!vp) return;

            if (inst.Cam.orthographic)
                PuzzleCamFitter.FitOrthoByAspect(inst.Cam, vp, referenceAspect, referenceOrthoSize, fitMode, cameraPaddingPct);
            else
                PuzzleCamFitter.FitBoundsPerspective(inst.Cam, inst.Root.transform, vp, fitMode, cameraPaddingPct);
        }

        void ApplyPadding(RectTransform content)
        {
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 0.5f);

            Vector4 off;
            if (paddingMode == PaddingMode.PixelsUI)
            {
                off = padding;
            }
            else
            {
                var rc = root ? root.pixelRect.size : new Vector2(Screen.width, Screen.height);
                float sf = root ? Mathf.Max(0.0001f, root.scaleFactor) : 1f;
                float w = rc.x / sf, h = rc.y / sf;
                off = new Vector4(
                    paddingPercent.x * w,
                    paddingPercent.y * w,
                    paddingPercent.z * h,
                    paddingPercent.w * h
                );
            }

            // offsetMin=(L,B), offsetMax=(-R,-T)
            content.offsetMin = new Vector2(off.x, off.w);
            content.offsetMax = new Vector2(-off.y, -off.z);
        }

        void KeepViewportStretch()
        {
            var v = host?.Viewport ? host.Viewport.transform as RectTransform : null;
            if (!v) return;
            v.anchorMin = Vector2.zero; v.anchorMax = Vector2.one;
            v.offsetMin = Vector2.zero; v.offsetMax = Vector2.zero;
        }
    }
}