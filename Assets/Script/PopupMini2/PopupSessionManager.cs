using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PopupMini
{
    [DisallowMultipleComponent]
    public class PopupSessionManager : MonoBehaviour
    {
        [Header("Scene wiring")]
        public PopupHost Host;

        [Header("Options")]
        public string MiniWorldLayerName = "MiniWorld";
        public bool preventReentry = true;

        PuzzleFactory _factory;
        ViewportService _viewport;
        PopupAutoLayout _auto;

        IDisposable _gate;
        CancellationTokenSource _cts;
        bool _busy;
        PuzzleInstance _inst;

        void Awake()
        {
            if (!Host) Host = GetComponent<PopupHost>();
            _factory = new PuzzleFactory(MiniWorldLayerName);
            _viewport = new ViewportService(Host ? Host.Viewport : null);
            _auto = Host ? Host.GetComponent<PopupAutoLayout>() : null;
        }

        public bool IsBusy => _busy;

        public async Task<PuzzleResult> OpenAsync(PuzzleRequest req, CancellationToken externalCt = default)
        {
            if (req == null || req.Definition == null) return PuzzleResult.Error("bad_request");
            if (preventReentry && _busy) return PuzzleResult.Cancel("busy");
            if (!Host) return PuzzleResult.Error("host:null");
            _busy = true;

            var def = req.Definition;

            // ---- 세션 옵션 병합 ----
            var sess = req.SessionOverride;
            if (!sess.Modal) { sess.Modal = def.Modal; sess.BackdropClosable = def.BackdropClosable; }
            if (!(sess.TimeoutSec > 0f) && def.TimeoutSec > 0f) sess.TimeoutSec = def.TimeoutSec;

            // ---- 입력 게이트(모달) ----
            if (sess.Modal)
            {
                _gate = InputModalGate.Acquire(new InputModalGate.Options
                {
                    GameplayMap = "Gameplay",
                    UIMap = "UI",
                    ShowCursor = true
                });
            }

            // ---- (선택) UI 오버라이드 적용 ----
            var bakHost = SaveHost(Host);
            ApplyUIOverride(Host, req.UIOverride);

            try
            {
                // ---- 레이아웃(패딩/스케일) 선적용 ----
                if (!Host.ContentRoot) return PuzzleResult.Error("host.content:null");
                if (!Host.Viewport) return PuzzleResult.Error("host.viewport:null");

                _auto?.ApplyBeforeOpen();

                // ---- 패널 즉시 ON(코루틴 회피) ----
                Host.Show(instant: true);

                // ---- 퍼즐 인스턴스 ----
                _inst = _factory.Create(def);
                if (_inst.Root == null) return PuzzleResult.Error("create_failed");
                if (!_inst.Cam) return PuzzleResult.Error("prefab.camera:null");
                if (_inst.Controller == null) return PuzzleResult.Error("prefab.controller:null");

                // ---- 프리팹 기본 Args ----
                var cfg = _inst.Root.GetComponentInChildren<PuzzlePrefabConfig>(true);
                if (string.IsNullOrEmpty(req.Args) && cfg && cfg.defaultArgsJson)
                    req.Args = cfg.defaultArgsJson.text;

                // ---- 바인딩(여기서만 퍼즐캠 활성) ----
                _viewport.Bind(_inst.Cam, def);

                // ---- 카메라 자동 프레이밍(옵션) ----
                _auto?.ApplyAfterBind(_inst);

                // ---- 완료 대기 ----
                var tcs = new TaskCompletionSource<PuzzleResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
                _cts = linked;
                if (sess.TimeoutSec > 0f) _cts.CancelAfter(TimeSpan.FromSeconds(sess.TimeoutSec));

                var timer = Host.PanelRoot ? Host.PanelRoot.GetComponentInChildren<PopupCountdown>(true) : null;
                if (timer && sess.TimeoutSec > 0f)
                {
                    timer.gameObject.SetActive(true);
                    timer.StartCountdown(sess.TimeoutSec, _cts.Token); // unscaledDeltaTime 사용 권장
                }

                using var _reg = _cts.Token.Register(() =>
                    tcs.TrySetResult(PuzzleResult.Cancel(sess.TimeoutSec > 0f ? "timeout" : "abort:external")));

                void OnCompleted(PuzzleResult r) => tcs.TrySetResult(r);
                _inst.Controller.Completed += OnCompleted;

                try
                {
                    _inst.Controller.Begin(req.Args, _cts.Token);
                    return await tcs.Task;
                }
                finally
                {
                    try { if (timer) { timer.Stop(); timer.gameObject.SetActive(false); } } catch { }
                    _inst.Controller.Completed -= OnCompleted;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[PopupSession][EX] {e}");
                return PuzzleResult.Error(e.Message);
            }
            finally
            {
                try { _viewport.Unbind(); } catch { }
                try { _factory.Destroy(_inst); } catch { }
                try { _gate?.Dispose(); } catch { }
                try { Host.Hide(instant: true); } catch { }

                RestoreHost(Host, bakHost);

                _inst = default;
                _cts?.Dispose(); _cts = null;
                _busy = false;
            }
        }

        public void Cancel() { try { _cts?.Cancel(); } catch { } }

        // ----- Host override helpers -----
        struct HostBak
        {
            public GameObject Panel;
            public RectTransform Content;
            public CamToRawImage View;
            public CanvasGroup Group;
        }
        HostBak SaveHost(PopupHost h) => new HostBak
        {
            Panel = h ? h.PanelRoot : null,
            Content = h ? h.ContentRoot : null,
            View = h ? h.Viewport : null,
            Group = h ? h.CanvasGroup : null
        };
        void RestoreHost(PopupHost h, HostBak b)
        {
            if (!h) return;
            h.PanelRoot = b.Panel;
            h.ContentRoot = b.Content;
            h.Viewport = b.View;
            h.CanvasGroup = b.Group;
        }
        void ApplyUIOverride(PopupHost host, PopupUIOverride ov)
        {
            if (!host) return;
            if (ov.PanelRoot) host.PanelRoot = ov.PanelRoot;
            if (ov.ContentRoot) host.ContentRoot = ov.ContentRoot;
            if (ov.Viewport) host.Viewport = ov.Viewport;
            if (ov.CanvasGroup) host.CanvasGroup = ov.CanvasGroup;

            // 자동 보정
            if (host.PanelRoot && !host.CanvasGroup)
                host.CanvasGroup = host.PanelRoot.GetComponent<CanvasGroup>() ?? host.PanelRoot.AddComponent<CanvasGroup>();
            if (!host.ContentRoot && host.PanelRoot)
                host.ContentRoot = host.PanelRoot.GetComponentInChildren<RectTransform>(true);
            if (!host.Viewport && host.PanelRoot)
                host.Viewport = host.PanelRoot.GetComponentInChildren<CamToRawImage>(true);
        }
    }
}