using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PuzzleSequenceManager : MonoBehaviour
{
    [Header("���� ��� ����")]
    public Image originalImageHolder; // ���� �׸��� ������ ����
    public GameObject puzzleCore;       // ���� ���� ��ǰ���� ����ִ� ������Ʈ

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
        puzzleCore.SetActive(false);

        // 2. ���� �̹����� ������
        originalImageHolder.sprite = fullImageSprite;
        originalImageHolder.gameObject.SetActive(true);

        // 3. 3�� ��ٸ�
        yield return new WaitForSeconds(3f);

        // 4. ���� �̹����� �����, ���� ������ ���۽�Ŵ
        originalImageHolder.gameObject.SetActive(false);
        puzzleCore.SetActive(true);
    }
}
