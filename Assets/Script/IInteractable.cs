using UnityEngine;

public interface IInteractable
{
    Transform GetTransform();
    void OnInteract(GameObject interactor);
    void SetHighlighted(bool on);
}
