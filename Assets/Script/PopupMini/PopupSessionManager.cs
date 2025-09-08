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
        public PopupHost Host;               // Panel/Content/Viewport ����
        public PopupResizer Resizer;         // (����) ũ�� ����

        [Header("Options")]
        public string MiniWorldLayerName = "MiniWorld";
        public bool preventReentry = true;

        // internal
        PuzzleFactory _factory;
        ViewportService _viewport;
        IDisposable _gate;                   // ��� �Է� ����Ʈ
        CancellationTokenSource _cts;        // ���ǿ� CTS
        bool _busy;
        PuzzleInstance _inst;

        void Awake()
        {
            if (!Host) Host = GetComponent<PopupHost>();
            if (!Resizer) Resizer = GetComponent<PopupResizer>();

            _factory = new PuzzleFactory(MiniWorldLayerName);     // NameToLayer�� ���丮 ���ο��� ������
            _viewport = new ViewportService(Host ? Host.Viewport : null);
        }

        public bool IsBusy => _busy;

        /// <summary>
        /// ���� ���� ����� ��ٸ��ϴ�. �ܺ� ��� ��ū�� �ѱ�� abort:external�� ����˴ϴ�.
        /// </summary>
        public async Task<PuzzleResult> OpenAsync(PuzzleRequest req, CancellationToken externalCt = default)
        {
            if (req == null || req.Definition == null) return PuzzleResult.Error("bad_request");
            if (preventReentry && _busy) return PuzzleResult.Error("busy");

            _busy = true;

            var def = req.Definition;

            // -------- 1) ������ ���� (�ۼ�Ʈ ��Ŀ �켱) --------
            // SizeOverride�� nullable�̸� ?? ��, ��-nullable�̸� ���޵� ���� �״�� ���
            PopupSizeOptions size = req.SizeOverride is PopupSizeOptions so ? so : PopupSizeOptions.DefaultSquare;

            // ���� ������ �⺻ Aspect/AspectMode�� ä���ֱ� (0 �Ǵ� �̼����̸� ����)
            if (size.AspectMode == 0) size.AspectMode = def.AspectMode;
            if (size.Aspect <= 0) size.Aspect = def.Aspect;

            if (Resizer) Resizer.Apply(size);

            // -------- 2) ���� �ɼ� ���� (Modal / Timeout ��) --------
            // SessionOverride�� struct�� class�� ���� ������ �۾�
            var sess = req.SessionOverride;
            // Modal/BackdropClosable: ���ǰ����� �⺻ ����
            if (!sess.Modal)
            {
                sess.Modal = def.Modal;
                sess.BackdropClosable = def.BackdropClosable;
            }
            // Timeout: ��û�� 0/�̼����̸� ���ǰ�����
            if (!(sess.TimeoutSec > 0f) && def.TimeoutSec > 0f)
                sess.TimeoutSec = def.TimeoutSec;

            // -------- 3) ��� �Է� ����Ʈ --------
            if (sess.Modal)
            {
                // Input System ������ ������ UI/Gameplay �� ��ȯ + Ŀ�� ǥ��
                _gate = InputModalGate.Acquire(new InputModalGate.Options
                {
                    GameplayMap = "Gameplay",
                    UIMap = "UI",
                    ShowCursor = true
                });
            }

            // -------- 4) �г� ǥ�� --------
            Host?.Show();

            // -------- 5) ���� �ν��Ͻ� ���� �� ����Ʈ ���ε� --------
            _inst = _factory.Create(def);
            if (_inst.Root == null)
            {
                SafeTearDown();
                return PuzzleResult.Error("create_failed");
            }

            // ī�޶� �� RawImage ���ε�
            _viewport.Bind(_inst.Cam, def);

            // (����) RT UI Ŭ�� ���Ͻ� �ڵ� ���ε�
            var proxy = Host?.Viewport ? Host.Viewport.GetComponent<RTUIClickProxyPro>() : null;
            if (proxy) proxy.BindAuto(_inst.Cam, _inst.Root.transform);

            // -------- 6) �Ϸ� ��� (Completed / ��� / Ÿ�Ӿƿ�) --------
            var tcs = new TaskCompletionSource<PuzzleResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            // ���ǿ� CTS (�ܺ� ��ū�� ��ũ)
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            _cts = linked; // Cancel() ȣ�� ���� ���� ����

            if (sess.TimeoutSec > 0f)
                _cts.CancelAfter(TimeSpan.FromSeconds(sess.TimeoutSec.Value));

            // �ܺ�/Ÿ�Ӿƿ� ��� �� ����� ��ȯ
            using var _ = _cts.Token.Register(() =>
            {
                var reason = (sess.TimeoutSec > 0f) ? "timeout" : "abort:external";
                tcs.TrySetResult(PuzzleResult.Cancel(reason));
            });

            // Completed �ڵ鷯 (�� ����)
            void OnCompleted(PuzzleResult r)
            {
                tcs.TrySetResult(r);
            }

            _inst.Controller.Completed += OnCompleted;

            try
            {
                // ���� ���� (args/null ���), ���� ��ū ����
                _inst.Controller.Begin(req.Args, _cts.Token);

                // �Ϸ���� ���
                var result = await tcs.Task;
                return result;
            }
            finally
            {
                // ���� ����(�߿�)
                if (_inst.Controller != null)
                    _inst.Controller.Completed -= OnCompleted;

                // -------- 7) ���� --------
                if (proxy) proxy.Unbind();
                SafeTearDown();   // viewport.Unbind �� factory.Destroy �� gate.Dispose �� Host.Hide ��
            }
        }

        /// <summary>����� ��� ��ư ��� ȣ��</summary>
        public void Cancel(string reason = "abort:user")
        {
            try { _cts?.Cancel(); } catch { /* ignore */ }
        }

        // ���ҽ�/���� ����
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
