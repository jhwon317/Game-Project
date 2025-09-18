using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform correctParent;

    [Tooltip("이 거리 안으로 들어와야 정답으로 인정됩니다.")]
    public float snapDistance = 150f; // [핵심 수정!] 판정 거리를 150으로 늘리고, 인스펙터에서 조절 가능하게 함

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

        // [탐정!] 현재 거리와 판정 거리를 콘솔에 보고
        Debug.Log($"'{name}' 조각 놓음! 정답과의 거리: {distance} (판정 기준: {snapDistance} 미만)");

        // 만약 정답 위치와 충분히 가깝다면
        if (distance < snapDistance)
        {
            transform.SetParent(correctParent);
            transform.position = correctParent.position;
            enabled = false;
            FindObjectOfType<JigsawPuzzleController>().PiecePlaced();
        }
        else
        {
            // 정답 위치가 아니면 원래 있던 자리로 되돌아감
            transform.SetParent(originalParent);
            transform.position = startPosition;
        }
    }
}