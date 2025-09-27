using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform correctParent;

    [Tooltip("�� �Ÿ� ������ ���;� �������� �����˴ϴ�.")]
    public float snapDistance = 150f; // [�ٽ� ����!] ���� �Ÿ��� 150���� �ø���, �ν����Ϳ��� ���� �����ϰ� ��

    private Vector3 startPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        float distance = Vector3.Distance(transform.position, correctParent.position);

        // [Ž��!] ���� �Ÿ��� ���� �Ÿ��� �ֿܼ� ����
        Debug.Log($"'{name}' ���� ����! ������� �Ÿ�: {distance} (���� ����: {snapDistance} �̸�)");

        // ���� ���� ��ġ�� ����� �����ٸ�
        if (distance < snapDistance)
        {
            transform.SetParent(correctParent);
            transform.position = correctParent.position;
            enabled = false;
            FindObjectOfType<JigsawPuzzleController>().PiecePlaced();
        }
        else
        {
            // ���� ��ġ�� �ƴϸ� ���� �ִ� �ڸ��� �ǵ��ư�
            transform.SetParent(originalParent);
            transform.position = startPosition;
        }
    }
}