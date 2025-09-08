using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace PopupMini.Sample
{
    public class SimplePuzzleController : MonoBehaviour, PopupMini.IPuzzleController
    {
        public event Action<PopupMini.PuzzleResult> Completed;

        [Header("UI")]
        public Button OkButton;
        public Button CancelButton;

        public void Begin(object args, CancellationToken ct)
        {
            if (OkButton) OkButton.onClick.AddListener(() => Completed?.Invoke(PopupMini.PuzzleResult.Ok()));
            if (CancelButton) CancelButton.onClick.AddListener(() => Completed?.Invoke(PopupMini.PuzzleResult.Cancel("abort:user")));

            // 키보드 백업
            StartCoroutine(KeyWatch(ct));
        }

        System.Collections.IEnumerator KeyWatch(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (Input.GetKeyDown(KeyCode.Return)) { Completed?.Invoke(PopupMini.PuzzleResult.Ok()); yield break; }
                if (Input.GetKeyDown(KeyCode.Escape)) { Completed?.Invoke(PopupMini.PuzzleResult.Cancel("abort:user")); yield break; }
                yield return null;
            }
            Completed?.Invoke(PopupMini.PuzzleResult.Cancel("abort:external"));
        }
    }
}