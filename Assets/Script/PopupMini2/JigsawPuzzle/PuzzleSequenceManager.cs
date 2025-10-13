using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PuzzleSequenceManager : MonoBehaviour
{
    [Header("연출 대상 연결")]
    public Image originalImageHolder; // 원본 그림을 보여줄 액자

    // [수정] PuzzleCore 대신, 실제 부품들을 직접 연결
    public GameObject puzzleFrame;      // 정답판
    public GameObject piecesContainer;  // 조각들이 흩뿌려질 곳

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
        puzzleFrame.SetActive(false);
        piecesContainer.SetActive(false);

        // 2. 원본 이미지를 보여줌
        originalImageHolder.sprite = fullImageSprite;
        originalImageHolder.gameObject.SetActive(true);

        // 3. 2초 기다림 (진짜 시계 기준)
        yield return new WaitForSecondsRealtime(2f);

        // 4. 원본 이미지를 숨기고, 실제 퍼즐을 시작시킴
        originalImageHolder.gameObject.SetActive(false);
        puzzleFrame.SetActive(true);
        piecesContainer.SetActive(true);

        // [수정] PuzzleCore를 켤 필요 없이, 퍼즐 제작자가 바로 일을 시작함
        GetComponentInChildren<JigsawPuzzleController>().enabled = true;
    }


}