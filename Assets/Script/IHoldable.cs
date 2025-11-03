using UnityEngine;

/// <summary>
/// 플레이어가 들 수 있는 모든 아이템의 공통 인터페이스
/// </summary>
public interface IHoldable
{
    /// <summary>
    /// 아이템의 Transform
    /// </summary>
    Transform GetTransform();

    /// <summary>
    /// 플레이어가 아이템을 들었을 때 호출
    /// </summary>
    void OnPickedUp(Transform holdPoint);

    /// <summary>
    /// 플레이어가 E키를 눌러 아이템을 놓을 때 호출
    /// </summary>
    void OnPutDown(Vector3 dropPosition, Vector3 playerForward);

    /// <summary>
    /// 이동속도 감소 배율 (1.0 = 감소 없음, 0.7 = 30% 감소)
    /// </summary>
    float GetSpeedModifier();
}
