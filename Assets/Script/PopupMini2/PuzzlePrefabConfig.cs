using UnityEngine;

namespace PopupMini
{
    [DisallowMultipleComponent]
    public class PuzzlePrefabConfig : MonoBehaviour
    {
        [Header("기본 Args(JSON)")]
        public TextAsset defaultArgsJson;

        [Header("팝업 레이아웃 힌트 (Host의 PopupAutoLayout에 주입)")]
        public bool applyLayoutHints = true;

        public bool usePadding = true;
        public PaddingMode paddingMode = PaddingMode.PixelsUI;     // PixelsUI or PercentOfPanel
        public Vector4 padding = new Vector4(32, 32, 40, 40);      // L,R,T,B (UI px)
        public Vector4 paddingPercent = new Vector4(0.05f, 0.05f, 0.05f, 0.05f); // L,R,T,B (0~1)

        [Min(0.01f)] public float contentScale = 1.0f;             // ContentRoot.localScale (s,s,1)

        [Header("카메라 자동맞춤 힌트")]
        public bool autoFitCamera = true;
        public FitMode fitMode = FitMode.Contain;                   // Contain or Fill
        public float referenceAspect = 16f / 9f;                      // Ortho용 기준 비율
        public float referenceOrthoSize = 5f;                       // Ortho 기준 반높이
        [Range(0, 0.4f)] public float cameraPaddingPct = 0.08f;
    }
}