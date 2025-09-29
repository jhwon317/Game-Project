using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Drag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public Transform correctParent; // 내가 원래 있어야 할 정답 위치

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
        transform.SetParent(transform.root); // 드래그하는 동안 최상위로
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 만약 정답 위치와 충분히 가깝다면
        if (Vector3.Distance(transform.position, correctParent.position) < 100f)
        {
            transform.SetParent(correctParent);
            transform.position = correctParent.position;
            image.raycastTarget = false; // 더 이상 드래그 안 되게

            GetComponentInParent<JigsawPuzzleController>().PiecePlaced();
        }
        else
        {
            // 정답 위치가 아니면 원래 있던 자리로 되돌아감
            transform.SetParent(originalParent);
            transform.position = startPosition;
        }
    }
}