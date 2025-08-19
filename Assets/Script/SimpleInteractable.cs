using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleInteractable : MonoBehaviour, IInteractable
{
    public string displayName = "Object";
    public Highlighter highlighter;

    protected virtual void Awake()
    {
        if (!highlighter) highlighter = GetComponent<Highlighter>();
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer != -1) gameObject.layer = layer;
    }

    public Transform GetTransform() => transform;

    public void OnInteract(GameObject interactor)
    {
        Debug.Log($"[Interact] {interactor.name} ¡æ {displayName}");
    }

    public void SetHighlighted(bool on)
    {
        if (highlighter) highlighter.SetHighlight(on);
    }
}
