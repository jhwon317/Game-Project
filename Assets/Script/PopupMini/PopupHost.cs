using System.Collections;
using UnityEngine;

namespace PopupMini
{
    [DisallowMultipleComponent]
    public class PopupHost : MonoBehaviour
    {
        [Header("Wiring")]
        public GameObject PanelRoot;           // Panel
        public RectTransform ContentRoot;      // Panel/Content
        public CamToRawImage Viewport;         // Panel/Content/Viewport
        public CanvasGroup CanvasGroup;        // ∆‰¿ÃµÂ

        void Reset()
        {
            PanelRoot = gameObject;
            CanvasGroup = GetComponent<CanvasGroup>();
            if (!CanvasGroup) CanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void Show(bool instant = false) => StartCoroutine(FadeTo(1f, instant ? 0f : 0.12f));
        public void Hide(bool instant = false) => StartCoroutine(FadeTo(0f, instant ? 0f : 0.12f));

        IEnumerator FadeTo(float target, float dur)
        {
            if (!PanelRoot) yield break;
            PanelRoot.SetActive(true);
            if (!CanvasGroup) CanvasGroup = PanelRoot.AddComponent<CanvasGroup>();
            float s = CanvasGroup.alpha, t = 0f;
            while (t < dur) { t += Time.unscaledDeltaTime; CanvasGroup.alpha = Mathf.Lerp(s, target, dur <= 0 ? 1 : t / dur); yield return null; }
            CanvasGroup.alpha = target;
            if (Mathf.Approximately(target, 0f)) PanelRoot.SetActive(false);
        }
    }
}