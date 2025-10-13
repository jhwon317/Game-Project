using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PuzzleSequenceManager : MonoBehaviour
{
    [Header("���� ��� ����")]
    public Image originalImageHolder; // ���� �׸��� ������ ����

    // [����] PuzzleCore ���, ���� ��ǰ���� ���� ����
    public GameObject puzzleFrame;      // ������
    public GameObject piecesContainer;  // �������� ��ѷ��� ��

    [Header("���� ��� ����")]
    public Sprite fullImageSprite; // ��¥ ���� �̹���

    void OnEnable()
    {
        // �� ������ ���� ������, ���� ����!
        StartCoroutine(StartPuzzleSequence());
    }

    IEnumerator StartPuzzleSequence()
    {
        // 1. ���� ���� ��, ��� �� ����
        originalImageHolder.gameObject.SetActive(false);
        puzzleFrame.SetActive(false);
        piecesContainer.SetActive(false);

        // 2. ���� �̹����� ������
        originalImageHolder.sprite = fullImageSprite;
        originalImageHolder.gameObject.SetActive(true);

        // 3. 2�� ��ٸ� (��¥ �ð� ����)
        yield return new WaitForSecondsRealtime(2f);

        // 4. ���� �̹����� �����, ���� ������ ���۽�Ŵ
        originalImageHolder.gameObject.SetActive(false);
        puzzleFrame.SetActive(true);
        piecesContainer.SetActive(true);

        // [����] PuzzleCore�� �� �ʿ� ����, ���� �����ڰ� �ٷ� ���� ������
        GetComponentInChildren<JigsawPuzzleController>().enabled = true;
    }


}