using UnityEngine;
using UnityEngine.UI;

public class RectChainDebug : MonoBehaviour
{
    public RectTransform panelRoot;
    public RectTransform contentRoot;

    void LateUpdate()
    {
        var canvas = GetComponentInParent<Canvas>()?.rootCanvas;
        var view = transform as RectTransform;

        string s = $"[RectChain] " +
                   $"Canvas={canvas?.pixelRect.size ?? Vector2.zero} " +
                   $"Panel={Size(panelRoot)} " +
                   $"Content={Size(contentRoot)} " +
                   $"View={Size(view)}";
        Debug.Log(s);
    }

    Vector2 Size(RectTransform rt) => rt ? rt.rect.size : Vector2.zero;
}
