using UnityEngine;

namespace PopupMini
{
    [CreateAssetMenu(menuName = "PopupMini2/PuzzleDefinition")]
    public class PuzzleDefinition : ScriptableObject
    {
        [Header("Prefab")]
        public GameObject Prefab;

        [Header("Viewport")]
        public AspectMode AspectMode = AspectMode.FitContain;
        public float Aspect = 0f;   // 0 = ÀÚÀ¯
        public int AntiAliasing = 1;
        public FilterMode FilterMode = FilterMode.Bilinear;
        public Color BackgroundColor = new Color(0, 0, 0, 0);

        [Header("Session")]
        public bool Modal = true;
        public bool BackdropClosable = false;
        public float TimeoutSec = 0f;

        [Header("Perf")]
        public bool ShadowsOff = true;
    }
}
