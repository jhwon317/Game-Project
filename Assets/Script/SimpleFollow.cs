using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    // 인스펙터 창에서 따라갈 대상을 지정
    public Transform target;

    // 카메라가 얼마나 부드럽게 따라갈지 (값이 작을수록 부드러움)
    public float smoothSpeed = 0.125f;

    // LateUpdate는 모든 Update가 끝난 후에 호출돼서,
    // 카메라처럼 마지막에 위치를 잡는 기능에 쓰면 좋아.
    void LateUpdate()
    {
        if (target != null)
        {
            // 목표 위치는 타겟의 위치 그대로
            Vector3 desiredPosition = target.position;
            // 현재 위치에서 목표 위치까지 부드럽게 이동
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            // 계산된 위치로 내 위치를 업데이트
            transform.position = smoothedPosition;
        }
    }
}
