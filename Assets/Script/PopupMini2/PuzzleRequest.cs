namespace PopupMini
{
    public struct PopupSessionOptions
    {
        public bool Modal;
        public bool BackdropClosable;
        public float TimeoutSec;
    }

    public struct PopupUIOverride
    {
        public UnityEngine.GameObject PanelRoot;
        public UnityEngine.RectTransform ContentRoot;
        public CamToRawImage Viewport;
        public UnityEngine.CanvasGroup CanvasGroup;
    }

    public class PuzzleRequest
    {
        public PuzzleDefinition Definition;
        public string Args;             // JSON/string (nullable)
        public PopupUIOverride UIOverride;       // (선택)
        public PopupSessionOptions SessionOverride;  // (선택)
    }
}