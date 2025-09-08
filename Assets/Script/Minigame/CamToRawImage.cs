using UnityEngine;
using UnityEngine.UI;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class CamToRawImage : MonoBehaviour
{
    public Camera cam;                 // ← PuzzleCam 드래그
    public bool transparentBG = true;  // 카메라 배경 투명

    RenderTexture _rt;
    RectTransform _rtf; Canvas _root; RawImage _img;

    void Awake()
    {
        _rtf = (RectTransform)transform;
        _root = GetComponentInParent<Canvas>()?.rootCanvas;
        _img = GetComponent<RawImage>();
    }

    void OnEnable() { Rebuild(); }
    void OnDisable() { Release(); }
    void OnRectTransformDimensionsChange() { if (isActiveAndEnabled) Rebuild(); }

    void Rebuild()
    {
        if (!cam || !_img) return;

        // (URP) 반드시 Base 카메라
#if UNITY_RENDER_PIPELINE_UNIVERSAL
        var urp = cam.GetComponent<UniversalAdditionalCameraData>();
        if (urp){ urp.renderType = CameraRenderType.Base; urp.cameraStack.Clear(); }
#endif

        // RawImage 픽셀 크기 계산(캔버스 스케일 반영)
        float scale = _root ? _root.scaleFactor : 1f;
        var size = _rtf.rect.size * scale;
        int w = Mathf.Max(2, Mathf.RoundToInt(size.x));
        int h = Mathf.Max(2, Mathf.RoundToInt(size.y));

        if (_rt && (_rt.width != w || _rt.height != h)) Release();
        if (_rt == null)
        {
            _rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1
            };
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
    }

    void Release()
    {
        if (cam) cam.targetTexture = null;
        if (_img) _img.texture = null;
        if (_rt) { _rt.Release(); Destroy(_rt); _rt = null; }
    }
}
