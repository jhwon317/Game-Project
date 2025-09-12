using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    // �ν����� â���� ���� ����� ����
    public Transform target;

    // ī�޶� �󸶳� �ε巴�� ������ (���� �������� �ε巯��)
    public float smoothSpeed = 0.125f;

    // LateUpdate�� ��� Update�� ���� �Ŀ� ȣ��ż�,
    // ī�޶�ó�� �������� ��ġ�� ��� ��ɿ� ���� ����.
    void LateUpdate()
    {
        if (target != null)
        {
            // ��ǥ ��ġ�� Ÿ���� ��ġ �״��
            Vector3 desiredPosition = target.position;
            // ���� ��ġ���� ��ǥ ��ġ���� �ε巴�� �̵�
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            // ���� ��ġ�� �� ��ġ�� ������Ʈ
            transform.position = smoothedPosition;
        }
    }
}
