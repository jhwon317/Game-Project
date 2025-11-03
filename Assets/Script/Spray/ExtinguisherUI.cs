using UnityEngine;

/// <summary>
/// 소화기 탱크 잔량 UI. 플레이어가 소화기를 들고 있을 때만 표시.
/// InventoryDebugHUD 스타일 재활용.
/// </summary>
public class ExtinguisherUI : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("UI 크기 배율")]
    public float uiScale = 1f;
    [Tooltip("화면 위치 (우상단 기준 오프셋)")]
    public Vector2 screenOffset = new Vector2(-220, 12);

    [Header("Auto Find (Optional)")]
    [Tooltip("비워두면 자동으로 ExtinguisherController 찾음")]
    public ExtinguisherController extinguisher;

    [Header("Colors")]
    public Color normalColor = Color.cyan;
    public Color lowColor = Color.red;
    [Range(0f, 1f)] public float lowThreshold = 0.3f;

    private GUIStyle _labelStyle;
    private GUIStyle _barBgStyle;
    private Texture2D _whiteTex;

    void Awake()
    {
        // 흰색 텍스처 생성 (바 그리기용)
        _whiteTex = new Texture2D(1, 1);
        _whiteTex.SetPixel(0, 0, Color.white);
        _whiteTex.Apply();
    }

    void OnGUI()
    {
        // 소화기를 들고 있지 않으면 표시하지 않음
        if (!FindActiveExtinguisher()) return;

        // 스타일 초기화
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = Mathf.RoundToInt(18 * uiScale);
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.alignment = TextAnchor.MiddleLeft;
        }

        float percent = extinguisher.TankPercent;
        Color barColor = percent > lowThreshold ? normalColor : lowColor;

        // UI 영역 설정 (우상단)
        float areaWidth = 200 * uiScale;
        float areaHeight = 80 * uiScale;
        float areaX = Screen.width + screenOffset.x * uiScale;
        float areaY = screenOffset.y * uiScale;

        GUILayout.BeginArea(new Rect(areaX, areaY, areaWidth, areaHeight));

        // 텍스트 표시
        GUILayout.Label($"소화기: {percent * 100:0}%", _labelStyle);

        GUILayout.Space(8 * uiScale);

        // 바 배경
        Rect barBg = new Rect(0, 35 * uiScale, 180 * uiScale, 20 * uiScale);
        GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        GUI.DrawTexture(barBg, _whiteTex);

        // 바 채우기
        Rect barFill = new Rect(0, 35 * uiScale, 180 * uiScale * percent, 20 * uiScale);
        GUI.color = barColor;
        GUI.DrawTexture(barFill, _whiteTex);

        // 바 테두리
        GUI.color = Color.white;
        DrawBorder(barBg, 2);

        GUI.color = Color.white;
        GUILayout.EndArea();
    }

    /// <summary>
    /// 현재 플레이어가 들고 있는 소화기를 자동으로 찾음
    /// </summary>
    bool FindActiveExtinguisher()
    {
        // 이미 할당되어 있고 활성화된 경우
        if (extinguisher != null && extinguisher.enabled && extinguisher.gameObject.activeInHierarchy)
            return true;

        // PlayerController에서 들고 있는 오브젝트 확인
        var player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        if (player == null || player.heldObject == null) return false;

        // ExtinguisherItem 확인
        var item = player.heldObject.GetComponent<ExtinguisherItem>();
        if (item == null) return false;

        extinguisher = item.controller;
        return extinguisher != null && extinguisher.enabled;
    }

    /// <summary>
    /// 사각형 테두리 그리기
    /// </summary>
    void DrawBorder(Rect rect, float thickness)
    {
        // 상
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), _whiteTex);
        // 하
        GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), _whiteTex);
        // 좌
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), _whiteTex);
        // 우
        GUI.DrawTexture(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), _whiteTex);
    }

    void OnDestroy()
    {
        if (_whiteTex) Destroy(_whiteTex);
    }
}
