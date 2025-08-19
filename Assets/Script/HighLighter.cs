using UnityEngine;

public class Highlighter : MonoBehaviour
{
    public Renderer targetRenderer;   // ������ ������
    public Color highlightColor = Color.yellow;

    private Material originalMaterial;
    private Color originalColor;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<Renderer>();
        if (targetRenderer)
        {
            // ��Ƽ���� �ν��Ͻ� ���� (���� ��Ƽ���� �Ѽ� ����)
            originalMaterial = targetRenderer.material;
            originalColor = originalMaterial.color;
        }
    }

    public void SetHighlight(bool on)
    {
        if (!targetRenderer) return;
        if (on) targetRenderer.material.color = highlightColor;
        else targetRenderer.material.color = originalColor;
    }
}
