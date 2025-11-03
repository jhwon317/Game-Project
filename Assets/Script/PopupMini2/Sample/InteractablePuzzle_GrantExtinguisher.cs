using UnityEngine;
using PopupMini;

/// <summary>
/// 팝업 성공시 소화기 모드로 진입
/// ExtinguisherHelper를 통해 처리
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractablePuzzle_GrantExtinguisher : MonoBehaviour, IInteractable
{
    [Header("Puzzle")]
    public PopupSessionManager session;
    public PuzzleDefinition definition;
    [TextArea] public string jsonArgs;

    [Header("Extinguisher Reward")]
    [Tooltip("소화기 프리팹 (없으면 기본 생성)")]
    public GameObject extinguisherPrefab;

    [Header("Mode Options")]
    [Tooltip("제한 시간 (0 = 무제한)")]
    public float durationSeconds = 0f;
    [Tooltip("모드 해제 시 소화기 파괴")]
    public bool autoDestroy = true;

    [Header("Consume Options")]
    public bool oneTimeUse = true;
    public bool disableAfterUse = true;

    private bool _busy;
    private bool _consumed;

    public Transform GetTransform() => transform;
    public void SetHighlighted(bool on) { }

    public async void OnInteract(GameObject interactor)
    {
        if (_busy || !session || !definition) return;
        if (_consumed && oneTimeUse) return;

        _busy = true;

        var req = new PuzzleRequest
        {
            Definition = definition,
            Args = string.IsNullOrEmpty(jsonArgs) ? null : jsonArgs
        };

        var result = await session.OpenAsync(req);

        if (result.Success)
        {
            bool granted = GrantExtinguisher(interactor);

            if (granted && oneTimeUse)
            {
                _consumed = true;
                if (disableAfterUse)
                {
                    foreach (var c in GetComponentsInChildren<Collider>(true))
                        c.enabled = false;
                    gameObject.SetActive(false);
                }
            }
        }

        _busy = false;
    }

    bool GrantExtinguisher(GameObject interactor)
    {
        if (!interactor)
        {
            Debug.LogError("[PuzzleReward] Interactor is null!");
            return false;
        }

        var player = interactor.GetComponent<PlayerController>();
        if (!player)
        {
            Debug.LogWarning("[PuzzleReward] No PlayerController found!");
            return false;
        }

        // 헬퍼를 통해 소화기 모드 진입
        bool success;

        if (durationSeconds > 0f)
        {
            // 제한 시간 모드
            ExtinguisherHelper.EnterModeWithTimer(player, extinguisherPrefab, durationSeconds);
            success = true;
        }
        else
        {
            // 무제한 모드
            success = ExtinguisherHelper.EnterMode(player, extinguisherPrefab, autoDestroy);
        }

        if (success)
        {
            Debug.Log($"[PuzzleReward] Granted extinguisher! (Duration: {(durationSeconds > 0 ? durationSeconds + "s" : "unlimited")})");
        }

        return success;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!enabled) return;

        Gizmos.color = _consumed ? Color.gray : Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);

        // 제한 시간 표시
        if (durationSeconds > 0f)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"Timer: {durationSeconds}s", 
                new GUIStyle { normal = new GUIStyleState { textColor = Color.yellow } });
        }
    }
#endif
}
