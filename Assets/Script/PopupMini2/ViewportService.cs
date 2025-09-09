using UnityEngine;

namespace PopupMini
{
    public class ViewportService
    {
        readonly CamToRawImage _view;

        public ViewportService(CamToRawImage view) { _view = view; }

        public void Bind(Camera cam, PuzzleDefinition def)
        {
            if (!_view || !cam || !def) return;
            _view.Bind(cam, def.AspectMode, def.Aspect, def.AntiAliasing, def.FilterMode, def.BackgroundColor);
        }

        public void Unbind()
        {
            if (_view) _view.Unbind();
        }
    }
}