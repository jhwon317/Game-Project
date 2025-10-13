using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public GameObject popupSessionCanvas; // �ܺ� â��(PopupSessionCanvas)
    public MonoBehaviour playerMoveScript;   // �÷��̾� �̵� ��ũ��Ʈ

    void Start()
    {
        popupSessionCanvas.SetActive(false); // ������ �� ����
    }

    public void ShowPopup()
    {
        Time.timeScale = 0f; // ���� �ð� ����
        playerMoveScript.enabled = false;
        popupSessionCanvas.SetActive(true);
    }

    public void HidePopup()
    {
        popupSessionCanvas.SetActive(false);
        playerMoveScript.enabled = true;
        Time.timeScale = 1f; // ���� �ð� �ٽ� �帣��
    }
}