using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Scan")]
    public float scanRadius = 2.5f;
    public LayerMask interactableMask;
    public float verticalTolerance = 1.2f; // Y 높이 차 허용(벨트스크롤용)

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;

    IInteractable current;

    void Update()
    {
        var nearest = FindNearest();

        if (nearest != current)
        {
            if (current != null) current.SetHighlighted(false);
            current = nearest;
            if (current != null) current.SetHighlighted(true);
        }

        if (current != null && Input.GetKeyDown(interactKey))
            current.OnInteract(gameObject);
    }

    IInteractable FindNearest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, scanRadius, interactableMask, QueryTriggerInteraction.Collide);

        float best = float.MaxValue;
        IInteractable pick = null;

        foreach (var h in hits)
        {
            // 너무 높은/낮은 건 제외
            if (Mathf.Abs(h.transform.position.y - transform.position.y) > verticalTolerance) continue;

            var it = h.GetComponentInParent<IInteractable>();
            if (it == null) continue;

            float d = (it.GetTransform().position - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                pick = it;
            }
        }
        return pick;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
