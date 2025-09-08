using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PopupMini
{
    [RequireComponent(typeof(PopupHost))]
    public class PopupResizer : MonoBehaviour
    {
        public PopupHost Host;

        public enum SizingMode { PercentAnchors, FixedPixelsUI }  // �⺻: PercentAnchors ����
        public SizingMode Mode = SizingMode.PercentAnchors;

        void Reset() { Host = GetComponent<PopupHost>(); }

        public void Apply(PopupSizeOptions opt)
        {
            if (!Host || !Host.ContentRoot) return;

            var rt = Host.ContentRoot;
            var parent = rt.parent as RectTransform;
            if (!parent) return;

            // Viewport�� ǥ�� ��嵵 ���� �ݿ�
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

        // �θ� ���� %�� ũ��: �����Ϸ�/�ػ� ����, ���� ưư
        void ApplyPercentAnchors(RectTransform rt, PopupSizeOptions opt)
        {
            float px = Mathf.Clamp01(opt.PercentX <= 0 ? 0.6f : opt.PercentX);
            float py = Mathf.Clamp01(opt.PercentY <= 0 ? 0.6f : opt.PercentY);

            // �߾� ����(�ǹ� 0.5, 0.5), ��Ŀ�� ũ�⸦ ǥ��
            rt.pivot = new Vector2(0.5f, 0.5f);

            Vector2 half = new Vector2(px, py) * 0.5f;
            Vector2 min = new Vector2(0.5f - half.x, 0.5f - half.y);
            Vector2 max = new Vector2(0.5f + half.x, 0.5f + half.y);

            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            // (����) �ּ�/�ִ� �ȼ� ���� ����: ��Ŀ�� ���� ��� �� ������ �� �ȼ� �ѵ� ����
            if (opt.MinSize != Vector2.zero || opt.MaxSize != Vector2.zero)
                StartCoroutine(CoClampAfterLayout(rt, opt));
        }

        // UI ����(=sizeDelta) ����: �����Ϸ� ���� ���� (�����ϸ� PercentAnchors ��õ)
        void ApplyFixedPixelsUI(RectTransform rt, PopupSizeOptions opt)
        {
            var root = GetComponentInParent<Canvas>()?.rootCanvas;
            float scale = root ? Mathf.Max(0.0001f, root.scaleFactor) : 1f;

            // ���� �ȼ��� rootCanvas.pixelRect; SafeArea�� Overlay������ �ǹ� ����
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

            // UI ������ ȯ��
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
            // ���̾ƿ� Ȯ�� ���
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

            // �ѵ� ������ ��Ŀ ������ ä sizeDelta�� �̼� ����
            var finalUI = sizePx / scale;
            rt.sizeDelta = finalUI; // ��Ŀ �ۼ�Ʈ�� ���� ���¿����� ������ 0�̸� �߾� ����
        }

        // ���̾ƿ�/�����Ϸ� �ݿ� �� ȣ���ϰ� �ʹٸ� �̰� ���
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
