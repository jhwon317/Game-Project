using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PuzzleSequenceManager : MonoBehaviour
{
    [Header("연출 대상 연결")]
    public Image originalImageHolder; // 원본 그림을 보여줄 액자
    public GameObject puzzleCore;       // 실제 퍼즐 부품들이 들어있는 오브젝트

    [Header("연출 재료 연결")]
    public Sprite fullImageSprite; // 통짜 원본 이미지

    void OnEnable()
    {
        // 이 퍼즐이 켜질 때마다, 연출 시작!
        StartCoroutine(StartPuzzleSequence());
    }

    IEnumerator StartPuzzleSequence()
    {
        // 1. 연출 시작 전, 모든 걸 숨김
        originalImageHolder.gameObject.SetActive(false);
        puzzleCore.SetActive(false);

        // 2. 원본 이미지를 보여줌
        originalImageHolder.sprite = fullImageSprite;
        originalImageHolder.gameObject.SetActive(true);

        // 3. 3초 기다림
        yield return new WaitForSeconds(3f);

        // 4. 원본 이미지를 숨기고, 실제 퍼즐을 시작시킴
        originalImageHolder.gameObject.SetActive(false);
        puzzleCore.SetActive(true);
    }
}
