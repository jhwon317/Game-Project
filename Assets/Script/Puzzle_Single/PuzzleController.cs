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
            slot.AddComponent<RectTransform>();

            // [�ٽ�!] �ٷ� �� �κ��� �װ� �߰��ϰ� �;��� �ڵ��!
            // ���Կ� ���� ���� �� �̹����� �߰�
            Image slotImage = slot.AddComponent<Image>();
            slotImage.sprite = gridSprite;
            slotImage.color = new Color(1f, 1f, 1f, 0.2f); // ���� ����ϰ� (���� 20%)
            slotImage.raycastTarget = false; // Ŭ���� �������� �ʵ��� ����!
        }

        // 9���� ������ '�������' ���� �����ؼ� ¦�� �ξ���
        List<GameObject> pieces = new List<GameObject>();
        for (int i = 0; i < slicedSprites.Length; i++)
        {
            // piecesContainer ���, �� ��ũ��Ʈ�� �پ��ִ� Panel ���� �ٷ� ����
            GameObject newPiece = Instantiate(piecePrefab, transform);
            newPiece.name = $"Piece_{i}";
            newPiece.GetComponent<Image>().sprite = slicedSprites[i];

            // �װ� �̸��� �ٲ� Drag.cs�� ã���� ����
            newPiece.GetComponent<Drag>().correctParent = puzzleFrame.GetChild(i);
            pieces.Add(newPiece);

            // PuzzlePanel ���� �ȿ��� ������ ��ġ�� ��Ѹ���
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
            Debug.Log("���� Ŭ����!");

            // "�� �� Ǯ�����!" �ϰ� �����
            OnPuzzleSolved?.Invoke();
        }
    }


}
