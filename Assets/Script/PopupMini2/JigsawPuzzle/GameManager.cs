using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject puzzlePanel; // 인스펙터에서 퍼즐 프리팹을 연결
    public MonoBehaviour playerMoveScript; // 플레이어의 이동 스크립트

    // 퍼즐을 시작하는 함수
    public void ShowPuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true); // 퍼즐 UI 켜기
        }
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = false; // 플레이어 조작 비활성화
        }
        Time.timeScale = 0f; // 게임 시간을 멈춤 (일시정지)
        Cursor.visible = true; // 마우스 커서 보이기
        Cursor.lockState = CursorLockMode.None;
    }

    // 퍼즐을 끝내는 함수
    public void HidePuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false); // 퍼즐 UI 끄기
        }
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = true; // 플레이어 조작 활성화
        }
        Time.timeScale = 1f; // 게임 시간 다시 흐르게
        Cursor.visible = false; // 마우스 커서 숨기기
        Cursor.lockState = CursorLockMode.Locked;
    }
}