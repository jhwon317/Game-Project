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
        public PopupHost Host;               // Panel/Content/Viewport 묶음
        public PopupResizer Resizer;         // (선택) 크기 조절

        [Header("Options")]
        public string MiniWorldLayerName = "MiniWorld";
        public bool preventReentry = true;

        // internal
        PuzzleFactory _factory;
        ViewportService _viewport;
        IDisposable _gate;                   // 모달 입력 게이트
        CancellationTokenSource _cts;        // 세션용 CTS
        bool _busy;
        PuzzleInstance _inst;

        void Awake()
        {
            if (!Host) Host = GetComponent<PopupHost>();
            if (!Resizer) Resizer = GetComponent<PopupResizer>();

            _factory = new PuzzleFactory(MiniWorldLayerName);     // NameToLayer는 팩토리 내부에서 지연평가
            _viewport = new ViewportService(Host ? Host.Viewport : null);
        }

        public bool IsBusy => _busy;

        /// <summary>
        /// 퍼즐 열고 결과를 기다립니다. 외부 취소 토큰을 넘기면 abort:external로 종료됩니다.
        /// </summary>
        public async Task<PuzzleResult> OpenAsync(PuzzleRequest req, CancellationToken externalCt = default)
        {
            if (req == null || req.Definition == null) return PuzzleResult.Error("bad_request");
            if (preventReentry && _busy) return PuzzleResult.Error("busy");

            _busy = true;

            var def = req.Definition;

            // -------- 1) 사이즈 적용 (퍼센트 앵커 우선) --------
            // SizeOverride가 nullable이면 ?? 로, 비-nullable이면 전달된 값을 그대로 사용
            PopupSizeOptions size = req.SizeOverride is PopupSizeOptions so ? so : PopupSizeOptions.DefaultSquare;

            // 퍼즐 정의의 기본 Aspect/AspectMode로 채워주기 (0 또는 미설정이면 보강)
            if (size.AspectMode == 0) size.AspectMode = def.AspectMode;
            if (size.Aspect <= 0) size.Aspect = def.Aspect;

            if (Resizer) Resizer.Apply(size);

            // -------- 2) 세션 옵션 머지 (Modal / Timeout 등) --------
            // SessionOverride가 struct든 class든 로컬 변수로 작업
            var sess = req.SessionOverride;
            // Modal/BackdropClosable: 정의값으로 기본 세팅
            if (!sess.Modal)
            {
                sess.Modal = def.Modal;
                sess.BackdropClosable = def.BackdropClosable;
            }
            // Timeout: 요청이 0/미설정이면 정의값으로
            if (!(sess.TimeoutSec > 0f) && def.TimeoutSec > 0f)
                sess.TimeoutSec = def.TimeoutSec;

            // -------- 3) 모달 입력 게이트 --------
            if (sess.Modal)
            {
                // Input System 세팅이 있으면 UI/Gameplay 맵 전환 + 커서 표시
                _gate = InputModalGate.Acquire(new InputModalGate.Options
                {
                    GameplayMap = "Gameplay",
                    UIMap = "UI",
                    ShowCursor = true
                });
            }

            // -------- 4) 패널 표시 --------
            Host?.Show();

            // -------- 5) 퍼즐 인스턴스 생성 및 뷰포트 바인딩 --------
            _inst = _factory.Create(def);
            if (_inst.Root == null)
            {
                SafeTearDown();
                return PuzzleResult.Error("create_failed");
            }

            // 카메라 → RawImage 바인딩
            _viewport.Bind(_inst.Cam, def);

            // (선택) RT UI 클릭 프록시 자동 바인딩
            var proxy = Host?.Viewport ? Host.Viewport.GetComponent<RTUIClickProxyPro>() : null;
            if (proxy) proxy.BindAuto(_inst.Cam, _inst.Root.transform);

            // -------- 6) 완료 대기 (Completed / 취소 / 타임아웃) --------
            var tcs = new TaskCompletionSource<PuzzleResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            // 세션용 CTS (외부 토큰과 링크)
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            _cts = linked; // Cancel() 호출 위해 참조 보관

            if (sess.TimeoutSec > 0f)
                _cts.CancelAfter(TimeSpan.FromSeconds(sess.TimeoutSec.Value));

            // 외부/타임아웃 취소 → 결과로 전환
            using var _ = _cts.Token.Register(() =>
            {
                var reason = (sess.TimeoutSec > 0f) ? "timeout" : "abort:external";
                tcs.TrySetResult(PuzzleResult.Cancel(reason));
            });

            // Completed 핸들러 (한 번만)
            void OnCompleted(PuzzleResult r)
            {
                tcs.TrySetResult(r);
            }

            _inst.Controller.Completed += OnCompleted;

            try
            {
                // 퍼즐 시작 (args/null 허용), 세션 토큰 전달
                _inst.Controller.Begin(req.Args, _cts.Token);

                // 완료까지 대기
                var result = await tcs.Task;
                return result;
            }
            finally
            {
                // 구독 해제(중요)
                if (_inst.Controller != null)
                    _inst.Controller.Completed -= OnCompleted;

                // -------- 7) 정리 --------
                if (proxy) proxy.Unbind();
                SafeTearDown();   // viewport.Unbind → factory.Destroy → gate.Dispose → Host.Hide 등
            }
        }

        /// <summary>사용자 취소 버튼 등에서 호출</summary>
        public void Cancel(string reason = "abort:user")
        {
            try { _cts?.Cancel(); } catch { /* ignore */ }
        }

        // 리소스/상태 정리
        void SafeTearDown()
        {
            try { _viewport?.Unbind(); } catch { }
            try { _factory?.Destroy(_inst); } catch { }
            try { _gate?.Dispose(); } catch { }
            try { Host?.Hide(); } catch { }

            _inst = default;
            _cts?.Dispose(); _cts = null;
            _busy = false;
        }
    }
}
