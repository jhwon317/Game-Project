using UnityEngine;

/// <summary>
/// 소화기 분사 제어. 탱크 관리, 입력 처리, 오디오 제어
/// PlayerController에서 TrySpraying()을 호출해서 사용
/// </summary>
[DisallowMultipleComponent]
public class ExtinguisherController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("분사 방향 기준 (보통 플레이어)")]
    public Transform player;
    [Tooltip("실제 분사 물리를 담당하는 컴포넌트")]
    public SprayEmitter emitter;

    [Header("Tank")]
    [Tooltip("탱크 최대 용량 (초)")]
    public float tankMax = 12f;
    [Tooltip("현재 남은 용량 (초)")]
    public float tankCurrent = 12f;
    [Tooltip("초당 소모량")]
    public float flowRate = 1f;

    [Header("Audio")]
    public AudioSource sprayLoop;

    private bool _isSpraying = false;

    public bool CanSpray => tankCurrent > 0.05f;
    public float TankPercent => tankMax > 0 ? Mathf.Clamp01(tankCurrent / tankMax) : 0f;
    public bool IsSpraying => _isSpraying;

    void Awake()
    {
        // 자동 참조 설정
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!emitter)
            emitter = GetComponentInChildren<SprayEmitter>();

        tankCurrent = tankMax;
    }

    void OnDisable()
    {
        // 비활성화되면 분사 중지
        StopSpraying();
    }

    /// <summary>
    /// 분사 시도. PlayerController에서 매 프레임 호출
    /// </summary>
    public void TrySpraying(float deltaTime)
    {
        Debug.Log("[ExtinguisherController] TrySpraying called");
        if (!CanSpray || !emitter || !player)
        {
            StopSpraying();
            return;
        }

        // 분사 시작
        if (!_isSpraying)
        {
            _isSpraying = true;
            if (sprayLoop && !sprayLoop.isPlaying)
                sprayLoop.Play();
        }

        Debug.Log("[ExtinguisherController] Spraying...");
        // 실제 분사 (SprayEmitter에게 위임)
        emitter.Spray(player.forward, deltaTime);

        // 탱크 소모
        tankCurrent = Mathf.Max(0f, tankCurrent - flowRate * deltaTime);

        // 탱크 고갈 시 자동 정지
        if (tankCurrent <= 0.05f)
            StopSpraying();
    }

    /// <summary>
    /// 분사 중지
    /// </summary>
    public void StopSpraying()
    {
        if (!_isSpraying) return;
        _isSpraying = false;

        if (emitter) emitter.StopVFX();
        if (sprayLoop && sprayLoop.isPlaying) sprayLoop.Stop();
    }

    /// <summary>
    /// 탱크 리필 (디버그/치트용)
    /// </summary>
    public void Refill()
    {
        tankCurrent = tankMax;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!player) return;

        // 분사 방향 표시
        Gizmos.color = _isSpraying ? Color.cyan : Color.gray;
        Gizmos.DrawRay(transform.position, player.forward * 2f);
    }
#endif
}
