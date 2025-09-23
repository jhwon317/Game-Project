using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class JigsawPuzzleController : MonoBehaviour
{
    public GameObject piecePrefab;
    public Sprite[] slicedSprites;
    public Transform puzzleFrame;
    public Transform piecesContainer;

    // [새로 추가!] 격자 선으로 쓸 이미지
    public Sprite gridSprite;

    private int piecesCorrect = 0;

    void Start()
    {
        // [핵심 수정!] 9개의 정답 위치(슬롯)에 정답 그림 대신 격자 선을 깔아둠
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(puzzleFrame);
            slot.AddComponent<RectTransform>().localScale = Vector3.one;

            // 슬롯에 격자 선 이미지를 추가
            Image slotImage = slot.AddComponent<Image>();
            slotImage.sprite = gridSprite; // 옅은 정답 그림 대신 격자 선 스프라이트 사용
            slotImage.color = new Color(1f, 1f, 1f, 0.2f); // 아주 희미하게 (투명도 20%)
            slotImage.raycastTarget = false; // 클릭을 방해하지 않도록 설정!
        }

        // --- 이 아래는 이전과 동일 ---

        // 9개의 조각을 '순서대로' 먼저 생성해서 짝을 맺어줌
        List<GameObject> pieces = new List<GameObject>();
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            GameObject newPiece = Instantiate(piecePrefab, piecesContainer);
            newPiece.name = $"Piece_{i}";
            newPiece.GetComponent<Image>().sprite = slicedSprites[i];
            newPiece.GetComponent<DraggablePiece>().correctParent = puzzleFrame.GetChild(i);
            pieces.Add(newPiece);
        }

        // 짝을 다 맺어준 다음에, 조각들의 '순서'만 섞음
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

        // [핵심 수정!] 모든 조각이 맞춰졌는지 확인
        if (piecesCorrect >= slicedSprites.Length)
        {
            Debug.Log("퍼즐 클리어! 다음 스테이지로 이동합니다.");

            // 현재 씬의 번호표를 찾아서 +1 한 다음, 그 번호의 씬으로 이동!
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            // 만약 다음 씬이 '목차'에 있다면 이동
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                // 목차에 다음 씬이 없으면 에러 메시지를 띄워줌
                Debug.LogError("다음 스테이지가 없습니다! Build Profiles에 Stage 2-4를 추가했는지 확인해주세요!");
            }
        }


    }
}