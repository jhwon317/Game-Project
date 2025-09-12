using UnityEngine;

public class StageClearSwitch : MonoBehaviour
{
    // 유니티 에디터에서 제어할 문을 연결
    public ExitDoor doorToControl;

    // 상자가 올라왔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MovableBox"))
        {
            // 문에게 "활성화돼라!" 라고 신호를 보냄
            doorToControl.Activate();
        }
    }

    // 상자가 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MovableBox"))
        {
            // 문에게 "다시 비활성화돼라!" 라고 신호를 보냄
            doorToControl.Deactivate();
        }
    }
}
