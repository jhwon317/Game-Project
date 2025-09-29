using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Drag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public Transform correctParent; // ���� ���� �־�� �� ���� ��ġ

    private Vector3 startPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private Image image;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        startPosition = transform.position;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root); // �巡���ϴ� ���� �ֻ�����
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // ���� ���� ��ġ�� ����� �����ٸ�
        if (Vector3.Distance(transform.position, correctParent.position) < 100f)
        {
            transform.SetParent(correctParent);
            transform.position = correctParent.position;
            image.raycastTarget = false; // �� �̻� �巡�� �� �ǰ�

            GetComponentInParent<JigsawPuzzleController>().PiecePlaced();
        }
        else
        {
            // ���� ��ġ�� �ƴϸ� ���� �ִ� �ڸ��� �ǵ��ư�
            transform.SetParent(originalParent);
            transform.position = startPosition;
        }
    }
}