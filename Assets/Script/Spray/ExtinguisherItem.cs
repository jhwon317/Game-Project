using UnityEngine;

/// <summary>
/// 소화기 아이템 - 모드 전용
/// PlayerController가 직접 제어하므로 BePickedUp 등 불필요
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ExtinguisherItem : MonoBehaviour
{
    [Header("Spray System")]
    public ExtinguisherController controller;

    [Header("Components (Optional)")]
    public Rigidbody rb;
    public Collider itemCollider;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!itemCollider) itemCollider = GetComponent<Collider>();
        if (!controller) controller = GetComponentInChildren<ExtinguisherController>();

        // 처음엔 비활성화
        if (controller) controller.enabled = false;
    }

    void OnDestroy()
    {
        // 파괴될 때 분사 중지
        if (controller)
        {
            controller.StopSpraying();
        }
    }
}
