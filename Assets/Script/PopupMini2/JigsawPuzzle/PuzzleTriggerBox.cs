using UnityEngine;

// IInteractable ��Ģ�� �����ٰ� ����
public class PuzzleTriggerBox : MonoBehaviour, IInteractable
{
    // EŰ�� ������ ��
    public void OnInteract(GameObject interactor)
    {
        // ���� �ִ� '�Ѱ���' PopupManager�� ã�Ƽ� ShowPopup �Լ��� ȣ��
        PopupManager popupManager = FindObjectOfType<PopupManager>();
        if (popupManager != null)
        {
            popupManager.ShowPopup();
        }
        else
        {
            Debug.LogError("���� PopupManager�� �����ϴ�!");
        }
    }

    public Transform GetTransform() { return transform; }
    public void SetHighlighted(bool on) { /* ���̶���Ʈ ���� */ }
}