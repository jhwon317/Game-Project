using UnityEngine;
using PopupMini;
using System.Threading.Tasks;

/// <summary>
/// FirePin 퍼즐 상호작용 → 성공 시 소화기 모드 진입
/// SimplePlayerMove와 PlayerController 모두 지원
/// </summary>
[RequireComponent(typeof(Collider))]
public class FirePinPuzzleInteractable : MonoBehaviour, IInteractable
{
    [Header("Popup Puzzle")]
    [Tooltip("팝업 세션 매니저 (씬에 있어야 함)")]
    public PopupSessionManager sessionManager;
    
    [Tooltip("FirePin 퍼즐 정의 (ScriptableObject)")]
    public PuzzleDefinition firePinDefinition;
    
    [TextArea(3, 5)]
    [Tooltip("퍼즐에 전달할 JSON 인자 (선택사항)")]
    public string puzzleArgs = "";

    [Header("Extinguisher Reward")]
    [Tooltip("보상으로 줄 소화기 프리팹 (없으면 기본 생성)")]
    public GameObject extinguisherPrefab;
    
    [Tooltip("소화기 사용 제한 시간 (0 = 무제한)")]
    public float extinguisherDuration = 0f;
    
    [Tooltip("모드 해제 시 소화기 자동 파괴")]
    public bool autoDestroyExtinguisher = true;

    [Header("Interaction Settings")]
    [Tooltip("한 번만 상호작용 가능")]
    public bool oneTimeUse = true;
    
    [Tooltip("사용 후 오브젝트 비활성화")]
    public bool disableAfterUse = true;

    [Header("Visual Feedback")]
    [Tooltip("하이라이트 머티리얼 (선택사항)")]
    public Material highlightMaterial;
    
    [Tooltip("하이라이트 적용할 렌더러 (비어있으면 자동 검색)")]
    public Renderer targetRenderer;

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip interactSound;
    public AudioClip successSound;
    public AudioClip failSound;

    [Header("Status")]
    [SerializeField] private bool _isUsed = false;
    [SerializeField] private bool _isBusy = false;

    private Material _originalMaterial;
    private Collider _collider;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        
        // 자동 참조 설정
        if (!sessionManager)
            sessionManager = FindObjectOfType<PopupSessionManager>();
        
        if (!targetRenderer)
            targetRenderer = GetComponentInChildren<Renderer>();
        
        if (targetRenderer && highlightMaterial)
            _originalMaterial = targetRenderer.material;
        
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // 필수 컴포넌트 검증
        if (!sessionManager)
        {
            Debug.LogError($"[FirePinPuzzle] {name}: PopupSessionManager가 없습니다! 씬에 추가하세요.");
            enabled = false;
            return;
        }

        if (!firePinDefinition)
        {
            Debug.LogError($"[FirePinPuzzle] {name}: PuzzleDefinition이 할당되지 않았습니다!");
            enabled = false;
            return;
        }
    }

    public Transform GetTransform() => transform;

    public void SetHighlighted(bool highlighted)
    {
        if (_isUsed || !targetRenderer || !highlightMaterial) return;

        if (highlighted)
            targetRenderer.material = highlightMaterial;
        else if (_originalMaterial)
            targetRenderer.material = _originalMaterial;
    }

    public async void OnInteract(GameObject interactor)
    {
        // 이미 사용했거나 바쁜 경우 무시
        if (_isBusy || (oneTimeUse && _isUsed))
        {
            Debug.Log($"[FirePinPuzzle] {name}: 이미 사용되었거나 진행 중입니다.");
            return;
        }

        // 플레이어 확인 (PlayerController 또는 SimplePlayerMove)
        var playerController = interactor.GetComponent<PlayerController>();
        var simplePlayerMove = interactor.GetComponent<SimplePlayerMove>();
        
        if (!playerController && !simplePlayerMove)
        {
            Debug.LogWarning($"[FirePinPuzzle] {name}: PlayerController 또는 SimplePlayerMove를 찾을 수 없습니다!");
            return;
        }

        // PlayerController가 있고 이미 소화기 모드인지 확인
        if (playerController && playerController.IsInExtinguisherMode)
        {
            Debug.Log($"[FirePinPuzzle] {name}: 이미 소화기를 들고 있습니다!");
            PlaySound(failSound);
            return;
        }

        _isBusy = true;

        // 상호작용 사운드
        PlaySound(interactSound);

        // 퍼즐 시작
        Debug.Log($"[FirePinPuzzle] {name}: FirePin 퍼즐을 시작합니다...");
        
        var result = await OpenFirePinPuzzle();

        // 결과 처리
        if (result.Success)
        {
            Debug.Log($"[FirePinPuzzle] {name}: 퍼즐 성공! 소화기를 지급합니다.");
            PlaySound(successSound);
            
            bool granted = GrantExtinguisher(interactor, playerController);

            if (granted)
            {
                _isUsed = true;

                // 사용 완료 처리
                if (oneTimeUse && disableAfterUse)
                {
                    SetHighlighted(false);
                    if (_collider) _collider.enabled = false;
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError($"[FirePinPuzzle] {name}: 소화기 지급 실패!");
                PlaySound(failSound);
            }
        }
        else
        {
            Debug.Log($"[FirePinPuzzle] {name}: 퍼즐 실패 또는 취소됨. Reason: {result.Reason}");
            PlaySound(failSound);
        }

        _isBusy = false;
    }

    /// <summary>
    /// FirePin 퍼즐 열기
    /// </summary>
    private async Task<PopupMini.PuzzleResult> OpenFirePinPuzzle()
    {
        var request = new PuzzleRequest
        {
            Definition = firePinDefinition,
            Args = string.IsNullOrEmpty(puzzleArgs) ? null : puzzleArgs
        };

        try
        {
            var result = await sessionManager.OpenAsync(request);
            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FirePinPuzzle] {name}: 퍼즐 오픈 중 예외 발생: {e.Message}");
            return PopupMini.PuzzleResult.Error(e.Message);
        }
    }

    /// <summary>
    /// 플레이어에게 소화기 지급
    /// </summary>
    private bool GrantExtinguisher(GameObject playerObj, PlayerController playerController)
    {
        if (!playerObj)
        {
            Debug.LogError($"[FirePinPuzzle] {name}: Player GameObject가 null입니다!");
            return false;
        }

        // PlayerController가 있으면 ExtinguisherHelper 사용
        if (playerController)
        {
            bool success;

            if (extinguisherDuration > 0f)
            {
                // 제한 시간 모드
                Debug.Log($"[FirePinPuzzle] {name}: 제한 시간 모드 ({extinguisherDuration}초)");
                ExtinguisherHelper.EnterModeWithTimer(playerController, extinguisherPrefab, extinguisherDuration);
                success = true;
            }
            else
            {
                // 무제한 모드
                Debug.Log($"[FirePinPuzzle] {name}: 무제한 모드");
                success = ExtinguisherHelper.EnterMode(playerController, extinguisherPrefab, autoDestroyExtinguisher);
            }

            if (success)
            {
                Debug.Log($"[FirePinPuzzle] {name}: 소화기 지급 완료!");
            }

            return success;
        }
        // SimplePlayerMove만 있는 경우 - 직접 소화기 생성 및 활성화
        else
        {
            Debug.Log($"[FirePinPuzzle] {name}: SimplePlayerMove 감지 - 소화기를 씬에 생성합니다.");
            
            // 소화기 생성 또는 활성화
            GameObject extinguisher = null;
            
            if (extinguisherPrefab)
            {
                // 프리팹으로 생성
                extinguisher = Instantiate(extinguisherPrefab, playerObj.transform.position + playerObj.transform.forward * 2f, Quaternion.identity);
            }
            else
            {
                // 기본 소화기 생성
                extinguisher = new GameObject("Extinguisher");
                extinguisher.transform.position = playerObj.transform.position + playerObj.transform.forward * 2f;
                
                // 기본 컴포넌트 추가
                var item = extinguisher.AddComponent<ExtinguisherItem>();
                var rb = extinguisher.AddComponent<Rigidbody>();
                var controller = extinguisher.AddComponent<ExtinguisherController>();
                
                item.controller = controller;
                item.rb = rb;
                
                Debug.LogWarning($"[FirePinPuzzle] {name}: 프리팹이 없어 기본 소화기를 생성했습니다.");
            }
            
            if (extinguisher)
            {
                // 제한 시간 처리 (SimplePlayerMove용)
                if (extinguisherDuration > 0f && autoDestroyExtinguisher)
                {
                    Destroy(extinguisher, extinguisherDuration);
                    Debug.Log($"[FirePinPuzzle] {name}: 소화기가 {extinguisherDuration}초 후 파괴됩니다.");
                }
                
                Debug.Log($"[FirePinPuzzle] {name}: 소화기가 생성되었습니다.");
                return true;
            }
            else
            {
                Debug.LogError($"[FirePinPuzzle] {name}: 소화기 생성 실패!");
                return false;
            }
        }
    }

    /// <summary>
    /// 사운드 재생 헬퍼
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// 상태 리셋 (디버그용)
    /// </summary>
    [ContextMenu("Reset State")]
    public void ResetState()
    {
        _isUsed = false;
        _isBusy = false;
        if (_collider) _collider.enabled = true;
        gameObject.SetActive(true);
        SetHighlighted(false);
        Debug.Log($"[FirePinPuzzle] {name}: 상태가 리셋되었습니다.");
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 상호작용 범위 표시
        Gizmos.color = _isUsed ? Color.gray : (_isBusy ? Color.yellow : Color.green);
        Gizmos.DrawWireSphere(transform.position, 1.5f);

        // 상태 텍스트
        var status = _isUsed ? "USED" : (_isBusy ? "BUSY" : "READY");
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            $"FirePin Puzzle\n[{status}]",
            new GUIStyle 
            { 
                normal = new GUIStyleState 
                { 
                    textColor = _isUsed ? Color.gray : (_isBusy ? Color.yellow : Color.green) 
                },
                alignment = UnityEngine.TextAnchor.MiddleCenter,
                fontSize = 12
            }
        );

        // 제한 시간 표시
        if (extinguisherDuration > 0f)
        {
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2.5f,
                $"⏱ {extinguisherDuration}s",
                new GUIStyle 
                { 
                    normal = new GUIStyleState { textColor = Color.cyan },
                    alignment = UnityEngine.TextAnchor.MiddleCenter,
                    fontSize = 10
                }
            );
        }

        // 플레이어 타입 표시
        var player = FindObjectOfType<PlayerController>();
        var simplePlayer = FindObjectOfType<SimplePlayerMove>();
        var playerType = player ? "PlayerController" : (simplePlayer ? "SimplePlayerMove" : "NONE");
        
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 3f,
            $"Player: {playerType}",
            new GUIStyle 
            { 
                normal = new GUIStyleState { textColor = Color.white },
                alignment = UnityEngine.TextAnchor.MiddleCenter,
                fontSize = 9
            }
        );
    }

    void OnValidate()
    {
        // 자동 이름 설정
        if (string.IsNullOrEmpty(gameObject.name) || gameObject.name == "GameObject")
        {
            gameObject.name = "FirePinPuzzle_Interactable";
        }
    }
#endif
}
