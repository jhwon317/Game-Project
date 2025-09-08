using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PopupMini
{
    [RequireComponent(typeof(PopupHost))]
    public class PopupResizer : MonoBehaviour
    {
        public PopupHost Host;

        public enum SizingMode { PercentAnchors, FixedPixelsUI }  // 기본: PercentAnchors 권장
        public SizingMode Mode = SizingMode.PercentAnchors;

        void Reset() { Host = GetComponent<PopupHost>(); }

        public void Apply(PopupSizeOptions opt)
        {
            if (!Host || !Host.ContentRoot) return;

            var rt = Host.ContentRoot;
            var parent = rt.parent as RectTransform;
            if (!parent) return;

            // Viewport의 표시 모드도 같이 반영
            if (Host.Viewport) Host.Viewport.aspect = opt.AspectMode;

            switch (Mode)
            {
                case SizingMode.PercentAnchors:
                    ApplyPercentAnchors(rt, opt);
                    break;

                case SizingMode.FixedPixelsUI:
                    ApplyFixedPixelsUI(rt, opt);
                    break;
            }
        }

        // 부모 기준 %로 크기: 스케일러/해상도 무관, 가장 튼튼
        void ApplyPercentAnchors(RectTransform rt, PopupSizeOptions opt)
        {
            float px = Mathf.Clamp01(opt.PercentX <= 0 ? 0.6f : opt.PercentX);
            float py = Mathf.Clamp01(opt.PercentY <= 0 ? 0.6f : opt.PercentY);

            // 중앙 정렬(피벗 0.5, 0.5), 앵커로 크기를 표현
            rt.pivot = new Vector2(0.5f, 0.5f);

            Vector2 half = new Vector2(px, py) * 0.5f;
            Vector2 min = new Vector2(0.5f - half.x, 0.5f - half.y);
            Vector2 max = new Vector2(0.5f + half.x, 0.5f + half.y);

            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            // (선택) 최소/최대 픽셀 제한 적용: 앵커로 먼저 잡고 한 프레임 뒤 픽셀 한도 보정
            if (opt.MinSize != Vector2.zero || opt.MaxSize != Vector2.zero)
                StartCoroutine(CoClampAfterLayout(rt, opt));
        }

        // UI 단위(=sizeDelta) 고정: 스케일러 영향 받음 (가능하면 PercentAnchors 추천)
        void ApplyFixedPixelsUI(RectTransform rt, PopupSizeOptions opt)
        {
            var root = GetComponentInParent<Canvas>()?.rootCanvas;
            float scale = root ? Mathf.Max(0.0001f, root.scaleFactor) : 1f;

            // 기준 픽셀은 rootCanvas.pixelRect; SafeArea는 Overlay에서만 의미 있음
            Vector2 basePx = root ? root.pixelRect.size : new Vector2(Screen.width, Screen.height);
            if (opt.UseSafeArea && root && root.renderMode == RenderMode.ScreenSpaceOverlay)
                basePx = Screen.safeArea.size;

            float pxW = Mathf.Clamp(
                basePx.x * Mathf.Clamp01(opt.PercentX <= 0 ? 0.6f : opt.PercentX),
                (opt.MinSize.x <= 0 ? 1 : opt.MinSize.x),
                (opt.MaxSize.x <= 0 ? 999999 : opt.MaxSize.x)
            );
            float pxH = Mathf.Clamp(
                basePx.y * Mathf.Clamp01(opt.PercentY <= 0 ? 0.6f : opt.PercentY),
                (opt.MinSize.y <= 0 ? 1 : opt.MinSize.y),
                (opt.MaxSize.y <= 0 ? 999999 : opt.MaxSize.y)
            );

            if (opt.Aspect > 0f)
            {
                float ar = pxW / Mathf.Max(1f, pxH);
                if (ar > opt.Aspect) pxW = pxH * opt.Aspect;
                else pxH = pxW / opt.Aspect;
            }

            // UI 단위로 환산
            float uiW = pxW / scale;
            float uiH = pxH / scale;

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(uiW, uiH);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        IEnumerator CoClampAfterLayout(RectTransform rt, PopupSizeOptions opt)
        {
            // 레이아웃 확정 대기
            yield return null;
            Canvas.ForceUpdateCanvases();

            var root = GetComponentInParent<Canvas>()?.rootCanvas;
            float scale = root ? Mathf.Max(0.0001f, root.scaleFactor) : 1f;
            var sizeUI = rt.rect.size;
            var sizePx = sizeUI * scale;

            Vector2 minPx = new Vector2(opt.MinSize.x <= 0 ? 0 : opt.MinSize.x,
                                         opt.MinSize.y <= 0 ? 0 : opt.MinSize.y);
            Vector2 maxPx = new Vector2(opt.MaxSize.x <= 0 ? 999999 : opt.MaxSize.x,
                                         opt.MaxSize.y <= 0 ? 999999 : opt.MaxSize.y);

            sizePx = new Vector2(Mathf.Clamp(sizePx.x, minPx.x, maxPx.x),
                                 Mathf.Clamp(sizePx.y, minPx.y, maxPx.y));

            // 한도 보정은 앵커 유지한 채 sizeDelta로 미세 보정
            var finalUI = sizePx / scale;
            rt.sizeDelta = finalUI; // 앵커 퍼센트가 같은 상태에서도 오프셋 0이면 중앙 유지
        }

        // 레이아웃/스케일러 반영 뒤 호출하고 싶다면 이걸 사용
        public void ApplyDeferred(PopupSizeOptions opt, int settleFrames = 1)
        {
            StartCoroutine(CoDeferred(opt, settleFrames));
        }
        IEnumerator CoDeferred(PopupSizeOptions opt, int settleFrames)
        {
            for (int i = 0; i < Mathf.Max(1, settleFrames); i++)
            {
                yield return null;
                Canvas.ForceUpdateCanvases();
            }
            Apply(opt);
        }
    }
}
