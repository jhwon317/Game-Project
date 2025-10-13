using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform correctParent;

    // [핵심 수정!] 모든 방(함수)에서 쓸 수 있도록 '공용 거실'에 변수들을 만듦
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
        // 이제는 새로 만드는 게 아니라, 공용 변수에 값을 저장만 함
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
            // 이제 다른 방에서도 공용 변수를 자유롭게 쓸 수 있음
            transform.SetParent(originalParent);
            transform.position = startPosition;
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }
}