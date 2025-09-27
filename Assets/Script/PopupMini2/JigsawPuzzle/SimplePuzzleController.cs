using System;
using System.Threading;

// 팝업 퍼즐의 결과물을 담는 구조체
public struct PuzzleResult
{
    public bool Success;
    public string Message;

    public static PuzzleResult Ok() => new PuzzleResult { Success = true };
    public static PuzzleResult Cancel(string message = "Canceled") => new PuzzleResult { Success = false, Message = message };
}

// 모든 팝업 퍼즐이 따라야 할 규칙(인터페이스)
public interface IPuzzleController
{
    // 퍼즐이 끝났을 때 PD에게 보고할 이벤트
    event Action<PuzzleResult> Completed;
    // PD가 "촬영 시작!" 하고 호출할 함수
    void Begin(object args, CancellationToken ct);
}