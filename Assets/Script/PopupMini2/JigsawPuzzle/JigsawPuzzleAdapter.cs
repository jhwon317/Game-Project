// JigsawPuzzleAdapter.cs (patched)
// - 네임스페이스 정합: PopupMini.IPuzzleController 구현을 명시
// - 구독 순서: 구독 먼저 → 퍼즐 활성화 (초기 신호 누락 방지)
// - 취소 대응: CancellationToken 등록
// - 클린업: OnDestroy에서 안전 해제

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

            // 1) 성공/실패 신호 먼저 구독
            puzzleController.OnPuzzleSolved -= HandleSolved; // 중복 방지
            puzzleController.OnPuzzleSolved += HandleSolved;

            // 필요하다면 실패/포기 이벤트도 여기서 구독 (예: puzzleController.OnAbort += HandleAbort;)

            // 2) 취소 토큰 등록
            if (ct.CanBeCanceled)
            {
                _ctr = ct.Register(() =>
                {
                    if (_done) return;
                    SafeComplete(PuzzleResult.Cancel("canceled"));
                });
            }

            // 3) 퍼즐 가동 (활성화/리셋 등)
            if (!puzzleController.gameObject.activeSelf)
                puzzleController.gameObject.SetActive(true);

            // 퍼즐이 SequenceManager에 의해 나중에 enable되는 구조라도,
            // 위 구독이 먼저라 초기 이벤트는 놓치지 않음.
        }

        void HandleSolved()
        {
            if (_done) return;
            SafeComplete(PuzzleResult.Ok()); // 성공
        }

        // 실패/포기 시그널을 쓰는 경우 예시
        // void HandleAbort(string reason) { if (_done) return; SafeComplete(PuzzleResult.Cancel(reason)); }

        void SafeComplete(PuzzleResult result)
        {
            if (_done) return;
            _done = true;

            try { _ctr.Dispose(); } catch { /* ignore */ }

            // 필요 시 퍼즐 비활성화/정리
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
