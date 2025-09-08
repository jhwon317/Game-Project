using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

namespace PopupMini
{
    /// <summary>
    /// ���� ī�޶� ����� RenderTexture�� �޾� RawImage�� ǥ��.
    /// - ���̾ƿ�/CanvasScaler �ݿ��� ���� '�� ������ ����' �� ũ�� Ȯ��
    /// - RT �ػ�: ����Ʈ ��Ī / ���� / ���� �ȼ� ����
    /// - URP ī�޶� Base ���� & Stack Ŭ����
    /// - ũ��/���� ��ȭ �� �ڵ� �����
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(RawImage))]
    public class CamToRawImage : MonoBehaviour
    {
        // --------- ���ε�/ǥ�� �ɼ� ---------
        public Camera cam;
        public AspectMode aspect = AspectMode.FillCrop;  // Stretch / FitContain / FillCrop
        [Min(1)] public int aa = 1;                      // Anti Aliasing
        public FilterMode filter = FilterMode.Point;
        public bool transparentBG = true;

        public enum SizeMode { MatchViewport, ScaleBy, FixedPixels }
        [Header("RT Size")]
        public SizeMode sizeMode = SizeMode.MatchViewport;
        [Range(0.25f, 4f)] public float resolutionScale = 1f;     // ScaleBy���� ���
        public Vector2Int fixedPixels = new Vector2Int(1024, 1024); // FixedPixels���� ���
        public Vector2Int minPixels = new Vector2Int(64, 64);
        public Vector2Int maxPixels = new Vector2Int(4096, 4096);

        // --------- ���� ���� ---------
        RenderTexture _rt;
        RectTransform _rtf;
        Canvas _root;            // rootCanvas
        RawImage _img;
        bool _pending;           // ���� �����ӿ� Rebuild ����

        void Awake()
        {
            _rtf = (RectTransform)transform;
            _img = GetComponent<RawImage>();
            _root = GetComponentInParent<Canvas>()?.rootCanvas;
        }

        void OnEnable() => RequestRebuild();
        void OnDisable() => Release();

        // ����Ʈ ũ��/ĵ���� ���濡 ����
        void OnRectTransformDimensionsChange() { if (isActiveAndEnabled) RequestRebuild(); }
        void OnCanvasHierarchyChanged() { if (isActiveAndEnabled) RequestRebuild(); }

        /// <summary>
        /// ����/���񽺿��� ȣ��: ī�޶�/ǥ�� �ɼ� ���ε�
        /// aspectRatio ���ڴ� �ܺο��� �ʿ� �� ���޵�����, RT ������ ���� RectTransform/CanvasScale�� ����մϴ�.
        /// </summary>
        public void Bind(Camera newCam, AspectMode mode, float aspectRatio, int antiAliasing,
                         FilterMode filt, Color? bg = null)
        {
            cam = newCam;
            aspect = mode;
            aa = Mathf.Max(1, antiAliasing);
            filter = filt;

            if (bg.HasValue)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = bg.Value;
            }
            RequestRebuild();     // ��ð� �ƴ϶� �� ������ �ڿ� Ȯ��
        }

        public void Unbind()
        {
            if (cam) cam.targetTexture = null;
            if (_img) _img.texture = null;
        }

        void RequestRebuild()
        {
            if (!isActiveAndEnabled || _pending) return;
            _pending = true;
            StartCoroutine(CoRebuildNextFrame());
        }

        IEnumerator CoRebuildNextFrame()
        {
            // ���̾ƿ�/CanvasScaler�� ��� ����ǵ��� 1������ ���
            yield return null;
            Canvas.ForceUpdateCanvases();
            _pending = false;
            Rebuild();
        }

        void Rebuild()
        {
            if (!_img || !gameObject.activeInHierarchy) return;
            if (!cam) { _img.texture = null; return; }

#if UNITY_RENDER_PIPELINE_UNIVERSAL
            // URP: Base ī�޶� ���� �� ���� ����(������ũ�� ����)
            var urp = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urp) { urp.renderType = CameraRenderType.Base; urp.cameraStack.Clear(); }
#endif
            // ���� BG
            if (transparentBG)
            {
                var c = cam.backgroundColor; c.a = 0f;
                cam.backgroundColor = c;
            }

            // ���� UI �ȼ� ũ�� ���
            float scale = _root ? _root.scaleFactor : 1f;
            var uiSize = _rtf.rect.size;
            int w, h;

            switch (sizeMode)
            {
                case SizeMode.FixedPixels:
                    w = fixedPixels.x; h = fixedPixels.y;
                    break;
                case SizeMode.ScaleBy:
                    w = Mathf.RoundToInt(uiSize.x * scale * resolutionScale);
                    h = Mathf.RoundToInt(uiSize.y * scale * resolutionScale);
                    break;
                default: // MatchViewport
                    w = Mathf.RoundToInt(uiSize.x * scale);
                    h = Mathf.RoundToInt(uiSize.y * scale);
                    break;
            }

            w = Mathf.Clamp(Mathf.Max(2, w), minPixels.x, maxPixels.x);
            h = Mathf.Clamp(Mathf.Max(2, h), minPixels.y, maxPixels.y);

            // RT ����/����
            if (_rt && (_rt.width != w || _rt.height != h || _rt.antiAliasing != aa))
            {
                _img.texture = null;
                _rt.Release(); Destroy(_rt); _rt = null;
            }
            if (_rt == null)
            {
                _rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32)
                {
                    antiAliasing = aa,
                    filterMode = filter
                };
                _rt.Create();
            }

            cam.targetTexture = _rt;
            _img.texture = _rt;

            ApplyAspectUV();
            // �����(���ϸ� �ּ� ����)
            // Debug.Log($"[RTDiag] viewUI=({uiSize.x}x{uiSize.y}), scaleFactor={scale}, rt=({w}x{h})");
        }

        void ApplyAspectUV()
        {
            if (!_img || !_img.texture)
            {
                if (_img) _img.uvRect = new Rect(0, 0, 1, 1);
                return;
            }

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
                    // �߸� ���� ��ü ǥ��(����/�ʷ��� �θ� ������� ó��)
                    uv = new Rect(0, 0, 1, 1);
                    break;

                case AspectMode.FillCrop:
                    // �� ä��� ��ġ�� ���� ũ��
                    if (viewAR > texAR)
                    {
                        // ���ΰ� �� ���� �� ���� ũ��
                        float v = texAR / viewAR;         // ǥ�õ� V ����(0~1)
                        float off = (1f - v) * 0.5f;
                        uv = new Rect(0, off, 1, v);
                    }
                    else
                    {
                        // ���ΰ� �� ŭ �� ���� ũ��
                        float u = viewAR / texAR;         // ǥ�õ� U ��(0~1)
                        float off = (1f - u) * 0.5f;
                        uv = new Rect(off, 0, u, 1);
                    }
                    break;
            }

            _img.uvRect = uv;
        }

        void Release()
        {
            if (cam) cam.targetTexture = null;
            if (_img) _img.texture = null;
            if (_rt)
            {
                _rt.Release();
                Destroy(_rt);
                _rt = null;
            }
            _pending = false;
        }
    }
}
