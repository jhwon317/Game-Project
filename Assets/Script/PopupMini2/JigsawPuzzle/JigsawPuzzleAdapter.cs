using UnityEngine;
using System; // Action을 위해 필요
using System.Threading; // CancellationToken을 위해 필요

// 네 동료가 만든 IPuzzleController 규칙을 따른다고 선언
public class JigsawPuzzleAdapter : MonoBehaviour, IPuzzleController
{
    // 실제 퍼즐 로직이 담긴 JigsawPuzzleController를 연결
    public JigsawPuzzleController puzzleController;

    // 퍼즐이 끝났을 때 PD에게 보고할 이벤트
    public event Action<PuzzleResult> Completed;

    public void Begin(object args, CancellationToken ct)
    {
        // PD가 "촬영 시작!" 하면, 실제 퍼즐을 켠다.
        puzzleController.gameObject.SetActive(true);

        // JigsawPuzzleController에게 "퍼즐 끝나면 나한테 알려줘!" 라고 부탁
        puzzleController.OnPuzzleSolved += HandlePuzzleSolved;
    }

    // JigsawPuzzleController가 "저 다 풀었어요!" 하고 알려주면 실행될 함수
    private void HandlePuzzleSolved()
    {
        // PD에게 "촬영 끝! 성공적으로 마쳤습니다!" 라고 보고
        Completed?.Invoke(PuzzleResult.Ok());
    }
}