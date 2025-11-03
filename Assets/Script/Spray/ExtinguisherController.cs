using UnityEngine;
using System;
using System.Linq;

public class ExtinguisherController : MonoBehaviour
{
    [Header("References")]
    public Transform player;                 // 선택(없어도 됨)
    public SprayEmitter emitter;             // ★ 목표: 절대 null 안 되게
    public Transform nozzle;                 // 분사 원점/방향(없으면 자동 추적)

    [Tooltip("이름 단서(타입명이 다를 때 반사 탐색)")]
    public string emitterTypeName = "SprayEmitter";
    [Tooltip("노즐 오브젝트 이름 단서(포함 일치)")]
    public string nozzleNameHint = "nozzle";

    [Header("Tank/Flow")]
    public float tankMax = 12f;
    public float tankCurrent = 12f;
    public float flowRate = 1f;

    [Header("Audio/VFX (optional)")]
    public ParticleSystem sprayLoop;
    public AudioSource spraySfx;

    // UI 호환
    public float TankMax => tankMax;
    public float TankCurrent => tankCurrent;
    public float TankPercent => Mathf.Clamp01(tankMax > 0f ? tankCurrent / tankMax : 0f);
    public event Action<float, float> OnTankChanged;

    bool _isSpraying;
    public bool CanSpray => tankCurrent > 0.05f;

    void OnValidate()
    {
        tankMax = Mathf.Max(0f, tankMax);
        if (tankMax > 0f) tankCurrent = Mathf.Clamp(tankCurrent, 0f, tankMax);
    }

    void Awake()
    {
        if (!player) player = transform;
        if (!sprayLoop) sprayLoop = GetComponentInChildren<ParticleSystem>(true);

        // 1) 자식 트리에서 즉시 시도
        if (!emitter) emitter = GetComponentInChildren<SprayEmitter>(true);
        if (!nozzle) nozzle = FindNozzleTransform();

        // 2) 타입명이 달라서 못 잡는 경우 반사 탐색
        if (!emitter)
        {
            var any = ResolveEmitterAny();
            if (any is SprayEmitter se) emitter = se;
        }

        // 3) 그래도 없으면 씬 전체에서 가장 가까운 SprayEmitter 하나 물고오자(디버그 방어용)
        if (!emitter)
        {
            var all = FindObjectsOfType<SprayEmitter>(true);
            if (all != null && all.Length > 0)
            {
                emitter = all.OrderBy(se => (se.transform.position - transform.position).sqrMagnitude).FirstOrDefault();
                Debug.LogWarning($"[Ext] emitter not found under player → picked nearest in scene: {emitter?.name}");
            }
        }

        // 초기 보정
        if (tankMax <= 0f) tankMax = Mathf.Max(1f, tankCurrent);
        tankCurrent = Mathf.Clamp(tankCurrent, 0f, tankMax);
        RaiseTankChanged();

        Debug.Log($"[Ext] Awake bind: emitter={(emitter ? emitter.name : "null")} nozzle={(nozzle ? nozzle.name : "null")}");
    }

    Transform FindNozzleTransform()
    {
        // 우선 emitter가 있으면 그 트랜스폼
        if (emitter) return emitter.nozzle ? emitter.nozzle : emitter.transform;

        // 이름 단서로 자식 검색
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (!t) continue;
            if (!string.IsNullOrEmpty(nozzleNameHint) &&
                t.name.IndexOf(nozzleNameHint, StringComparison.OrdinalIgnoreCase) >= 0)
                return t;
        }
        // 실패 시 자기 자신
        return this.transform;
    }

    Component ResolveEmitterAny()
    {
        // 현재 트리에서 타입명이 "SprayEmitter"인 컴포넌트 탐색(네임스페이스 달라도 OK)
        foreach (var mb in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (!mb) continue;
            var tn = mb.GetType().Name;
            if (tn == emitterTypeName || tn.IndexOf("SprayEmitter", StringComparison.OrdinalIgnoreCase) >= 0)
                return mb;
        }
        return null;
    }

    public void TrySpraying(float deltaTime)
    {
        // 매 프레임 바인딩 재확인 (런타임 활성화/비활성 전환 대응)
        if (!emitter) emitter = GetComponentInChildren<SprayEmitter>(true);
        if (!emitter)
        {
            var any = ResolveEmitterAny();
            if (any is SprayEmitter se) emitter = se;
        }
        if (!emitter)
        {
            // 최후 방어: 씬 전체에서 최근접 잡기
            var all = FindObjectsOfType<SprayEmitter>(true);
            if (all != null && all.Length > 0)
                emitter = all.OrderBy(se => (se.transform.position - transform.position).sqrMagnitude).FirstOrDefault();
        }

        if (!emitter)
        {
            StopSpraying();
            Debug.LogError("[Ext] emitter missing (can’t find SprayEmitter anywhere)");
            return;
        }
        if (!CanSpray) { StopSpraying(); return; }

        if (!_isSpraying)
        {
            _isSpraying = true;
            if (sprayLoop && !sprayLoop.isPlaying) sprayLoop.Play();
            if (spraySfx && !spraySfx.isPlaying) spraySfx.Play();
        }

        if (!nozzle) nozzle = FindNozzleTransform();

        // ★ 너가 준 SprayEmitter 시그니처: Spray(Vector3 direction, float deltaTime)
        Vector3 dir = nozzle ? nozzle.forward : transform.forward;
        float dt = Mathf.Max(0.0001f, deltaTime);
        emitter.Spray(dir, dt);

        // 탱크 소모
        float before = tankCurrent;
        tankCurrent = Mathf.Clamp(tankCurrent - flowRate * dt, 0f, tankMax);
        if (!Mathf.Approximately(before, tankCurrent)) RaiseTankChanged();

        if (tankCurrent <= 0.05f) StopSpraying();
    }

    public void StopSpraying()
    {
        if (_isSpraying)
        {
            _isSpraying = false;
            if (sprayLoop && sprayLoop.isPlaying)
                sprayLoop.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (spraySfx && spraySfx.isPlaying)
                spraySfx.Stop();
        }
    }

    public void RefillAll() => SetTank(tankMax);

    public void SetTank(float value)
    {
        float v = Mathf.Clamp(value, 0f, Mathf.Max(0.0001f, tankMax));
        if (!Mathf.Approximately(tankCurrent, v))
        {
            tankCurrent = v;
            RaiseTankChanged();
        }
    }

    public void SetTankMax(float newMax, bool keepPercent = true)
    {
        newMax = Mathf.Max(0.01f, newMax);
        float percent = TankPercent;
        tankMax = newMax;
        tankCurrent = keepPercent ? percent * tankMax : Mathf.Min(tankCurrent, tankMax);
        RaiseTankChanged();
    }

    void RaiseTankChanged() => OnTankChanged?.Invoke(tankCurrent, TankPercent);
}
