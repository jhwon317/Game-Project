using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class JigsawPuzzleController : MonoBehaviour
{
    public GameObject piecePrefab;
    public Sprite[] slicedSprites;
    public Transform puzzleFrame;
    public Transform piecesContainer;

    private int piecesCorrect = 0;

    void Start()
    {
        // 9개의 정답 위치(슬롯)를 먼저 만듦
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(puzzleFrame);
            slot.AddComponent<RectTransform>().localScale = Vector3.one;
        }

        // [핵심 수정!] 9개의 조각을 '순서대로' 먼저 생성
        List<GameObject> pieces = new List<GameObject>();
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            GameObject newPiece = Instantiate(piecePrefab, piecesContainer);
            newPiece.name = $"Piece_{i}";
            newPiece.GetComponent<Image>().sprite = slicedSprites[i];

            // 조각에게 "네 정답 위치는 너랑 번호가 똑같은 저기야!" 하고 짝을 맺어줌
            newPiece.GetComponent<DraggablePiece>().correctParent = puzzleFrame.GetChild(i);
            pieces.Add(newPiece);
        }

        // [핵심 수정!] 짝을 다 맺어준 다음에, 조각들의 '순서'만 섞음
        for (int i = 0; i < pieces.Count; i++)
        {
            int randomIndex = Random.Range(i, pieces.Count);
            pieces[i].transform.SetSiblingIndex(randomIndex);
        }

        // PiecesContainer 안에 자동으로 3x3 정렬되도록 Grid Layout Group 추가
        GridLayoutGroup containerLayout = piecesContainer.gameObject.AddComponent<GridLayoutGroup>();
        containerLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        containerLayout.constraintCount = 3;
        containerLayout.cellSize = new Vector2(200, 200);
        containerLayout.spacing = new Vector2(10, 10);
    }

    public void PiecePlaced()
    {
        piecesCorrect++;

        if (piecesCorrect >= slicedSprites.Length)
        {
            Debug.Log("퍼즐 클리어!");
        }
    }
}