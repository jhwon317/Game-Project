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

    // [���� �߰�!] ���� ������ �� �̹���
    public Sprite gridSprite;

    private int piecesCorrect = 0;

    void Start()
    {
        // [�ٽ� ����!] 9���� ���� ��ġ(����)�� ���� �׸� ��� ���� ���� ��Ƶ�
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(puzzleFrame);
            slot.AddComponent<RectTransform>().localScale = Vector3.one;

            // ���Կ� ���� �� �̹����� �߰�
            Image slotImage = slot.AddComponent<Image>();
            slotImage.sprite = gridSprite; // ���� ���� �׸� ��� ���� �� ��������Ʈ ���
            slotImage.color = new Color(1f, 1f, 1f, 0.2f); // ���� ����ϰ� (���� 20%)
            slotImage.raycastTarget = false; // Ŭ���� �������� �ʵ��� ����!
        }

        // --- �� �Ʒ��� ������ ���� ---

        // 9���� ������ '�������' ���� �����ؼ� ¦�� �ξ���
        List<GameObject> pieces = new List<GameObject>();
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            GameObject newPiece = Instantiate(piecePrefab, piecesContainer);
            newPiece.name = $"Piece_{i}";
            newPiece.GetComponent<Image>().sprite = slicedSprites[i];
            newPiece.GetComponent<DraggablePiece>().correctParent = puzzleFrame.GetChild(i);
            pieces.Add(newPiece);
        }

        // ¦�� �� �ξ��� ������, �������� '����'�� ����
        for (int i = 0; i < pieces.Count; i++)
        {
            int randomIndex = Random.Range(i, pieces.Count);
            pieces[i].transform.SetSiblingIndex(randomIndex);
        }

        // PiecesContainer �ȿ� �ڵ����� 3x3 ���ĵǵ��� Grid Layout Group �߰�
        GridLayoutGroup containerLayout = piecesContainer.gameObject.AddComponent<GridLayoutGroup>();
        containerLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        containerLayout.constraintCount = 3;
        containerLayout.cellSize = new Vector2(200, 200);
        containerLayout.spacing = new Vector2(10, 10);
    }

    public void PiecePlaced()
    {
        piecesCorrect++;

        // [�ٽ� ����!] ��� ������ ���������� Ȯ��
        if (piecesCorrect >= slicedSprites.Length)
        {
            Debug.Log("���� Ŭ����! ���� ���������� �̵��մϴ�.");

            // ���� ���� ��ȣǥ�� ã�Ƽ� +1 �� ����, �� ��ȣ�� ������ �̵�!
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            // ���� ���� ���� '����'�� �ִٸ� �̵�
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                // ������ ���� ���� ������ ���� �޽����� �����
                Debug.LogError("���� ���������� �����ϴ�! Build Profiles�� Stage 2-4�� �߰��ߴ��� Ȯ�����ּ���!");
            }
        }


    }
}