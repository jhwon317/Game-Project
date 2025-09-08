using UnityEngine;
using UnityEngine.UI;

public class RectInspector : MonoBehaviour
{
    public RectTransform target;

    void Reset() { target = transform as RectTransform; }

    void LateUpdate()
    {
        var rt = target ? target : (RectTransform)transform;
        string why = "";
        if (rt.GetComponent<LayoutElement>()) why += " LayoutElement";
        if (rt.GetComponent<ContentSizeFitter>()) why += " ContentSizeFitter";
        if (rt.GetComponent<AspectRatioFitter>()) why += " AspectRatioFitter";
        if (rt.GetComponent<HorizontalOrVerticalLayoutGroup>()) why += " H/VLayoutGroup";
        if (rt.GetComponent<GridLayoutGroup>()) why += " GridLayoutGroup";

        var s = rt.rect.size;
        Debug.Log($"[RectInspector] {rt.name} size={s} causes:{why}");
    }
}
