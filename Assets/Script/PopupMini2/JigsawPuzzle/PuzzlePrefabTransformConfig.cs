using UnityEngine;

// ���ӽ����̽��� ���ᰡ ���� �Ͱ� �Ȱ��� �����ִ� �� ����.
// namespace PopupMini 
// {
// �ν����� â���� ������ �� �ֵ��� enum(������)�� ����� ��
public enum RectTransformMode
{
    None,           // �ƹ��͵� �� �� (���� PopupAutoLayout ������ ����)
    FixedPixels,    // ������ �ȼ� ũ��� �߾ӿ� ��ġ
    RawAnchorsOffsets // ��Ŀ�� �������� ���� ���� (���)
}

public class PuzzlePrefabTransformConfig : MonoBehaviour
{
    [Header("���̾ƿ� ���� ��Ʈ")]
    [Tooltip("�� ������Ʈ�� ������ ������ �������� �����մϴ�.")]
    public bool applyTransformHints = true;

    [Tooltip("�˾� ������ ������ ũ��� ��ġ�� �����ϴ� ����Դϴ�.")]
    public RectTransformMode rectMode = RectTransformMode.FixedPixels;

    [Header("��庰 ����")]
    [Tooltip("FixedPixels ����� �� ����� ���� ũ���Դϴ� (����, ����).")]
    public Vector2 fixedPixelSize = new Vector2(1280, 720);

    [Tooltip("RawAnchorsOffsets ����� �� ����� �ּ� ��Ŀ�Դϴ�.")]
    public Vector2 raw_anchorMin = new Vector2(0.1f, 0.1f);
    [Tooltip("RawAnchorsOffsets ����� �� ����� �ִ� ��Ŀ�Դϴ�.")]
    public Vector2 raw_anchorMax = new Vector2(0.9f, 0.9f);

    [Tooltip("RawAnchorsOffsets ����� �� ����� �ּ� �������Դϴ�.")]
    public Vector2 raw_offsetMin = Vector2.zero;
    [Tooltip("RawAnchorsOffsets ����� �� ����� �ִ� �������Դϴ�.")]
    public Vector2 raw_offsetMax = Vector2.zero;

    [Header("���� ������")]
    [Tooltip("���������� ����� ��ü���� ũ�� �����Դϴ�.")]
    [Range(0.1f, 2f)]
    public float contentScale = 1.0f;
}
// }
