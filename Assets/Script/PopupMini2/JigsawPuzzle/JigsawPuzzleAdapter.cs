// JigsawPuzzleAdapter.cs (patched)
// - ���ӽ����̽� ����: PopupMini.IPuzzleController ������ ���
// - ���� ����: ���� ���� �� ���� Ȱ��ȭ (�ʱ� ��ȣ ���� ����)
// - ��� ����: CancellationToken ���
// - Ŭ����: OnDestroy���� ���� ����

using System;
using System.Threading;
using UnityEngine;

namespace PopupMini
{
    public class JigsawPuzzleAdapter : MonoBehaviour, IPuzzleController
    {
        [Header("Wire the actual puzzle logic here")]
        public JigsawPuzzleController puzzleController;

        public event Action<PuzzleResult> Completed;

        bool _done;
        CancellationTokenRegistration _ctr;

        public void Begin(object args, CancellationToken ct)
        {
            if (_done) return;

            if (!puzzleController)
            {
                Debug.LogError("[JigsawPuzzleAdapter] puzzleController not assigned");
                SafeComplete(PuzzleResult.Cancel("missing_controller"));
                return;
            }

            // 1) ����/���� ��ȣ ���� ����
            puzzleController.OnPuzzleSolved -= HandleSolved; // �ߺ� ����
            puzzleController.OnPuzzleSolved += HandleSolved;

            // �ʿ��ϴٸ� ����/���� �̺�Ʈ�� ���⼭ ���� (��: puzzleController.OnAbort += HandleAbort;)

            // 2) ��� ��ū ���
            if (ct.CanBeCanceled)
            {
                _ctr = ct.Register(() =>
                {
                    if (_done) return;
                    SafeComplete(PuzzleResult.Cancel("canceled"));
                });
            }

            // 3) ���� ���� (Ȱ��ȭ/���� ��)
            if (!puzzleController.gameObject.activeSelf)
                puzzleController.gameObject.SetActive(true);

            // ������ SequenceManager�� ���� ���߿� enable�Ǵ� ������,
            // �� ������ ������ �ʱ� �̺�Ʈ�� ��ġ�� ����.
        }

        void HandleSolved()
        {
            if (_done) return;
            SafeComplete(PuzzleResult.Ok()); // ����
        }

        // ����/���� �ñ׳��� ���� ��� ����
        // void HandleAbort(string reason) { if (_done) return; SafeComplete(PuzzleResult.Cancel(reason)); }

        void SafeComplete(PuzzleResult result)
        {
            if (_done) return;
            _done = true;

            try { _ctr.Dispose(); } catch { /* ignore */ }

            // �ʿ� �� ���� ��Ȱ��ȭ/����
            if (puzzleController)
                puzzleController.enabled = false;

            try { Completed?.Invoke(result); }
            catch (Exception e) { Debug.LogException(e); }
        }

        void OnDestroy()
        {
            try
            {
                if (puzzleController) puzzleController.OnPuzzleSolved -= HandleSolved;
                _ctr.Dispose();
            }
            catch { }
        }
    }
}
