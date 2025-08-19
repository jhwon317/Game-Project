using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleInteractable : MonoBehaviour, IInteractable
{
    public string displayName = "Object";

    public void OnInteract(GameObject interactor)
    {
        Debug.Log($"[Interact] {interactor.name} ¡æ {displayName}");
    }
}
