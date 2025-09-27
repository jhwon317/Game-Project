using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class FireHP : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    public float hp = 100f;

    [Header("Scale Mapping")]
    [Tooltip("스케일을 적용할 타깃(없으면 이 오브젝트)")]
    public Transform scaleTarget;
    [Tooltip("HP=Max일 때 스케일 1, HP=0일 때 이 배율까지 축소")]
    [Range(0.05f, 1f)] public float minScaleFactor = 0.2f;

    [Header("Auto Despawn")]
    [Tooltip("HP가 0이 되면 자동으로 GameObject를 파괴할지 여부")]
    public bool autoDestroy = true;
    [Tooltip("소멸(파괴) 지연 시간(초)")]
    public float destroyDelay = 0.25f;
    [Tooltip("소멸 직전에 모든 Collider를 비활성화할지 여부")]
    public bool disableCollidersOnOut = true;

    [Header("Optional VFX/SFX")]
    [Tooltip("StopAction을 Destroy로 두면 파티클이 스스로 정리됨")]
    public ParticleSystem flameVfx;
    public ParticleSystem smokeVfx;
    public Light flameLight;

    [Header("Events")]
    public UnityEvent onExtinguished;

    Vector3 _baseScale;
    bool _extinguishedSent = false;
    bool _despawnScheduled = false;

    void Awake()
    {
        if (!scaleTarget) scaleTarget = transform;
        _baseScale = scaleTarget.localScale;
        hp = Mathf.Clamp(hp, 0f, maxHP);
        ApplyVisuals();
    }

    public bool IsOut => hp <= 0.01f;

    /// <summary>
    /// 데미지는 +값(깎일 양)으로 넣어주세요.
    /// </summary>
    public void ApplyDamage(float damage)
    {
        if (damage <= 0f || _despawnScheduled) return;

        hp = Mathf.Max(0f, hp - damage);
        ApplyVisuals();

        if (IsOut && !_extinguishedSent)
        {
            _extinguishedSent = true;
            onExtinguished?.Invoke();
            BeginDespawnSequence();
        }
    }

    void BeginDespawnSequence()
    {
        if (_despawnScheduled) return;
        _despawnScheduled = true;

        // 충돌 비활성
        if (disableCollidersOnOut)
        {
            foreach (var col in GetComponentsInChildren<Collider>(true))
                col.enabled = false;
        }

        // 파티클/라이트 정리
        if (flameVfx) flameVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (smokeVfx) smokeVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (flameLight) flameLight.enabled = false;

        // 스케일 완전 0으로 스냅 (연출을 더 주고 싶으면 코루틴으로 Lerp)
        scaleTarget.localScale = _baseScale * minScaleFactor;

        if (autoDestroy)
        {
            // 파티클이 자식이면 StopAction=Destroy 설정 시 자동 정리됨
            if (destroyDelay <= 0f) Destroy(gameObject);
            else Destroy(gameObject, destroyDelay);
        }
        else
        {
            // autoDestroy가 꺼져있다면 오브젝트만 숨김 처리하고 종료
            gameObject.SetActive(false);
        }
    }

    void ApplyVisuals()
    {
        float t = (maxHP <= 0.001f) ? 0f : (hp / maxHP);
        float s = Mathf.Lerp(minScaleFactor, 1f, t);
        scaleTarget.localScale = _baseScale * s;

        if (flameVfx)
        {
            var em = flameVfx.emission;
            em.rateOverTime = Mathf.Lerp(0f, 200f, t);
            if (t > 0.01f && !flameVfx.isPlaying) flameVfx.Play();
            if (t <= 0.01f && flameVfx.isPlaying) flameVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        if (smokeVfx)
        {
            var em = smokeVfx.emission;
            em.rateOverTime = Mathf.Lerp(20f, 120f, Mathf.Clamp01(t));
            if (t > 0.0f && !smokeVfx.isPlaying) smokeVfx.Play();
            if (t <= 0.0f && smokeVfx.isPlaying) smokeVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        if (flameLight)
        {
            flameLight.intensity = Mathf.Lerp(0f, 6f, t);
            flameLight.enabled = t > 0.01f;
        }
    }
}
