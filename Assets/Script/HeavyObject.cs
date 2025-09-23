using UnityEngine;

// �� ��ũ��Ʈ�� '���ſ�' �Ӽ��� �ο��ϴ� ���Ҹ� �մϴ�.
public class HeavyObject : MonoBehaviour
{
    [Header("���� ����")]
    [Tooltip("�� ������ ����� �� �÷��̾� �ӵ��� �󸶳� �پ���� ���մϴ�. (��: 0.8 = 80% �ӵ�)")]
    [Range(0.1f, 1f)]
    public float speedModifier = 0.8f; // �⺻���� 80% �ӵ�
}
