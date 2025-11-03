using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 순수 분사 물리 로직만 담당. 방향과 deltaTime을 받아서 데미지 적용
/// </summary>
public class SprayEmitter : MonoBehaviour
{
    [Header("Nozzle")]
    [Tooltip("분사 시작 지점")]
    public Transform nozzle;

    [Header("Spray Shape")]
    [Range(5f, 40f)] public float coneAngleDeg = 24f;
    [Tooltip("최대 유효 거리(센티미터)")]
    [Range(1f, 200f)] public float effectiveRange = 140f;
    [Tooltip("프레임당 발사할 광선 개수. 성능 최적화를 위해 16으로 설정")]
    [Range(4, 24)] public int raysPerFrame = 16;
    [Tooltip("SphereCast 반경(센티미터)")]
    [Range(0f, 1f)] public float sprayRadius = 0.3f;
    public LayerMask hitMask = ~0;

    [Header("Damage Model")]
    [Tooltip("초당 기본 데미지")]
    public float baseDPS = 110f;
    [Tooltip("이 거리에서 데미지가 절반이 됨")]
    public float halfDamageAtMeters = 8f;
    [Tooltip("감쇠 곡선 (높을수록 급격)")]
    public float alpha = 1.35f;
    [Tooltip("최소 거리 (너무 가까우면 데미지 고정)")]
    public float minDist = 0.6f;

    [Header("VFX")]
    public ParticleSystem sprayVfx;

    // 캐시
    private RaycastHit[] _hits = new RaycastHit[8];
    private float _k; // 감쇠 계수
    private Dictionary<FireHP, float> _damageAccumulator = new Dictionary<FireHP, float>();

    void Awake()
    {
        if (!nozzle) nozzle = transform;
        RecomputeFalloff();
    }

    void OnValidate()
    {
        RecomputeFalloff();
    }

    void RecomputeFalloff()
    {
        float a = Mathf.Max(0.001f, alpha);
        float H = Mathf.Max(0.001f, halfDamageAtMeters);
        _k = (Mathf.Pow(2f, 1f / a) - 1f) / H;
    }

    /// <summary>
    /// 주어진 방향으로 분사. 매 프레임 호출됨.
    /// </summary>
    public void Spray(Vector3 direction, float deltaTime)
    {
        if (!nozzle) return;

        // VFX 시작
        if (sprayVfx && !sprayVfx.isPlaying) sprayVfx.Play();

        Vector3 origin = nozzle.position;
        float perRayDps = baseDPS;

        // 데미지 누적기 초기화 (같은 프레임에 같은 불을 여러번 맞춰도 한번만 적용)
        _damageAccumulator.Clear();

        // 여러 광선 발사
        for (int i = 0; i < raysPerFrame; i++)
        {
            Vector3 dir = SampleCone(direction, coneAngleDeg);

            int cnt = Physics.SphereCastNonAlloc(
                new Ray(origin, dir),
                sprayRadius,
                _hits,
                effectiveRange,
                hitMask,
                QueryTriggerInteraction.Ignore
            );

            for (int h = 0; h < cnt; h++)
            {
                var hit = _hits[h];
                var fire = hit.collider.GetComponentInParent<FireHP>();
                if (!fire || fire.IsOut) continue;

                // 거리 기반 데미지 감쇠
                float dist = Mathf.Max(minDist, hit.distance);
                float mult = 1f / Mathf.Pow(1f + _k * dist, alpha);
                float damage = perRayDps * mult * deltaTime;

                // 누적 (같은 불에 여러 ray가 맞으면 합산)
                if (_damageAccumulator.ContainsKey(fire))
                    _damageAccumulator[fire] += damage;
                else
                    _damageAccumulator[fire] = damage;
            }
        }

        // 한번에 데미지 적용
        foreach (var pair in _damageAccumulator)
        {
            pair.Key.ApplyDamage(pair.Value);
        }
    }

    /// <summary>
    /// VFX만 정지
    /// </summary>
    public void StopVFX()
    {
        if (sprayVfx && sprayVfx.isPlaying)
            sprayVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    /// <summary>
    /// 원뿔 내에서 랜덤 방향 샘플링
    /// </summary>
    Vector3 SampleCone(Vector3 axis, float coneDeg)
    {
        float theta = Random.Range(0f, coneDeg) * Mathf.Deg2Rad;
        float phi = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        Vector3 a = axis.normalized;
        Vector3 ortho = Vector3.Cross(a, Vector3.up);
        if (ortho.sqrMagnitude < 1e-4f) ortho = Vector3.Cross(a, Vector3.right);
        ortho.Normalize();
        Vector3 ortho2 = Vector3.Cross(a, ortho);

        Vector3 dir = (a * Mathf.Cos(theta))
                    + (ortho * Mathf.Sin(theta) * Mathf.Cos(phi))
                    + (ortho2 * Mathf.Sin(theta) * Mathf.Sin(phi));
        return dir.normalized;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!nozzle) return;
        
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.3f);
        Vector3 origin = nozzle.position;
        Vector3 fwd = transform.parent ? transform.parent.forward : transform.forward;
        
        // 중심선
        Gizmos.DrawLine(origin, origin + fwd * effectiveRange);
        
        // 원뿔 끝 원
        float r = Mathf.Tan(coneAngleDeg * Mathf.Deg2Rad) * effectiveRange;
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawWireDisc(origin + fwd * effectiveRange, fwd, r);
        
        // SphereCast 반경
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
        UnityEditor.Handles.DrawWireDisc(origin, fwd, sprayRadius);
    }
#endif
}
