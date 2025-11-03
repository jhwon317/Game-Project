using UnityEngine;
using UnityEngine.EventSystems;
using PopupMini;

/// <summary>
/// 팝업 마우스 이벤트 진단 도구
/// PopupHost와 같은 GameObject에 추가하여 문제 진단
/// </summary>
public class PopupEventDebugger : MonoBehaviour
{
    [Header("Auto Find")]
    public bool autoFind = true;

    [Header("References")]
    public PopupHost host;
    public RTUIClickProxyPro clickProxy;
    public EventSystem eventSystem;
    public Canvas canvas;

    [Header("Debug Options")]
    public bool logMouseEvents = true;
    public bool logRaycastResults = true;
    public bool showGizmos = true;

    [Header("Status")]
    [SerializeField] private bool _isSetupValid = false;
    [SerializeField] private string _statusMessage = "";

    void Awake()
    {
        if (autoFind)
        {
            if (!host) host = GetComponent<PopupHost>();
            if (!clickProxy && host && host.Viewport) 
                clickProxy = host.Viewport.GetComponent<RTUIClickProxyPro>();
            if (!eventSystem) eventSystem = FindObjectOfType<EventSystem>();
            if (!canvas) canvas = GetComponentInParent<Canvas>();
        }
    }

    void Start()
    {
        ValidateSetup();
    }

    void Update()
    {
        if (logMouseEvents && Input.GetMouseButtonDown(0))
        {
            LogMouseClick();
        }
    }

    void ValidateSetup()
    {
        _isSetupValid = true;
        _statusMessage = "✅ 모든 설정이 정상입니다.";

        // EventSystem 확인
        if (!eventSystem)
        {
            _isSetupValid = false;
            _statusMessage = "❌ EventSystem이 없습니다!";
            Debug.LogError("[PopupDebug] EventSystem이 씬에 없습니다. GameObject > UI > Event System 추가 필요!");
            return;
        }

        // PopupHost 확인
        if (!host)
        {
            _isSetupValid = false;
            _statusMessage = "❌ PopupHost가 없습니다!";
            Debug.LogError("[PopupDebug] PopupHost를 찾을 수 없습니다!");
            return;
        }

        // Viewport 확인
        if (!host.Viewport)
        {
            _isSetupValid = false;
            _statusMessage = "❌ Viewport가 없습니다!";
            Debug.LogError("[PopupDebug] PopupHost.Viewport가 할당되지 않았습니다!");
            return;
        }

        // RawImage 확인
        var rawImage = host.Viewport.GetComponent<UnityEngine.UI.RawImage>();
        if (!rawImage)
        {
            _isSetupValid = false;
            _statusMessage = "❌ RawImage가 없습니다!";
            Debug.LogError("[PopupDebug] Viewport에 RawImage 컴포넌트가 없습니다!");
            return;
        }

        // Raycast Target 확인
        if (!rawImage.raycastTarget)
        {
            _isSetupValid = false;
            _statusMessage = "⚠️ RawImage의 Raycast Target이 꺼져있습니다!";
            Debug.LogWarning("[PopupDebug] RawImage의 'Raycast Target'을 체크해야 클릭이 작동합니다!");
            Debug.LogWarning("[PopupDebug] 자동으로 활성화합니다...");
            rawImage.raycastTarget = true;
        }

        // RTUIClickProxyPro 확인
        if (!clickProxy)
        {
            _isSetupValid = false;
            _statusMessage = "❌ RTUIClickProxyPro가 없습니다!";
            Debug.LogError("[PopupDebug] Viewport에 RTUIClickProxyPro 컴포넌트가 없습니다!");
            Debug.LogError("[PopupDebug] Viewport GameObject에 RTUIClickProxyPro 컴포넌트를 추가하세요!");
            return;
        }

        // ClickProxy 설정 확인
        if (!clickProxy.targetCamera)
        {
            Debug.LogWarning("[PopupDebug] RTUIClickProxyPro의 targetCamera가 null입니다. 자동 할당을 기다리는 중...");
        }

        if (!clickProxy.targetRoot)
        {
            Debug.LogWarning("[PopupDebug] RTUIClickProxyPro의 targetRoot가 null입니다. 자동 할당을 기다리는 중...");
        }

        // Canvas 확인
        if (!canvas)
        {
            Debug.LogWarning("[PopupDebug] Canvas를 찾을 수 없습니다. Overlay 모드인지 확인하세요.");
        }
        else
        {
            Debug.Log($"[PopupDebug] Canvas 모드: {canvas.renderMode}");
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && !canvas.worldCamera)
            {
                Debug.LogWarning("[PopupDebug] Canvas가 ScreenSpaceCamera 모드인데 worldCamera가 없습니다!");
            }
        }

        Debug.Log("[PopupDebug] ✅ 검증 완료!");
        Debug.Log($"[PopupDebug] EventSystem: {eventSystem?.name}");
        Debug.Log($"[PopupDebug] PopupHost: {host?.name}");
        Debug.Log($"[PopupDebug] Viewport: {host?.Viewport?.name}");
        Debug.Log($"[PopupDebug] ClickProxy: {clickProxy?.name}");
        Debug.Log($"[PopupDebug] Canvas: {canvas?.name} ({canvas?.renderMode})");
    }

    void LogMouseClick()
    {
        if (!eventSystem) return;

        var pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        Debug.Log($"[PopupDebug] ===== 마우스 클릭 감지 =====");
        Debug.Log($"[PopupDebug] 마우스 위치: {Input.mousePosition}");

        // 레이캐스트 결과 확인
        var results = new System.Collections.Generic.List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        Debug.Log($"[PopupDebug] 레이캐스트 결과: {results.Count}개");

        if (results.Count == 0)
        {
            Debug.LogWarning("[PopupDebug] ⚠️ 클릭이 아무 UI도 hit하지 않았습니다!");
            Debug.LogWarning("[PopupDebug] 가능한 원인:");
            Debug.LogWarning("[PopupDebug] 1. RawImage의 Raycast Target이 꺼져있음");
            Debug.LogWarning("[PopupDebug] 2. Canvas가 비활성화됨");
            Debug.LogWarning("[PopupDebug] 3. CanvasGroup의 blocksRaycasts가 false");
        }
        else
        {
            for (int i = 0; i < results.Count; i++)
            {
                var hit = results[i];
                Debug.Log($"[PopupDebug] [{i}] {hit.gameObject.name} (depth: {hit.depth})");
                
                if (hit.gameObject == host?.Viewport?.gameObject)
                {
                    Debug.Log($"[PopupDebug] ✅ Viewport를 hit했습니다!");
                    
                    // RTUIClickProxyPro 확인
                    if (clickProxy)
                    {
                        Debug.Log($"[PopupDebug] RTUIClickProxyPro 상태:");
                        Debug.Log($"[PopupDebug] - targetCamera: {clickProxy.targetCamera?.name ?? "null"}");
                        Debug.Log($"[PopupDebug] - targetRoot: {clickProxy.targetRoot?.name ?? "null"}");
                        Debug.Log($"[PopupDebug] - autoAssign: {clickProxy.autoAssignOnEnable}");
                    }
                    else
                    {
                        Debug.LogError("[PopupDebug] ❌ RTUIClickProxyPro가 없습니다!");
                    }
                }
            }
        }

        Debug.Log($"[PopupDebug] ================================");
    }

    [ContextMenu("Force Validate")]
    public void ForceValidate()
    {
        ValidateSetup();
    }

    [ContextMenu("Enable Debug Logging")]
    public void EnableDebugLogging()
    {
        if (clickProxy)
        {
            clickProxy.debugLog = true;
            Debug.Log("[PopupDebug] RTUIClickProxyPro 디버그 로그 활성화됨!");
        }
        else
        {
            Debug.LogError("[PopupDebug] RTUIClickProxyPro를 찾을 수 없습니다!");
        }
    }

    [ContextMenu("Test Raycast Target")]
    public void TestRaycastTarget()
    {
        if (!host || !host.Viewport) return;

        var rawImage = host.Viewport.GetComponent<UnityEngine.UI.RawImage>();
        if (rawImage)
        {
            Debug.Log($"[PopupDebug] RawImage.raycastTarget = {rawImage.raycastTarget}");
            if (!rawImage.raycastTarget)
            {
                Debug.LogWarning("[PopupDebug] ⚠️ Raycast Target이 꺼져있습니다! 켜는 중...");
                rawImage.raycastTarget = true;
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showGizmos || !host || !host.Viewport) return;

        var rt = host.Viewport.transform as RectTransform;
        if (!rt) return;

        // RawImage 범위 표시
        Gizmos.color = _isSetupValid ? Color.green : Color.red;
        
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }

        // 상태 텍스트
        UnityEditor.Handles.Label(
            rt.position + Vector3.up * 100f,
            _statusMessage,
            new GUIStyle
            {
                normal = new GUIStyleState 
                { 
                    textColor = _isSetupValid ? Color.green : Color.red 
                },
                fontSize = 14,
                alignment = UnityEngine.TextAnchor.MiddleCenter
            }
        );
    }
#endif
}
