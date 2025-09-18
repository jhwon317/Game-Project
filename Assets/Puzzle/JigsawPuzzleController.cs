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
        // 9���� ���� ��ġ(����)�� ���� ����
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(puzzleFrame);
            slot.AddComponent<RectTransform>().localScale = Vector3.one;
        }

        // [�ٽ� ����!] 9���� ������ '�������' ���� ����
        List<GameObject> pieces = new List<GameObject>();
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            GameObject newPiece = Instantiate(piecePrefab, piecesContainer);
            newPiece.name = $"Piece_{i}";
            newPiece.GetComponent<Image>().sprite = slicedSprites[i];

            // �������� "�� ���� ��ġ�� �ʶ� ��ȣ�� �Ȱ��� �����!" �ϰ� ¦�� �ξ���
            newPiece.GetComponent<DraggablePiece>().correctParent = puzzleFrame.GetChild(i);
            pieces.Add(newPiece);
        }

        // [�ٽ� ����!] ¦�� �� �ξ��� ������, �������� '����'�� ����
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

        if (piecesCorrect >= slicedSprites.Length)
        {
            Debug.Log("���� Ŭ����!");
        }
    }
}