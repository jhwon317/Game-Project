using UnityEngine;
public class RectWatch : MonoBehaviour
{
    RectTransform rt; Vector2 prevSize; Vector2 prevMin, prevMax; Vector2 prevOffMin, prevOffMax; Vector3 prevScale;
    void Awake() { rt = (RectTransform)transform; Snap("Awake"); }
    void LateUpdate()
    {
        if (!rt) return;
        if (rt.rect.size != prevSize || rt.anchorMin != prevMin || rt.anchorMax != prevMax ||
            rt.offsetMin != prevOffMin || rt.offsetMax != prevOffMax || rt.localScale != prevScale)
        {
            Snap("Changed");
        }
    }
    void Snap(string tag)
    {
        prevSize = rt.rect.size; prevMin = rt.anchorMin; prevMax = rt.anchorMax;
        prevOffMin = rt.offsetMin; prevOffMax = rt.offsetMax; prevScale = rt.localScale;
        Debug.Log($"[RectWatch:{tag}] {name} size={prevSize} anchor=({prevMin}->{prevMax}) offset=({prevOffMin},{prevOffMax}) scale={prevScale}");
    }
}