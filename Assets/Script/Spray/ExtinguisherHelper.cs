using UnityEngine;

/// <summary>
/// 소화기 모드 진입/해제를 관리하는 헬퍼 클래스
/// 팝업 리워드나 디버그에서 사용
/// </summary>
public static class ExtinguisherHelper
{
    /// <summary>
    /// 플레이어를 소화기 모드로 전환
    /// </summary>
    /// <param name="player">플레이어 컨트롤러</param>
    /// <param name="extinguisherPrefab">소화기 프리팹 (null이면 기본 생성)</param>
    /// <param name="autoDestroy">모드 해제 시 소화기 파괴 여부</param>
    /// <returns>성공 여부</returns>
    public static bool EnterMode(PlayerController player, GameObject extinguisherPrefab = null, bool autoDestroy = true)
    {
        if (!player)
        {
            Debug.LogError("[ExtinguisherHelper] PlayerController is null!");
            return false;
        }

        if (player.IsInExtinguisherMode)
        {
            Debug.LogWarning("[ExtinguisherHelper] Already in extinguisher mode!");
            return false;
        }

        // 1) 소화기 오브젝트 확보 (프리팹 → 인스턴스 or 런타임 생성)
        GameObject go = null;
        ExtinguisherItem item = null;
        ExtinguisherController ctrl = null;

        if (extinguisherPrefab != null)
        {
            go = Object.Instantiate(extinguisherPrefab);
            if (!go) { Debug.LogError("[ExtinguisherHelper] Instantiate failed"); return false; }

            item = go.GetComponent<ExtinguisherItem>() ?? go.AddComponent<ExtinguisherItem>();
            ctrl = go.GetComponent<ExtinguisherController>() ?? go.AddComponent<ExtinguisherController>();
            // (선택) 프리팹에 Rigidbody 없으면 추가
            var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
        else
        {
            go = new GameObject("Extinguisher_Runtime");
            var rb = go.AddComponent<Rigidbody>(); rb.isKinematic = true;
            ctrl = go.AddComponent<ExtinguisherController>();
            item = go.AddComponent<ExtinguisherItem>();
        }

        if (!item || !ctrl)
        {
            Debug.LogError("[ExtinguisherHelper] Missing ExtinguisherItem/Controller after setup");
            if (go) Object.Destroy(go);
            return false;
        }
        item.controller = ctrl;

        // 2) 플레이어 모드 진입 (우리가 추가한 오버로드)
        bool ok = player.EnterExtinguisherMode(item);
        if (!ok)
        {
            Object.Destroy(go);
            return false;
        }

        // 3) 자동 파괴 표시(옵션)
        if (autoDestroy) go.name += "_AutoDestroy";

        Debug.Log($"[ExtinguisherHelper] Entered mode! (AutoDestroy: {autoDestroy})");
        return true;
    }


    /// <summary>
    /// 소화기 모드 해제
    /// </summary>
    /// <param name="player">플레이어 컨트롤러</param>
    /// <param name="destroyExtinguisher">소화기 파괴 여부</param>
    public static void ExitMode(PlayerController player, bool destroyExtinguisher = true)
    {
        if (!player)
        {
            Debug.LogError("[ExtinguisherHelper] PlayerController is null!");
            return;
        }

        if (!player.IsInExtinguisherMode)
        {
            Debug.LogWarning("[ExtinguisherHelper] Not in extinguisher mode!");
            return;
        }

        var extinguisher = player.EquippedExtinguisher;

        // 모드 해제
        player.ExitExtinguisherMode();

        // 소화기 파괴
        if (destroyExtinguisher && extinguisher != null)
        {
            Object.Destroy(extinguisher.gameObject);
            Debug.Log("[ExtinguisherHelper] Extinguisher destroyed!");
        }

        Debug.Log("[ExtinguisherHelper] Exited mode!");
    }

    /// <summary>
    /// 소화기 모드 토글 (켜기/끄기)
    /// </summary>
    public static bool ToggleMode(PlayerController player, GameObject extinguisherPrefab = null)
    {
        if (!player) return false;

        if (player.IsInExtinguisherMode)
        {
            ExitMode(player, true);
            return false;
        }
        else
        {
            return EnterMode(player, extinguisherPrefab, true);
        }
    }

    /// <summary>
    /// 타이머로 자동 해제 (예: 제한 시간)
    /// </summary>
    public static void EnterModeWithTimer(PlayerController player, GameObject extinguisherPrefab, float durationSeconds)
    {
        // 주의: autoDestroy를 true로 설정해야 타이머 종료 시 자동 파괴됨
        if (EnterMode(player, extinguisherPrefab, true))
        {
            // 코루틴 시작을 위한 MonoBehaviour 필요
            var helper = player.gameObject.AddComponent<ExtinguisherTimerHelper>();
            helper.StartTimer(player, durationSeconds);
        }
    }
}

/// <summary>
/// 타이머 구현용 임시 컴포넌트
/// </summary>
internal class ExtinguisherTimerHelper : MonoBehaviour
{
    public void StartTimer(PlayerController player, float duration)
    {
        StartCoroutine(TimerCoroutine(player, duration));
    }

    System.Collections.IEnumerator TimerCoroutine(PlayerController player, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (player && player.IsInExtinguisherMode)
        {
            // 타이머 종료 시 항상 파괴 (autoDestroy를 true로 설정했으므로)
            ExtinguisherHelper.ExitMode(player, true);
            Debug.Log($"[ExtinguisherHelper] Timer expired! ({duration}s)");
        }

        Destroy(this);
    }
}
