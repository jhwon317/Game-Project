using UnityEngine;
using PopupMini;

/// <summary>
/// 런타임에서 팝업 사이즈/패딩/카메라 맞춤을 즉석에서 테스트하는 도구.
/// - Panel 패딩: ContentRoot.offset
/// - Content 스케일: ContentRoot.localScale
/// - 카메라 자동맞춤: Ortho는 즉시, Persp는 바운드용 루트가 필요(옵션).
/// 단독으로 동작. Host/AutoLayout 참조만 있으면 됨.
/// </summary>
[DisallowMultipleComponent]
public class PopupSizeTester : MonoBehaviour
{
    [Header("Wiring")]
    public PopupHost host;                  // Panel/Content/Viewport
    public PopupAutoLayout auto;            // 패딩/스케일/카메라 옵션
    [Tooltip("3D 퍼즐(퍼스펙티브)일 때 바운드 계산용 루트(미지정이면 스킵).")]
    public Transform sampleRootForBounds;   // 선택

    [Header("Overlay")]
    public bool showOverlay = true;
    public bool autoRefitCameraOnChange = true;

    // 단축키(원하는대로 바꿔도 됨)
    [Header("Hotkeys")]
    public KeyCode toggleOverlayKey = KeyCode.F9;
    public KeyCode preset1Key = KeyCode.F1;   // Small Card
    public KeyCode preset2Key = KeyCode.F2;   // Medium Card
    public KeyCode preset3Key = KeyCode.F3;   // Edge-to-Edge
    public KeyCode preset4Key = KeyCode.F4;   // Tall
    public KeyCode preset5Key = KeyCode.F5;   // Wide
    public KeyCode toggleModeKey = KeyCode.P; // Pixels/Percent
    public KeyCode toggleAutoFitKey = KeyCode.O;
    public KeyCode toggleFitModeKey = KeyCode.F; // Contain/Fill
    public KeyCode scalePlusKey = KeyCode.Equals;  // =
    public KeyCode scaleMinusKey = KeyCode.Minus;  // -
    public KeyCode resetKey = KeyCode.R;

    // 조정 스텝
    [Header("Adjust Steps")]
    public float pixelStep = 16f;
    public float percentStep = 0.02f;
    public float scaleStep = 0.05f;

    // 내부 상태(패딩은 L,R,T,B 순)
    Vector4 _padPx = new Vector4(24, 24, 24, 24);
    Vector4 _padPct = new Vector4(0.05f, 0.05f, 0.05f, 0.05f);
    float _scale = 1f;

    void Awake()
    {
        if (!host) host = GetComponent<PopupHost>();
        if (!auto && host) auto = host.GetComponent<PopupAutoLayout>();
        if (!auto) Debug.LogWarning("[PopupSizeTester] PopupAutoLayout 참조가 없어요. 패딩/스케일 반영만 해요.");

        // 초기값을 AutoLayout에서 받아옴
        if (auto)
        {
            _scale = auto.contentScale;
            _padPx = auto.padding;
            _padPct = auto.paddingPercent;
        }

        ApplyAll();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleOverlayKey)) showOverlay = !showOverlay;

        // 프리셋
        if (Input.GetKeyDown(preset1Key)) ApplyPresetSmall();
        if (Input.GetKeyDown(preset2Key)) ApplyPresetMedium();
        if (Input.GetKeyDown(preset3Key)) ApplyPresetEdgeToEdge();
        if (Input.GetKeyDown(preset4Key)) ApplyPresetTall();
        if (Input.GetKeyDown(preset5Key)) ApplyPresetWide();

        // 모드 토글
        if (Input.GetKeyDown(toggleModeKey))
        {
            if (auto) auto.paddingMode = auto.paddingMode == PaddingMode.PixelsUI ? PaddingMode.PercentOfPanel : PaddingMode.PixelsUI;
            ApplyAll();
        }
        if (Input.GetKeyDown(toggleAutoFitKey))
        {
            if (auto) auto.autoFitCamera = !auto.autoFitCamera;
            ApplyAll();
        }
        if (Input.GetKeyDown(toggleFitModeKey))
        {
            if (auto) auto.fitMode = (auto.fitMode == FitMode.Contain) ? FitMode.Fill : FitMode.Contain;
            ApplyAll();
        }

        // 스케일
        if (Input.GetKeyDown(scalePlusKey)) { _scale = Mathf.Max(0.01f, _scale + scaleStep); ApplyAll(); }
        if (Input.GetKeyDown(scaleMinusKey)) { _scale = Mathf.Max(0.01f, _scale - scaleStep); ApplyAll(); }

        // 패딩 조정(Shift로 4방향 분리)
        // 좌/우: [ ]  | 상/하: ; '  (미국 배열 기준, 필요시 바꿔 쓰세요)
        if (Input.GetKeyDown(KeyCode.LeftBracket)) { AddPadding(left: +1); }
        if (Input.GetKeyDown(KeyCode.RightBracket)) { AddPadding(right: +1); }
        if (Input.GetKeyDown(KeyCode.Semicolon)) { AddPadding(top: +1); }
        if (Input.GetKeyDown(KeyCode.Quote)) { AddPadding(bottom: +1); }

        if (Input.GetKeyDown(resetKey)) ResetAll();
    }

    void AddPadding(int left = 0, int right = 0, int top = 0, int bottom = 0)
    {
        bool percent = auto && auto.paddingMode == PaddingMode.PercentOfPanel;
        float step = percent ? percentStep : pixelStep;
        Vector4 v = percent ? _padPct : _padPx;

        v.x += left * step;   // L
        v.y += right * step;   // R
        v.z += top * step;   // T
        v.w += bottom * step;   // B

        if (percent)
        {
            v.x = Mathf.Clamp01(v.x);
            v.y = Mathf.Clamp01(v.y);
            v.z = Mathf.Clamp01(v.z);
            v.w = Mathf.Clamp01(v.w);
            _padPct = v;
        }
        else
        {
            v.x = Mathf.Max(0, v.x);
            v.y = Mathf.Max(0, v.y);
            v.z = Mathf.Max(0, v.z);
            v.w = Mathf.Max(0, v.w);
            _padPx = v;
        }

        ApplyAll();
    }

    void ApplyPresetSmall()
    {
        if (auto) auto.paddingMode = PaddingMode.PixelsUI;
        _padPx = new Vector4(56, 56, 56, 56);
        _scale = 1.0f;
        ApplyAll();
    }

    void ApplyPresetMedium()
    {
        if (auto) auto.paddingMode = PaddingMode.PixelsUI;
        _padPx = new Vector4(32, 32, 40, 40);
        _scale = 1.0f;
        ApplyAll();
    }

    void ApplyPresetEdgeToEdge()
    {
        if (auto) auto.paddingMode = PaddingMode.PercentOfPanel;
        _padPct = new Vector4(0.02f, 0.02f, 0.03f, 0.03f);
        _scale = 1.0f;
        ApplyAll();
    }

    void ApplyPresetTall()
    {
        if (auto) auto.paddingMode = PaddingMode.PixelsUI;
        _padPx = new Vector4(80, 80, 20, 20);
        _scale = 1.0f;
        ApplyAll();
    }

    void ApplyPresetWide()
    {
        if (auto) auto.paddingMode = PaddingMode.PixelsUI;
        _padPx = new Vector4(20, 20, 80, 80);
        _scale = 1.0f;
        ApplyAll();
    }

    void ResetAll()
    {
        if (auto) auto.paddingMode = PaddingMode.PixelsUI;
        _padPx = new Vector4(24, 24, 24, 24);
        _padPct = new Vector4(0.05f, 0.05f, 0.05f, 0.05f);
        _scale = 1.0f;
        if (auto) { auto.autoFitCamera = true; auto.fitMode = FitMode.Contain; }
        ApplyAll();
    }

    void ApplyAll()
    {
        if (!host) return;

        if (auto)
        {
            auto.padding = _padPx;
            auto.paddingPercent = _padPct;
            auto.contentScale = _scale;

            // 패딩/스케일 적용
            auto.ApplyBeforeOpen();

            // 카메라 자동맞춤(옵션)
            if (auto.autoFitCamera && autoRefitCameraOnChange && host.Viewport && host.Viewport.cam)
            {
                var vp = host.Viewport.GetComponent<RectTransform>();
                var cam = host.Viewport.cam;
                if (cam.orthographic)
                {
                    PuzzleCamFitter.FitOrthoByAspect(cam, vp, auto.referenceAspect, auto.referenceOrthoSize, auto.fitMode, auto.cameraPaddingPct);
                }
                else if (sampleRootForBounds)
                {
                    PuzzleCamFitter.FitBoundsPerspective(cam, sampleRootForBounds, vp, auto.fitMode, auto.cameraPaddingPct);
                }
            }
        }
        else
        {
            // AutoLayout이 없으면 최소한 Viewport Stretch 보장
            var v = host.Viewport ? host.Viewport.transform as RectTransform : null;
            if (v)
            {
                v.anchorMin = Vector2.zero; v.anchorMax = Vector2.one; v.offsetMin = Vector2.zero; v.offsetMax = Vector2.zero;
            }
        }
    }

    void OnGUI()
    {
        if (!showOverlay || !host) return;

        var rt = host.ContentRoot;
        var vp = host.Viewport ? host.Viewport.GetComponent<RectTransform>() : null;
        var sz = rt ? rt.rect.size : Vector2.zero;
        var vpsz = vp ? vp.rect.size : Vector2.zero;

        string mode = auto ? (auto.paddingMode == PaddingMode.PixelsUI ? "Pixels" : "Percent") : "(no AutoLayout)";
        string pad = (auto && auto.paddingMode == PaddingMode.PercentOfPanel)
            ? $"L{_padPct.x:0.00} R{_padPct.y:0.00} T{_padPct.z:0.00} B{_padPct.w:0.00}"
            : $"L{_padPx.x:0} R{_padPx.y:0} T{_padPx.z:0} B{_padPx.w:0}";

        GUILayout.BeginArea(new Rect(12, 12, 460, 260), GUI.skin.box);
        GUILayout.Label("<b>Popup Size Tester</b>");
        GUILayout.Label($"ContentRoot: {sz.x:0} x {sz.y:0}");
        GUILayout.Label($"Viewport   : {vpsz.x:0} x {vpsz.y:0}");
        GUILayout.Space(6);
        GUILayout.Label($"PaddingMode: {mode}");
        GUILayout.Label($"Padding    : {pad}");
        GUILayout.Label($"Scale      : x{_scale:0.00}");
        if (auto && host.Viewport && host.Viewport.cam)
        {
            var cam = host.Viewport.cam;
            string camInfo = cam.orthographic ? $"OrthoSize={cam.orthographicSize:0.00}" : $"FOV={cam.fieldOfView:0.0}";
            GUILayout.Label($"Camera     : {(auto.autoFitCamera ? "AutoFit ON" : "AutoFit OFF")}  [{auto.fitMode}]  {camInfo}");
        }
        GUILayout.Space(6);
        GUILayout.Label("Hotkeys: F1~F5 presets, P:Mode, O:AutoFit, F:FitMode, +/-:Scale, [ ] ; ':Padding, R:Reset, F9:Overlay");
        GUILayout.EndArea();
    }
}