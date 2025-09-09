using UnityEngine;
using PopupMini;

/// <summary>
/// ��Ÿ�ӿ��� �˾� ������/�е�/ī�޶� ������ �Ｎ���� �׽�Ʈ�ϴ� ����.
/// - Panel �е�: ContentRoot.offset
/// - Content ������: ContentRoot.localScale
/// - ī�޶� �ڵ�����: Ortho�� ���, Persp�� �ٿ��� ��Ʈ�� �ʿ�(�ɼ�).
/// �ܵ����� ����. Host/AutoLayout ������ ������ ��.
/// </summary>
[DisallowMultipleComponent]
public class PopupSizeTester : MonoBehaviour
{
    [Header("Wiring")]
    public PopupHost host;                  // Panel/Content/Viewport
    public PopupAutoLayout auto;            // �е�/������/ī�޶� �ɼ�
    [Tooltip("3D ����(�۽���Ƽ��)�� �� �ٿ�� ���� ��Ʈ(�������̸� ��ŵ).")]
    public Transform sampleRootForBounds;   // ����

    [Header("Overlay")]
    public bool showOverlay = true;
    public bool autoRefitCameraOnChange = true;

    // ����Ű(���ϴ´�� �ٲ㵵 ��)
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

    // ���� ����
    [Header("Adjust Steps")]
    public float pixelStep = 16f;
    public float percentStep = 0.02f;
    public float scaleStep = 0.05f;

    // ���� ����(�е��� L,R,T,B ��)
    Vector4 _padPx = new Vector4(24, 24, 24, 24);
    Vector4 _padPct = new Vector4(0.05f, 0.05f, 0.05f, 0.05f);
    float _scale = 1f;

    void Awake()
    {
        if (!host) host = GetComponent<PopupHost>();
        if (!auto && host) auto = host.GetComponent<PopupAutoLayout>();
        if (!auto) Debug.LogWarning("[PopupSizeTester] PopupAutoLayout ������ �����. �е�/������ �ݿ��� �ؿ�.");

        // �ʱⰪ�� AutoLayout���� �޾ƿ�
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

        // ������
        if (Input.GetKeyDown(preset1Key)) ApplyPresetSmall();
        if (Input.GetKeyDown(preset2Key)) ApplyPresetMedium();
        if (Input.GetKeyDown(preset3Key)) ApplyPresetEdgeToEdge();
        if (Input.GetKeyDown(preset4Key)) ApplyPresetTall();
        if (Input.GetKeyDown(preset5Key)) ApplyPresetWide();

        // ��� ���
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

        // ������
        if (Input.GetKeyDown(scalePlusKey)) { _scale = Mathf.Max(0.01f, _scale + scaleStep); ApplyAll(); }
        if (Input.GetKeyDown(scaleMinusKey)) { _scale = Mathf.Max(0.01f, _scale - scaleStep); ApplyAll(); }

        // �е� ����(Shift�� 4���� �и�)
        // ��/��: [ ]  | ��/��: ; '  (�̱� �迭 ����, �ʿ�� �ٲ� ������)
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

            // �е�/������ ����
            auto.ApplyBeforeOpen();

            // ī�޶� �ڵ�����(�ɼ�)
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
            // AutoLayout�� ������ �ּ��� Viewport Stretch ����
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