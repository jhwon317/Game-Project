using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject puzzlePanel; // �ν����Ϳ��� ���� �������� ����
    public MonoBehaviour playerMoveScript; // �÷��̾��� �̵� ��ũ��Ʈ

    // ������ �����ϴ� �Լ�
    public void ShowPuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true); // ���� UI �ѱ�
        }
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = false; // �÷��̾� ���� ��Ȱ��ȭ
        }
        Time.timeScale = 0f; // ���� �ð��� ���� (�Ͻ�����)
        Cursor.visible = true; // ���콺 Ŀ�� ���̱�
        Cursor.lockState = CursorLockMode.None;
    }

    // ������ ������ �Լ�
    public void HidePuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false); // ���� UI ����
        }
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = true; // �÷��̾� ���� Ȱ��ȭ
        }
        Time.timeScale = 1f; // ���� �ð� �ٽ� �帣��
        Cursor.visible = false; // ���콺 Ŀ�� �����
        Cursor.lockState = CursorLockMode.Locked;
    }
}