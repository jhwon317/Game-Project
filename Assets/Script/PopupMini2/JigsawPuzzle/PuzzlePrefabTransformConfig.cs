using UnityEngine;

// 네임스페이스는 동료가 쓰는 것과 똑같이 맞춰주는 게 좋아.
// namespace PopupMini 
// {
// 인스펙터 창에서 선택할 수 있도록 enum(선택지)을 만들어 줌
public enum RectTransformMode
{
    None,           // 아무것도 안 함 (씬의 PopupAutoLayout 설정을 따름)
    FixedPixels,    // 고정된 픽셀 크기로 중앙에 배치
    RawAnchorsOffsets // 앵커와 오프셋을 직접 제어 (고급)
}

public class PuzzlePrefabTransformConfig : MonoBehaviour
{
    [Header("레이아웃 적용 힌트")]
    [Tooltip("이 컴포넌트의 설정을 실제로 적용할지 결정합니다.")]
    public bool applyTransformHints = true;

    [Tooltip("팝업 내에서 퍼즐의 크기와 위치를 결정하는 방식입니다.")]
    public RectTransformMode rectMode = RectTransformMode.FixedPixels;

    [Header("모드별 설정")]
    [Tooltip("FixedPixels 모드일 때 사용할 고정 크기입니다 (가로, 세로).")]
    public Vector2 fixedPixelSize = new Vector2(1280, 720);

    [Tooltip("RawAnchorsOffsets 모드일 때 사용할 최소 앵커입니다.")]
    public Vector2 raw_anchorMin = new Vector2(0.1f, 0.1f);
    [Tooltip("RawAnchorsOffsets 모드일 때 사용할 최대 앵커입니다.")]
    public Vector2 raw_anchorMax = new Vector2(0.9f, 0.9f);

    [Tooltip("RawAnchorsOffsets 모드일 때 사용할 최소 오프셋입니다.")]
    public Vector2 raw_offsetMin = Vector2.zero;
    [Tooltip("RawAnchorsOffsets 모드일 때 사용할 최대 오프셋입니다.")]
    public Vector2 raw_offsetMax = Vector2.zero;

    [Header("공통 스케일")]
    [Tooltip("최종적으로 적용될 전체적인 크기 배율입니다.")]
    [Range(0.1f, 2f)]
    public float contentScale = 1.0f;
}
// }
