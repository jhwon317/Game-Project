// Assets/Script/PopupMini2/FirePin/FirePinUIPuzzleController.cs
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace PopupMini.Sample
{
    public class FirePinUIPuzzleController : MonoBehaviour, PopupMini.IPuzzleController
    {
        public event Action<PopupMini.PuzzleResult> Completed;

        [Header("Wiring")]
        public Camera puzzleCamera;          // 퍼즐 캔버스가 World/ScreenSpaceCamera면 worldCamera로 지정
        public Canvas puzzleCanvas;

        [Header("Pin")]
        public UIPinDraggableHover pin;      // ← 이거 하나만 연결

        [Header("SFX (optional)")]
        public AudioSource sfx;
        public AudioClip sfxGrab, sfxRelease, sfxSuccess;

        CancellationToken _ct;
        bool _finished;

        void Awake()
        {
            if (!puzzleCamera) puzzleCamera = GetComponentInChildren<Camera>(true);
            if (!puzzleCanvas) puzzleCanvas = GetComponentInChildren<Canvas>(true);
            if (!pin) pin = GetComponentInChildren<UIPinDraggableHover>(true);
        }

        public void Begin(object args, CancellationToken ct)
        {
            _ct = ct;

            if (puzzleCanvas &&
                (puzzleCanvas.renderMode == RenderMode.WorldSpace || puzzleCanvas.renderMode == RenderMode.ScreenSpaceCamera) &&
                !puzzleCanvas.worldCamera && puzzleCamera)
            {
                puzzleCanvas.worldCamera = puzzleCamera;
            }

            if (!pin)
            {
                Completed?.Invoke(PopupMini.PuzzleResult.Error("setup:no_pin_component"));
                return;
            }

            // 이벤트 바인딩
            pin.ResetState();
            pin.OnGrab += HandleGrab;
            pin.OnRelease += HandleRelease;
            pin.OnSuccess += HandleSuccess;

            StartCoroutine(CoWatchCancel());
        }

        IEnumerator CoWatchCancel()
        {
            while (!_ct.IsCancellationRequested && !_finished) yield return null;
            if (!_finished && _ct.IsCancellationRequested)
                SafeComplete(PopupMini.PuzzleResult.Cancel("abort:external"));
        }

        void Update()
        {
            if (_finished || _ct.IsCancellationRequested) return;
            if (Input.GetKeyDown(KeyCode.Escape))
                SafeComplete(PopupMini.PuzzleResult.Cancel("abort:user"));
        }

        void HandleGrab() { if (sfx && sfxGrab) sfx.PlayOneShot(sfxGrab); }
        void HandleRelease() { if (!_finished && sfx && sfxRelease) sfx.PlayOneShot(sfxRelease); }
        void HandleSuccess()
        {
            if (_finished) return; if (sfx && sfxSuccess) sfx.PlayOneShot(sfxSuccess);
            SafeComplete(PopupMini.PuzzleResult.Ok("{\"rewards\":[{\"id\":\"extinguisher\",\"count\":1}]}"));
        }

        void SafeComplete(PopupMini.PuzzleResult r)
        {
            if (_finished) return;
            _finished = true;
            if (pin)
            {
                pin.OnGrab -= HandleGrab;
                pin.OnRelease -= HandleRelease;
                pin.OnSuccess -= HandleSuccess;
            }
            Completed?.Invoke(r);
        }
    }
}
