using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class JigsawPuzzleController : MonoBehaviour
{
    public GameObject piecePrefab;
    public Sprite[] slicedSprites;
    public Transform puzzleFrame;
    public Transform piecesContainer;

    // [���� �߰�!] ���� ������ �� �̹���
    public Sprite gridSprite;

    // [���� �߰�!] ������ �����ٰ� �˷��� �̺�Ʈ
    public event Action OnPuzzleSolved;

    private int piecesCorrect = 0;

    void Start()
    {
        GenerateAndShufflePieces();
    }

    void GenerateAndShufflePieces()
    {
        // 9���� ���� ��ġ(����)�� 'UI�� ������Ʈ'�� ����� ����
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(puzzleFrame, false);
            slot.AddComponent<RectTransform>(); // UI ��ǰ�̶�� �˷���!

            // ���Կ� ���� ���� �̹����� �߰�
            Image slotImage = slot.AddComponent<Image>();
            slotImage.sprite = gridSprite;
            slotImage.color = new Color(1f, 1f, 1f, 0.2f); // ���� ����ϰ� (���� 20%)
            slotImage.raycastTarget = false; // Ŭ���� �������� �ʵ��� ����!
        }

        // 9���� ������ '�������' ���� �����ؼ� ¦�� �ξ���
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

        // ¦�� �� �ξ��� ������, �������� '����'�� ����
        for (int i = 0; i < pieces.Count; i++)
        {
            // '����Ƽ ������'�� Random�̶�� ��Ȯ�� �˷���
            int randomIndex = UnityEngine.Random.Range(i, pieces.Count);
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

            // "�� �� Ǯ�����!" �ϰ� �����
            OnPuzzleSolved?.Invoke();
        }
    }
}