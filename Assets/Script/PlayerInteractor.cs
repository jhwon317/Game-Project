using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float scanRadius = 2.5f;
    public LayerMask interactableMask;
    public KeyCode interactKey = KeyCode.E;

    IInteractable current;

    void Update()
    {
        current = FindNearest();

        if (current != null && Input.GetKeyDown(interactKey))
        {
            current.OnInteract(gameObject);
        }
    }

    IInteractable FindNearest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, scanRadius, interactableMask);
        IInteractable best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            var candidate = h.GetComponent<IInteractable>();
            if (candidate == null) continue;

            float d = (h.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = candidate;
            }
        }
        return best;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
