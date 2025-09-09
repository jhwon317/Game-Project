using System.Collections;
using UnityEngine;

namespace PopupMini
{
    [DisallowMultipleComponent]
    public class PopupHost : MonoBehaviour
    {
        [Header("Wiring")]
        public GameObject PanelRoot;    // 전체 오버레이(배경)
        public RectTransform ContentRoot;  // 창 영역(패딩/스케일 대상)
        public CamToRawImage Viewport;     // RawImage + CamToRawImage
        public CanvasGroup CanvasGroup;  // 페이드

        void Reset()
        {
            if (!PanelRoot) PanelRoot = gameObject;
            EnsureCanvasGroup();
        }

        void EnsureCanvasGroup()
        {
            if (!PanelRoot) return;
            if (!CanvasGroup)
            {
                CanvasGroup = PanelRoot.GetComponent<CanvasGroup>();
                if (!CanvasGroup) CanvasGroup = PanelRoot.AddComponent<CanvasGroup>();
            }
        }

        public void Show(bool instant = false)
        {
            if (!PanelRoot) return;
            EnsureCanvasGroup();

            // Host 비활성/갱신 불가일 때: 즉시 처리(코루틴 금지)
            if (!isActiveAndEnabled)
            {
                PanelRoot.SetActive(true);
                CanvasGroup.alpha = 1f;
                return;
            }

            StopAllCoroutines();
            StartCoroutine(FadeTo(1f, instant ? 0f : 0.12f));
        }

        public void Hide(bool instant = false)
        {
            if (!PanelRoot) return;
            EnsureCanvasGroup();

            if (!isActiveAndEnabled)
            {
                CanvasGroup.alpha = 0f;
                PanelRoot.SetActive(false);
                return;
            }

            StopAllCoroutines();
            StartCoroutine(FadeTo(0f, instant ? 0f : 0.12f));
        }

        IEnumerator FadeTo(float target, float dur)
        {
            PanelRoot.SetActive(true);
            EnsureCanvasGroup();

            float s = CanvasGroup.alpha, t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                CanvasGroup.alpha = Mathf.Lerp(s, target, dur <= 0 ? 1f : t / dur);
                yield return null;
            }
            CanvasGroup.alpha = target;

            if (Mathf.Approximately(target, 0f))
                PanelRoot.SetActive(false);
        }
    }
}