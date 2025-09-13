using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupCountdown : MonoBehaviour
{
    [Header("UI (����)")]
    public TMP_Text label;  // �ؽ�Ʈ ǥ�� (mm:ss.mmm)
    public Image fill;   // ���൵ ������ (0~1)

    [Header("ǥ�� �ɼ�")]
    public bool useUnscaledTime = true; // �˾��� ���� �Ͻ����� ����
    public int maxMinutesForFill = 10; // ������ ����ȭ ����(��)

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

        // ������ ���ذ� (����)
        float fillDenom = Mathf.Max(0.001f, seconds);

        while (t > 0f && !ct.IsCancellationRequested)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t = Mathf.Max(0f, t - dt);

            // --- �ؽ�Ʈ: mm:ss.mmm ---
            if (label)
            {
                // �ݿø� ��� �������� ��0.001�� ������ ���� ǥ�� ����
                int totalMs = Mathf.Clamp((int)(t * 1000f), 0, int.MaxValue);
                int minutes = totalMs / 60000;
                int secondsI = (totalMs / 1000) % 60;
                int millis = totalMs % 100;
                label.text = $"{minutes:00}:{secondsI:00}.{millis:00}";
            }

            // --- ������ ---
            if (fill) fill.fillAmount = 1f - Mathf.Clamp01((seconds - t) / fillDenom);

            yield return null;
        }

        // ������ ������ ����
        if (label) label.text = "00:00.00";
        if (fill) fill.fillAmount = 1f;
    }
}
