using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Scan")]
    public float scanRadius = 2.5f;
    public LayerMask interactableMask;

    // [�ٽ� ����!] 'private'�� 'public'���� �ٲٰ�,
    // �ܺο����� ���� �ٲ� �� ������ { get; private set; }�� �ٿ���
    public IInteractable currentInteractable { get; private set; }

    // private PlayerController playerController; // ���� PlayerController�� �� �ʿ� ����

    // void Awake() // Awake�� �ʿ� ������
    // {
    //     playerController = GetComponent<PlayerController>();
    // }

    void Update()
    {
        // Interactor�� ������ ���
        FindNearestInteractable();
    }

    void FindNearestInteractable()
    {
        var nearest = SearchForInteractable();

        // ���� ����� ����� �ٲ������ Ȯ���ϰ� ���̶���Ʈ ó��
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