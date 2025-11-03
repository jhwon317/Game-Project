using UnityEngine;
using UnityEngine.UI;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

namespace PopupMini
{
    public enum AspectMode { Stretch, FitContain, FillCrop }

    /// <summary>
    /// 카메라를 RawImage에 렌더링하고 자동으로 RenderTexture 관리
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class CamToRawImage : MonoBehaviour
    {
        public Camera cam;
        public AspectMode aspect = AspectMode.FitContain;
        public int aa = 1;
        public FilterMode filter = FilterMode.Bilinear;
        public bool transparentBG = true;

        RenderTexture _rt;
        RawImage _img;
        RectTransform _rtf;
        Canvas _root;

        void Awake()
        {
            _img = GetComponent<RawImage>();
            _rtf = (RectTransform)transform;
            _root = GetComponentInParent<Canvas>()?.rootCanvas;
        }

        void OnEnable() { Rebuild(); }
        void OnDisable() { Release(); }
        void OnRectTransformDimensionsChange() { if (isActiveAndEnabled) Rebuild(); }

        public void Bind(Camera newCam, AspectMode mode, float aspectRatio, int antiAliasing, FilterMode filt, Color? bg = null)
        {
            cam = newCam;
            aspect = mode;
            aa = Mathf.Max(1, antiAliasing);
            filter = filt;

            if (!cam) 
            { 
                _img.texture = null; 
                Debug.LogWarning("[CamToRawImage] Bind called with null camera!");
                return; 
            }

#if UNITY_RENDER_PIPELINE_UNIVERSAL
            var urp = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urp) 
            { 
                urp.renderType = CameraRenderType.Base; 
                urp.cameraStack.Clear(); 
            }
#endif
            if (bg.HasValue)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = bg.Value;
            }

            Rebuild();
            cam.enabled = true; // 이제 바인딩 시에만 활성화
        }

        public void Unbind()
        {
            if (cam)
            {
                cam.targetTexture = null;
                cam.enabled = false; // 언바인딩 시 항상 비활성화
            }
            if (_img) _img.texture = null;
        }

        void Rebuild()
        {
            if (!_img || !gameObject.activeInHierarchy) return;
            if (!cam) 
            { 
                _img.texture = null; 
                return; 
            }

            float scale = _root ? Mathf.Max(0.0001f, _root.scaleFactor) : 1f;
            var size = _rtf.rect.size * scale;
            int w = Mathf.Max(2, Mathf.RoundToInt(size.x));
            int h = Mathf.Max(2, Mathf.RoundToInt(size.y));

            // RenderTexture 재생성 필요 여부 체크
            if (_rt && (_rt.width != w || _rt.height != h || _rt.antiAliasing != aa))
            {
                _rt.Release(); 
                Destroy(_rt); 
                _rt = null;
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

            if (transparentBG)
            {
                var c = cam.backgroundColor; 
                c.a = 0f; 
                cam.backgroundColor = c;
            }

            cam.targetTexture = _rt;
            _img.texture = _rt;

            ApplyAspectUV();
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
                    // 전체가 보이도록 레터박스/필러박스
                    uv = new Rect(0, 0, 1, 1); 
                    break;
                    
                case AspectMode.FillCrop:
                    // 화면을 꽉 채우도록 크롭
                    if (viewAR > texAR)
                    {
                        // View가 더 넓음 → 높이를 크롭
                        float v = texAR / viewAR; 
                        float off = (1f - v) * 0.5f;
                        uv = new Rect(0, off, 1, v);
                    }
                    else
                    {
                        // View가 더 높음 → 너비를 크롭
                        float u = viewAR / texAR; 
                        float off = (1f - u) * 0.5f;
                        uv = new Rect(off, 0, u, 1);
                    }
                    break;
            }
            
            _img.uvRect = uv;
        }

        void Release()
        {
            if (cam) 
            { 
                cam.targetTexture = null; 
                cam.enabled = false; 
            }
            if (_rt) 
            { 
                _rt.Release(); 
                Destroy(_rt); 
                _rt = null; 
            }
            if (_img) _img.texture = null;
        }

        void OnDestroy()
        {
            Release();
        }
    }
}
