using UnityEngine;

namespace PopupMini
{
    public class ViewportService
    {
        readonly CamToRawImage _binder;
        public ViewportService(CamToRawImage binder) { _binder = binder; }

        public void Bind(Camera cam, PuzzleDefinition def)
        {
            if (!_binder) return;
            _binder.Bind(cam,
                def.AspectMode,
                Mathf.Approximately(def.Aspect, 0f) ? 1f : def.Aspect,
                Mathf.Max(1, def.AntiAliasing),
                def.FilterMode,
                def.BackgroundColor);
        }

        public void Unbind() { if (_binder) _binder.Unbind(); }
    }
}