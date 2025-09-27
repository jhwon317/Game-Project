using UnityEngine;
using System; // Action�� ���� �ʿ�
using System.Threading; // CancellationToken�� ���� �ʿ�

// �� ���ᰡ ���� IPuzzleController ��Ģ�� �����ٰ� ����
public class JigsawPuzzleAdapter : MonoBehaviour, IPuzzleController
{
    // ���� ���� ������ ��� JigsawPuzzleController�� ����
    public JigsawPuzzleController puzzleController;

    // ������ ������ �� PD���� ������ �̺�Ʈ
    public event Action<PuzzleResult> Completed;

    public void Begin(object args, CancellationToken ct)
    {
        // PD�� "�Կ� ����!" �ϸ�, ���� ������ �Ҵ�.
        puzzleController.gameObject.SetActive(true);

        // JigsawPuzzleController���� "���� ������ ������ �˷���!" ��� ��Ź
        puzzleController.OnPuzzleSolved += HandlePuzzleSolved;
    }

    // JigsawPuzzleController�� "�� �� Ǯ�����!" �ϰ� �˷��ָ� ����� �Լ�
    private void HandlePuzzleSolved()
    {
        // PD���� "�Կ� ��! ���������� ���ƽ��ϴ�!" ��� ����
        Completed?.Invoke(PuzzleResult.Ok());
    }
}