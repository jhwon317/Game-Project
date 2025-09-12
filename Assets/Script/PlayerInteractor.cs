using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Scan")]
    public float scanRadius = 2.5f;
    public LayerMask interactableMask;

    // [핵심 수정!] 'private'를 'public'으로 바꾸고,
    // 외부에서는 값을 바꿀 수 없도록 { get; private set; }을 붙여줌
    public IInteractable currentInteractable { get; private set; }

    // private PlayerController playerController; // 이제 PlayerController를 알 필요 없음

    // void Awake() // Awake도 필요 없어짐
    // {
    //     playerController = GetComponent<PlayerController>();
    // }

    void Update()
    {
        // Interactor는 정찰만 담당
        FindNearestInteractable();
    }

    void FindNearestInteractable()
    {
        var nearest = SearchForInteractable();

        // 가장 가까운 대상이 바뀌었는지 확인하고 하이라이트 처리
        if (nearest != currentInteractable)
        {
            if (currentInteractable != null) currentInteractable.SetHighlighted(false);
            currentInteractable = nearest;
            if (currentInteractable != null) currentInteractable.SetHighlighted(true);
        }
    }

    IInteractable SearchForInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, scanRadius, interactableMask);
        float bestDistance = float.MaxValue;
        IInteractable nearestPick = null;
        Vector3 myPositionOnGround = new Vector3(transform.position.x, 0, transform.position.z);
        foreach (var hitCollider in hits)
        {
            IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;
            Transform targetTransform = interactable.GetTransform();
            Vector3 targetPositionOnGround = new Vector3(targetTransform.position.x, 0, targetTransform.position.z);
            float distance = Vector3.Distance(myPositionOnGround, targetPositionOnGround);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearestPick = interactable;
            }
        }
        return nearestPick;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}