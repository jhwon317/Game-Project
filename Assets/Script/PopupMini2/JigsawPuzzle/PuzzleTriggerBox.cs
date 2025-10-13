using UnityEngine;

// IInteractable 규칙을 따른다고 선언
public class PuzzleTriggerBox : MonoBehaviour, IInteractable
{
    // E키를 눌렀을 때
    public void OnInteract(GameObject interactor)
    {
        // 씬에 있는 '총감독' PopupManager를 찾아서 ShowPopup 함수를 호출
        PopupManager popupManager = FindObjectOfType<PopupManager>();
        if (popupManager != null)
        {
            popupManager.ShowPopup();
        }
        else
        {
            Debug.LogError("씬에 PopupManager가 없습니다!");
        }
    }

    public Transform GetTransform() { return transform; }
    public void SetHighlighted(bool on) { /* 하이라이트 로직 */ }
}