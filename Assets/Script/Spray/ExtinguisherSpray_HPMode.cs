using UnityEngine;

[DisallowMultipleComponent]
public class ExtinguisherSpray : MonoBehaviour
{
    [Header("Wiring")]
    public Transform player;   // 분사축: player.forward (카메라 무관)
    public Transform nozzle;   // 분사 원점(손끝/노즐 팁)

    [Header("Input")]
    public KeyCode sprayKey = KeyCode.Mouse1;

    [Header("Spray Shape")]
    [Range(5f, 40f)] public float coneAngleDeg = 24f;
    [Tooltip("레캐스트 최대 사거리(미터). 200까지 허용.")]
    [Range(1f, 200f)] public float effectiveRange = 140f;
    [Range(4, 96)] public int raysPerFrame = 56;
    [Tooltip("SphereCast 반경(미터). 작은 불도 잘 맞도록 0.2~0.4 권장")]
    [Range(0f, 1f)] public float sprayRadius = 0.3f;
    public LayerMask hitMask = ~0;

    [Header("Damage Model (직관적 거리 감쇠)")]
    [Tooltip("이 거리에서 데미지가 절반(1/2)이 됩니다. (미터)")]
    public float halfDamageAtMeters = 8f;
    [Tooltip("감쇠 기울기. 0.7~0.9 완만 / 1.2~1.5 급격")]
    public float alpha = 1.35f;
    [Tooltip("초당 기본 데미지(DPS). 분사 중일 때 적용")]
    public float baseDPS = 110f;
    [Tooltip("근접 과출력을 방지할 최소 거리(미터)")]
    public float minDist = 0.6f;

    [Header("Tank")]
    public bool infiniteTank = true;
    public float tankSecondsTotal = 12f;
    public float tankSecondsLeft = 12f;
    public float flowPerSecond = 1f; // 잔량 차감 전용

    [Header("VFX/SFX (Optional)")]
    public ParticleSystem sprayVfx;
    public AudioSource sprayLoop;

    // 내부
    private RaycastHit[] _hits = new RaycastHit[8];
    private float _k; // halfDamageAtMeters/alpha에서 유도된 k

    public bool IsSpraying =>
        (infiniteTank || tankSecondsLeft > 0.05f) && Input.GetKey(sprayKey);

    void Reset()
    {
        player = transform;
    }

    void Awake()
    {
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
        _k = (Mathf.Pow(2f, 1f / a) - 1f) / H; // k = (2^(1/α)-1)/H
    }

    void Update()
    {
        if (!player || !nozzle)
        {
            StopFx();
            return;
        }

        if (IsSpraying)
        {
            StartFx();
            SprayTick(Time.deltaTime);
        }
        else
        {
            StopFx();
        }
    }

    void SprayTick(float dt)
    {
        Vector3 origin = nozzle.position;
        Vector3 axis = player.forward;

        // 프레임당 총량을 rays로 나눠서 과한 누적 방지
        float perRayDps = baseDPS;

        for (int i = 0; i < raysPerFrame; i++)
        {
            Vector3 dir = SampleCone(axis, coneAngleDeg);

            int cnt = Physics.SphereCastNonAlloc(
                new Ray(origin, dir),
                sprayRadius,
                _hits,
                effectiveRange,
                hitMask,
                QueryTriggerInteraction.Ignore
            );
            if (cnt <= 0) continue;

            // 여러 히트를 순회하며 FireHP에 즉시 적용 (다중 타겟 가능)
            for (int h = 0; h < cnt; h++)
            {
                var hit = _hits[h];
                var fire = hit.collider.GetComponentInParent<FireHP>();
                if (!fire) continue;

                float dist = Mathf.Max(minDist, hit.distance);
                float mult = 1f / Mathf.Pow(1f + _k * dist, Mathf.Max(0.001f, alpha));
                float damage = perRayDps * mult * dt;

                fire.ApplyDamage(damage);
            }
        }

        if (!infiniteTank)
        {
            tankSecondsLeft = Mathf.Max(0f, tankSecondsLeft - flowPerSecond * dt);
            if (tankSecondsLeft <= 0.05f) StopFx();
        }
    }

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

    void StartFx()
    {
        if (sprayVfx && !sprayVfx.isPlaying) sprayVfx.Play();
        if (sprayLoop && !sprayLoop.isPlaying) sprayLoop.Play();
    }

    void StopFx()
    {
        if (sprayVfx && sprayVfx.isPlaying) sprayVfx.Stop();
        if (sprayLoop && sprayLoop.isPlaying) sprayLoop.Stop();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!player || !nozzle) return;
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.25f);
        Vector3 origin = nozzle.position;
        Vector3 dir = player.forward;
        Gizmos.DrawLine(origin, origin + dir * effectiveRange);

        // Cone edge disc
        float r = Mathf.Tan(coneAngleDeg * Mathf.Deg2Rad) * effectiveRange;
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawWireDisc(origin + dir * effectiveRange, dir, r);

        // Spray radius hint
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.25f);
        UnityEditor.Handles.DrawWireDisc(origin, dir, sprayRadius);
    }
#endif
}
