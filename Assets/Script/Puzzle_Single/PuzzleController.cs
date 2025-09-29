using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class PuzzleController : MonoBehaviour
{
    public GameObject piecePrefab;
    public Sprite[] slicedSprites;
    public Transform puzzleFrame;

    // [새로 추가!] 격자 선으로 쓸 이미지
    public Sprite gridSprite;

    // [새로 추가!] 퍼즐이 끝났다고 알려줄 이벤트
    public event Action OnPuzzleSolved;

    private int piecesCorrect = 0;

    void Start()
    {
        GenerateAndShufflePieces();
    }

    void GenerateAndShufflePieces()
    {
        // 9개의 정답 위치(슬롯)를 'UI용 오브젝트'로 제대로 만듦
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(puzzleFrame, false);
            slot.AddComponent<RectTransform>();

            // [핵심!] 바로 이 부분이 네가 추가하고 싶었던 코드야!
            // 슬롯에 옅은 격자 선 이미지를 추가
            Image slotImage = slot.AddComponent<Image>();
            slotImage.sprite = gridSprite;
            slotImage.color = new Color(1f, 1f, 1f, 0.2f); // 아주 희미하게 (투명도 20%)
            slotImage.raycastTarget = false; // 클릭을 방해하지 않도록 설정!
        }

        // 9개의 조각을 '순서대로' 먼저 생성해서 짝을 맺어줌
        List<GameObject> pieces = new List<GameObject>();
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            // piecesContainer 대신, 이 스크립트가 붙어있는 Panel 위에 바로 생성
            GameObject newPiece = Instantiate(piecePrefab, transform);
            newPiece.name = $"Piece_{i}";
            newPiece.GetComponent<Image>().sprite = slicedSprites[i];

            // 네가 이름을 바꾼 Drag.cs를 찾도록 수정
            newPiece.GetComponent<Drag>().correctParent = puzzleFrame.GetChild(i);
            pieces.Add(newPiece);

            // PuzzlePanel 영역 안에서 무작위 위치에 흩뿌리기
            RectTransform panelRect = GetComponent<RectTransform>();
            float randomX = UnityEngine.Random.Range(panelRect.rect.xMin / 2, panelRect.rect.xMax / 2);
            float randomY = UnityEngine.Random.Range(panelRect.rect.yMin / 2, panelRect.rect.yMax / 2);
            newPiece.GetComponent<RectTransform>().anchoredPosition = new Vector2(randomX, randomY);
        }

    }

    public void PiecePlaced()
    {
        piecesCorrect++;
        if (piecesCorrect >= slicedSprites.Length)
        {
            Debug.Log("퍼즐 클리어!");

            // "저 다 풀었어요!" 하고 방송함
            OnPuzzleSolved?.Invoke();
        }
    }


}
