using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform correctParent;

    // [�ٽ� ����!] ��� ��(�Լ�)���� �� �� �ֵ��� '���� �Ž�'�� �������� ����
    private Vector3 startPosition;
    private Transform originalParent;
    private int originalSiblingIndex;

    private CanvasGroup canvasGroup;
    private Image image;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ������ ���� ����� �� �ƴ϶�, ���� ������ ���� ���常 ��
        originalParent = transform.parent;
        startPosition = transform.position;
        originalSiblingIndex = transform.GetSiblingIndex();

        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (Vector3.Distance(transform.position, correctParent.position) < 100f)
        {
            transform.SetParent(correctParent);
            transform.position = correctParent.position;
            image.raycastTarget = false;

            GetComponentInParent<JigsawPuzzleController>().PiecePlaced();
        }
        else
        {
            // ���� �ٸ� �濡���� ���� ������ �����Ӱ� �� �� ����
            transform.SetParent(originalParent);
            transform.position = startPosition;
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }
}