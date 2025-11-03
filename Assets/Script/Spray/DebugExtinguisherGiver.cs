using UnityEngine;

/// <summary>
/// 디버그용 - 팝업 없이 바로 소화기 모드 진입
/// </summary>
public class DebugExtinguisherGiver : MonoBehaviour, IInteractable
{
    [Header("Extinguisher")]
    public GameObject extinguisherPrefab;

    [Header("Options")]
    [Tooltip("제한 시간 (0 = 무제한)")]
    public float durationSeconds = 0f;
    public bool oneTimeUse = true;

    private bool _used = false;

    public Transform GetTransform() => transform;
    public void SetHighlighted(bool on) { }

    public void OnInteract(GameObject interactor)
    {
        if (_used && oneTimeUse) return;

        var player = interactor.GetComponent<PlayerController>();
        if (!player)
        {
            Debug.LogWarning("[DebugGiver] No PlayerController!");
            return;
        }

        bool success;

        if (durationSeconds > 0f)
        {
            // 제한 시간
            ExtinguisherHelper.EnterModeWithTimer(player, extinguisherPrefab, durationSeconds);
            success = true;
        }
        else
        {
            // 무제한
            success = ExtinguisherHelper.EnterMode(player, extinguisherPrefab, true);
        }

        if (success)
        {
            Debug.Log($"[DebugGiver] Granted! (Duration: {(durationSeconds > 0 ? durationSeconds + "s" : "unlimited")})");
            
            if (oneTimeUse)
            {
                _used = true;
                gameObject.SetActive(false);
            }
        }
    }
}

/// <summary>
/// 키보드 단축키로 소화기 모드 토글 (디버그)
/// PlayerController에 붙여서 사용
/// </summary>
public class DebugExtinguisherToggle : MonoBehaviour
{
    [Header("Debug Hotkey")]
    public KeyCode toggleKey = KeyCode.F5;
    public GameObject extinguisherPrefab;

    private PlayerController _player;

    void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (_player)
            {
                bool isOn = ExtinguisherHelper.ToggleMode(_player, extinguisherPrefab);
                Debug.Log($"[Debug] Extinguisher mode: {(isOn ? "ON" : "OFF")}");
            }
        }
    }
}
