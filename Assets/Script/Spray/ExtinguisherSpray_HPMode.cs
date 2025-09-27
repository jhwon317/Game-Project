using UnityEngine;

[DisallowMultipleComponent]
public class ExtinguisherSpray : MonoBehaviour
{
    [Header("Wiring")]
    public Transform player;   // �л���: player.forward (ī�޶� ����)
    public Transform nozzle;   // �л� ����(�ճ�/���� ��)

    [Header("Input")]
    public KeyCode sprayKey = KeyCode.Mouse1;

    [Header("Spray Shape")]
    [Range(5f, 40f)] public float coneAngleDeg = 24f;
    [Tooltip("��ĳ��Ʈ �ִ� ��Ÿ�(����). 200���� ���.")]
    [Range(1f, 200f)] public float effectiveRange = 140f;
    [Range(4, 96)] public int raysPerFrame = 56;
    [Tooltip("SphereCast �ݰ�(����). ���� �ҵ� �� �µ��� 0.2~0.4 ����")]
    [Range(0f, 1f)] public float sprayRadius = 0.3f;
    public LayerMask hitMask = ~0;

    [Header("Damage Model (������ �Ÿ� ����)")]
    [Tooltip("�� �Ÿ����� �������� ����(1/2)�� �˴ϴ�. (����)")]
    public float halfDamageAtMeters = 8f;
    [Tooltip("���� ����. 0.7~0.9 �ϸ� / 1.2~1.5 �ް�")]
    public float alpha = 1.35f;
    [Tooltip("�ʴ� �⺻ ������(DPS). �л� ���� �� ����")]
    public float baseDPS = 110f;
    [Tooltip("���� ������� ������ �ּ� �Ÿ�(����)")]
    public float minDist = 0.6f;

    [Header("Tank")]
    public bool infiniteTank = true;
    public float tankSecondsTotal = 12f;
    public float tankSecondsLeft = 12f;
    public float flowPerSecond = 1f; // �ܷ� ���� ����

    [Header("VFX/SFX (Optional)")]
    public ParticleSystem sprayVfx;
    public AudioSource sprayLoop;

    // ����
    private RaycastHit[] _hits = new RaycastHit[8];
    private float _k; // halfDamageAtMeters/alpha���� ������ k

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
        _k = (Mathf.Pow(2f, 1f / a) - 1f) / H; // k = (2^(1/��)-1)/H
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

        // �����Ӵ� �ѷ��� rays�� ������ ���� ���� ����
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

            // ���� ��Ʈ�� ��ȸ�ϸ� FireHP�� ��� ���� (���� Ÿ�� ����)
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
