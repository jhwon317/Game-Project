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

        // 이미 소화기 모드면 실패
        if (player.IsInExtinguisherMode)
        {
            Debug.LogWarning("[ExtinguisherHelper] Already in extinguisher mode!");
            return false;
        }

        // 소화기 생성
        ExtinguisherItem extinguisher = null;

        if (extinguisherPrefab != null)
        {
            // 프리팹으로 생성
            var obj = Object.Instantiate(extinguisherPrefab, player.transform.position, Quaternion.identity);
            extinguisher = obj.GetComponent<ExtinguisherItem>();

            if (!extinguisher)
            {
                Debug.LogError("[ExtinguisherHelper] Prefab has no ExtinguisherItem component!");
                Object.Destroy(obj);
                return false;
            }
        }
        else
        {
            // 기본 소화기 생성 (빈 오브젝트)
            var obj = new GameObject("Extinguisher_Runtime");
            extinguisher = obj.AddComponent<ExtinguisherItem>();
            
            // 기본 컴포넌트 추가
            var rb = obj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            
            var controller = obj.AddComponent<ExtinguisherController>();
            extinguisher.controller = controller;

            Debug.LogWarning("[ExtinguisherHelper] No prefab provided, created basic extinguisher!");
        }

        // 모드 진입
        bool success = player.EnterExtinguisherMode(extinguisher);

        if (!success)
        {
            // 실패 시 소화기 파괴
            Object.Destroy(extinguisher.gameObject);
            return false;
        }

        // 자동 파괴 태그 설정
        if (autoDestroy)
        {
            extinguisher.gameObject.name += "_AutoDestroy";
        }

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
        if (EnterMode(player, extinguisherPrefab, false))
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
            ExtinguisherHelper.ExitMode(player, true);
            Debug.Log($"[ExtinguisherHelper] Timer expired! ({duration}s)");
        }

        Destroy(this);
    }
}
