using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

namespace PopupMini
{
    /// <summary>
    /// 퍼즐 카메라 출력을 RenderTexture로 받아 RawImage에 표시.
    /// - 레이아웃/CanvasScaler 반영을 위해 '한 프레임 지연' 후 크기 확정
    /// - RT 해상도: 뷰포트 매칭 / 배율 / 고정 픽셀 지원
    /// - URP 카메라 Base 고정 & Stack 클리어
    /// - 크기/계층 변화 시 자동 재생성
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(RawImage))]
    public class CamToRawImage : MonoBehaviour
    {
        // --------- 바인딩/표시 옵션 ---------
        public Camera cam;
        public AspectMode aspect = AspectMode.FillCrop;  // Stretch / FitContain / FillCrop
        [Min(1)] public int aa = 1;                      // Anti Aliasing
        public FilterMode filter = FilterMode.Point;
        public bool transparentBG = true;

        public enum SizeMode { MatchViewport, ScaleBy, FixedPixels }
        [Header("RT Size")]
        public SizeMode sizeMode = SizeMode.MatchViewport;
        [Range(0.25f, 4f)] public float resolutionScale = 1f;     // ScaleBy에서 사용
        public Vector2Int fixedPixels = new Vector2Int(1024, 1024); // FixedPixels에서 사용
        public Vector2Int minPixels = new Vector2Int(64, 64);
        public Vector2Int maxPixels = new Vector2Int(4096, 4096);

        // --------- 내부 상태 ---------
        RenderTexture _rt;
        RectTransform _rtf;
        Canvas _root;            // rootCanvas
        RawImage _img;
        bool _pending;           // 다음 프레임에 Rebuild 예약

        void Awake()
        {
            _rtf = (RectTransform)transform;
            _img = GetComponent<RawImage>();
            _root = GetComponentInParent<Canvas>()?.rootCanvas;
        }

        void OnEnable() => RequestRebuild();
        void OnDisable() => Release();

        // 뷰포트 크기/캔버스 변경에 반응
        void OnRectTransformDimensionsChange() { if (isActiveAndEnabled) RequestRebuild(); }
        void OnCanvasHierarchyChanged() { if (isActiveAndEnabled) RequestRebuild(); }

        /// <summary>
        /// 세션/서비스에서 호출: 카메라/표시 옵션 바인딩
        /// aspectRatio 인자는 외부에서 필요 시 전달되지만, RT 생성은 실제 RectTransform/CanvasScale로 계산합니다.
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
            RequestRebuild();     // 즉시가 아니라 한 프레임 뒤에 확정
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
            // 레이아웃/CanvasScaler가 모두 적용되도록 1프레임 대기
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
            // URP: Base 카메라 고정 및 스택 정리(오프스크린 안전)
            var urp = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urp) { urp.renderType = CameraRenderType.Base; urp.cameraStack.Clear(); }
#endif
            // 투명 BG
            if (transparentBG)
            {
                var c = cam.backgroundColor; c.a = 0f;
                cam.backgroundColor = c;
            }

            // 최종 UI 픽셀 크기 계산
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

            // RT 생성/갱신
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
            // 디버그(원하면 주석 해제)
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
                    // 잘림 없이 전체 표시(레터/필러는 부모 배경으로 처리)
                    uv = new Rect(0, 0, 1, 1);
                    break;

                case AspectMode.FillCrop:
                    // 꽉 채우되 넘치는 쪽을 크롭
                    if (viewAR > texAR)
                    {
                        // 가로가 더 넓음 → 세로 크롭
                        float v = texAR / viewAR;         // 표시될 V 높이(0~1)
                        float off = (1f - v) * 0.5f;
                        uv = new Rect(0, off, 1, v);
                    }
                    else
                    {
                        // 세로가 더 큼 → 가로 크롭
                        float u = viewAR / texAR;         // 표시될 U 폭(0~1)
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
