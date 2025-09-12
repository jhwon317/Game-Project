using UnityEngine;

public class StageClearSwitch : MonoBehaviour
{
    // ����Ƽ �����Ϳ��� ������ ���� ����
    public ExitDoor doorToControl;

    // ���ڰ� �ö���� ��
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MovableBox"))
        {
            // ������ "Ȱ��ȭ�Ŷ�!" ��� ��ȣ�� ����
            doorToControl.Activate();
        }
    }

    // ���ڰ� ������ ��
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MovableBox"))
        {
            // ������ "�ٽ� ��Ȱ��ȭ�Ŷ�!" ��� ��ȣ�� ����
            doorToControl.Deactivate();
        }
    }
}
