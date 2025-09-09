using UnityEngine;

namespace PopupMini
{
    public enum FitMode { Contain, Fill }

    public static class PuzzleCamFitter
    {
        public static void FitOrthoByAspect(Camera cam, RectTransform viewport, float referenceAspect = 16f / 9f,
                                            float referenceOrthoSize = 5f, FitMode fit = FitMode.Contain, float paddingPct = 0.05f)
        {
            if (!cam || !cam.orthographic || !viewport) return;
            var r = viewport.rect; if (r.width <= 1f || r.height <= 1f) return;

            float viewAspect = r.width / r.height;
            float size = referenceOrthoSize;

            if (fit == FitMode.Contain)
            {
                if (viewAspect < referenceAspect) size *= referenceAspect / viewAspect;
            }
            else
            {
                if (viewAspect > referenceAspect) size /= viewAspect / referenceAspect;
            }

            size *= (1f + Mathf.Clamp01(paddingPct));
            cam.orthographicSize = size;
        }

        public static void FitBoundsPerspective(Camera cam, Transform root, RectTransform viewport,
                                                FitMode fit = FitMode.Contain, float paddingPct = 0.1f)
        {
            if (!cam || cam.orthographic || !root || !viewport) return;

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            var bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);

            var rtf = viewport.rect; if (rtf.width <= 1f || rtf.height <= 1f) return;
            float aspect = rtf.width / rtf.height;

            float vFov = cam.fieldOfView * Mathf.Deg2Rad;
            float hFov = 2f * Mathf.Atan(Mathf.Tan(vFov * 0.5f) * aspect);

            float radius = bounds.extents.magnitude;
            if (radius <= 1e-4f) radius = 0.5f;

            float dV = radius / Mathf.Tan(vFov * 0.5f);
            float dH = radius / Mathf.Tan(hFov * 0.5f);
            float dist = (fit == FitMode.Contain) ? Mathf.Max(dV, dH) : Mathf.Min(dV, dH);

            dist *= (1f + Mathf.Clamp01(paddingPct));

            var center = bounds.center;
            var fwd = cam.transform.forward.normalized;
            var newPos = center - fwd * dist;

            cam.transform.position = newPos;
            cam.transform.rotation = Quaternion.LookRotation(center - newPos, cam.transform.up);

            float near = Mathf.Max(0.01f, dist - radius * 2f);
            float far = dist + radius * 4f;
            cam.nearClipPlane = Mathf.Min(cam.nearClipPlane, near);
            cam.farClipPlane = Mathf.Max(cam.farClipPlane, far);
        }
    }
}