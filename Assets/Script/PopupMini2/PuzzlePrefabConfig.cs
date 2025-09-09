using UnityEngine;

namespace PopupMini
{
    [DisallowMultipleComponent]
    public class PuzzlePrefabConfig : MonoBehaviour
    {
        [Header("�⺻ Args(JSON)")]
        public TextAsset defaultArgsJson;

        [Header("�˾� ���̾ƿ� ��Ʈ (Host�� PopupAutoLayout�� ����)")]
        public bool applyLayoutHints = true;

        public bool usePadding = true;
        public PaddingMode paddingMode = PaddingMode.PixelsUI;     // PixelsUI or PercentOfPanel
        public Vector4 padding = new Vector4(32, 32, 40, 40);      // L,R,T,B (UI px)
        public Vector4 paddingPercent = new Vector4(0.05f, 0.05f, 0.05f, 0.05f); // L,R,T,B (0~1)

        [Min(0.01f)] public float contentScale = 1.0f;             // ContentRoot.localScale (s,s,1)

        [Header("ī�޶� �ڵ����� ��Ʈ")]
        public bool autoFitCamera = true;
        public FitMode fitMode = FitMode.Contain;                   // Contain or Fill
        public float referenceAspect = 16f / 9f;                      // Ortho�� ���� ����
        public float referenceOrthoSize = 5f;                       // Ortho ���� �ݳ���
        [Range(0, 0.4f)] public float cameraPaddingPct = 0.08f;
    }
}