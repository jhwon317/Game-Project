using System;
using System.Threading;

// �˾� ������ ������� ��� ����ü
public struct PuzzleResult
{
    public bool Success;
    public string Message;

    public static PuzzleResult Ok() => new PuzzleResult { Success = true };
    public static PuzzleResult Cancel(string message = "Canceled") => new PuzzleResult { Success = false, Message = message };
}

// ��� �˾� ������ ����� �� ��Ģ(�������̽�)
public interface IPuzzleController
{
    // ������ ������ �� PD���� ������ �̺�Ʈ
    event Action<PuzzleResult> Completed;
    // PD�� "�Կ� ����!" �ϰ� ȣ���� �Լ�
    void Begin(object args, CancellationToken ct);
}