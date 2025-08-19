using UnityEngine;

public class Highlighter : MonoBehaviour
{
    public Renderer targetRenderer;   // 강조할 렌더러
    public Color highlightColor = Color.yellow;

    private Material originalMaterial;
    private Color originalColor;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<Renderer>();
        if (targetRenderer)
        {
            // 머티리얼 인스턴스 복제 (공유 머티리얼 훼손 방지)
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
