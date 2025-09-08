using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PopupHost : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] GameObject panelRoot;      // Panel(비/활성화 대상)
    [SerializeField] RectTransform contentRoot; // Panel/Content
    [SerializeField] CanvasGroup canvasGroup;   // 페이드(없으면 자동 추가)

    void Reset()
    {
        panelRoot = gameObject;
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public RectTransform ContentRoot => contentRoot;

    public void Show(bool instant = false) => StartCoroutine(FadeTo(1f, instant ? 0f : 0.12f));
    public void Hide(bool instant = false) => StartCoroutine(FadeTo(0f, instant ? 0f : 0.12f));

    IEnumerator FadeTo(float target, float dur)
    {
        if (!panelRoot) yield break;
        panelRoot.SetActive(true);
        if (!canvasGroup) canvasGroup = panelRoot.AddComponent<CanvasGroup>();

        float start = canvasGroup.alpha, t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, dur <= 0 ? 1f : t / dur);
            yield return null;
        }
        canvasGroup.alpha = target;
        if (Mathf.Approximately(target, 0f)) panelRoot.SetActive(false);
    }

    public void ClearContent()
    {
        if (!contentRoot) return;
        for (int i = contentRoot.childCount - 1; i >= 0; --i)
            Destroy(contentRoot.GetChild(i).gameObject);
    }
}
