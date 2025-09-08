using UnityEngine;
using UnityEngine.UI;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class CamToRawImage : MonoBehaviour
{
    public Camera cam;                 // �� PuzzleCam �巡��
    public bool transparentBG = true;  // ī�޶� ��� ����

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

        // (URP) �ݵ�� Base ī�޶�
#if UNITY_RENDER_PIPELINE_UNIVERSAL
        var urp = cam.GetComponent<UniversalAdditionalCameraData>();
        if (urp){ urp.renderType = CameraRenderType.Base; urp.cameraStack.Clear(); }
#endif

        // RawImage �ȼ� ũ�� ���(ĵ���� ������ �ݿ�)
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
            _rt.filterMode = FilterMode.Point; // UI ����
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
