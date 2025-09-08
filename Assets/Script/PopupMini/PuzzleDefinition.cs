using UnityEngine;

namespace PopupMini
{
    [CreateAssetMenu(fileName = "PuzzleDefinition", menuName = "PopupMini/Puzzle Definition", order = 10)]
    public class PuzzleDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string Id;

        [Header("Prefab (must contain Camera + IPuzzleController)")]
        public GameObject Prefab;

        [Header("Viewport")]
        public AspectMode AspectMode = AspectMode.FillCrop;
        public float Aspect = 1f;

        [Header("Session")]
        public bool Modal = true;
        public bool BackdropClosable = false;
        public float TimeoutSec = 0f; // 0=off

        [Header("RenderTexture Quality")]
        public int AntiAliasing = 1;
        public FilterMode FilterMode = FilterMode.Point;
        public Color BackgroundColor = new Color(0, 0, 0, 0);
        public bool ShadowsOff = false;
    }
}