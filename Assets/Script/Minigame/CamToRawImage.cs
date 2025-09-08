using UnityEngine;
using UnityEngine.UI;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class CamToRawImage : MonoBehaviour
{
    public enum AspectMode { Stretch, FitContain, FillCrop }

    [Header("Binding")]
    public Camera cam;                 // 외부에서 Bind()로 세팅 권장
    public bool transparentBG = true;  // 카메라 배경 투명 처리
    public AspectMode aspect = AspectMode.FillCrop;

    RenderTexture _rt;
    RectTransform _rtf;
    Canvas _root;
    RawImage _img;

    void Awake()
    {
        _rtf = (RectTransform)transform;
        _root = GetComponentInParent<Canvas>()?.rootCanvas;
        _img = GetComponent<RawImage>();
    }

    void OnEnable() { Rebuild(); }
    void OnDisable() { Release(); }
    void OnRectTransformDimensionsChange() { if (isActiveAndEnabled) Rebuild(); }

    public void Bind(Camera newCam) { cam = newCam; Rebuild(); }
    public void Unbind()
    {
        if (cam) cam.targetTexture = null;
        if (_img) _img.texture = null;
    }

    void Rebuild()
    {
        if (!_img || !gameObject.activeInHierarchy) return;
        if (!cam) { _img.texture = null; return; }

#if UNITY_RENDER_PIPELINE_UNIVERSAL
        var urp = cam.GetComponent<UniversalAdditionalCameraData>();
        if (urp){ urp.renderType = CameraRenderType.Base; urp.cameraStack.Clear(); }
#endif

        float scale = _root ? _root.scaleFactor : 1f;
        var size = _rtf.rect.size * scale;
        int w = Mathf.Max(2, Mathf.RoundToInt(size.x));
        int h = Mathf.Max(2, Mathf.RoundToInt(size.y));

        if (_rt && (_rt.width != w || _rt.height != h)) { _rt.Release(); Destroy(_rt); _rt = null; }
        if (_rt == null)
        {
            _rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32)
            { antiAliasing = 1 };
            _rt.Create();
            _rt.filterMode = FilterMode.Point; // UI 선명도
        }

        if (transparentBG)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            var c = cam.backgroundColor; c.a = 0f; cam.backgroundColor = c;
        }

        cam.targetTexture = _rt;
        _img.texture = _rt;

        ApplyAspectUV();
    }

    void ApplyAspectUV()
    {
        if (!_img || !_img.texture) { if (_img) _img.uvRect = new Rect(0, 0, 1, 1); return; }

        float viewW = Mathf.Max(1, _rtf.rect.width);
        float viewH = Mathf.Max(1, _rtf.rect.height);
        float viewAR = viewW / viewH;

        float texW = _img.texture.width;
        float texH = _img.texture.height;
        float texAR = texW / texH;

        var uv = new Rect(0, 0, 1, 1);

        switch (aspect)
        {
            case AspectMode.Stretch:
                uv = new Rect(0, 0, 1, 1);
                break;

            case AspectMode.FitContain:
                uv = new Rect(0, 0, 1, 1); // 레터/필러박스는 부모 배경으로 처리
                break;

            case AspectMode.FillCrop:
                if (viewAR > texAR)
                {
                    float visibleV = texAR / viewAR;
                    float vOff = (1f - visibleV) * 0.5f;
                    uv = new Rect(0f, vOff, 1f, visibleV);
                }
                else
                {
                    float visibleU = viewAR / texAR;
                    float uOff = (1f - visibleU) * 0.5f;
                    uv = new Rect(uOff, 0f, visibleU, 1f);
                }
                break;
        }

        _img.uvRect = uv;
    }

    void Release()
    {
        if (cam) cam.targetTexture = null;
        if (_img) _img.texture = null;
        if (_rt) { _rt.Release(); Destroy(_rt); _rt = null; }
    }
}
