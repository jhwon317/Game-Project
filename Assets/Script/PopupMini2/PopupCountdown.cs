using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupCountdown : MonoBehaviour
{
    [Header("UI (선택)")]
    public TMP_Text label;  // 텍스트 표시 (mm:ss.mmm)
    public Image fill;   // 진행도 게이지 (0~1)

    [Header("표시 옵션")]
    public bool useUnscaledTime = true; // 팝업은 보통 일시정지 무시
    public int maxMinutesForFill = 10; // 게이지 정규화 기준(분)

    Coroutine _co;

    public void StartCountdown(float seconds, CancellationToken ct)
    {
        Stop();
        gameObject.SetActive(true);
        _co = StartCoroutine(CoCountdown(seconds, ct));
    }

    public void Stop()
    {
        if (_co != null) { StopCoroutine(_co); _co = null; }
    }

    IEnumerator CoCountdown(float seconds, CancellationToken ct)
    {
        float t = Mathf.Max(0f, seconds);

        // 게이지 기준값 (선형)
        float fillDenom = Mathf.Max(0.001f, seconds);

        while (t > 0f && !ct.IsCancellationRequested)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t = Mathf.Max(0f, t - dt);

            // --- 텍스트: mm:ss.xx (hundredths) ---
            if (label)
            {
                // 안정적으로 내림 표시
                int totalHundredths = Mathf.Clamp((int)(t * 100f), 0, int.MaxValue);
                int minutes = totalHundredths / 6000;          // 60초 * 100
                int secondsI = (totalHundredths / 100) % 60;    // 초
                int hundredths = totalHundredths % 100;          // 00~99
                label.text = $"{minutes:00}:{secondsI:00}.{hundredths:00}";
            }

            // --- 게이지 ---
            if (fill) fill.fillAmount = 1f - Mathf.Clamp01((seconds - t) / fillDenom);

            yield return null;
        }

        // 마지막 프레임 정렬
        if (label) label.text = "00:00.00";
        if (fill) fill.fillAmount = 1f;
    }
}
