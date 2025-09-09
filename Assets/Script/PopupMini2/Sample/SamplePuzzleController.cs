using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace PopupMini.Sample
{
    public class SimplePuzzleController : MonoBehaviour, IPuzzleController
    {
        public event Action<PuzzleResult> Completed;

        [Header("UI")]
        public Button OkButton;
        public Button CancelButton;

        public void Begin(object args, CancellationToken ct)
        {
            if (OkButton) OkButton.onClick.AddListener(() => Completed?.Invoke(PuzzleResult.Ok()));
            if (CancelButton) CancelButton.onClick.AddListener(() => Completed?.Invoke(PuzzleResult.Cancel("cancel:user")));

            StartCoroutine(KeyWatch(ct));
        }

        System.Collections.IEnumerator KeyWatch(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (Input.GetKeyDown(KeyCode.Return)) { Completed?.Invoke(PuzzleResult.Ok()); yield break; }
                if (Input.GetKeyDown(KeyCode.Escape)) { Completed?.Invoke(PuzzleResult.Cancel("cancel:user")); yield break; }
                yield return null;
            }
            Completed?.Invoke(PuzzleResult.Cancel("abort:external"));
        }
    }
}