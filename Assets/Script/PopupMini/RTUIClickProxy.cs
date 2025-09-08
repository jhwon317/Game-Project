// RTUIClickProxy.cs (RawImage에 붙이기)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RTUIClickProxy : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IDragHandler, IScrollHandler
{
    public Camera targetCamera;                 // PuzzleCam
    public GraphicRaycaster targetRaycaster;    // 프리팹 Canvas의 GraphicRaycaster
    public Canvas targetCanvas;

    RectTransform _rt; RawImage _img;

    void Awake() { _rt = (RectTransform)transform; _img = GetComponent<RawImage>(); }

    public void Bind(Camera cam, GraphicRaycaster raycaster) { targetCamera = cam; targetRaycaster = raycaster; }
    public void Bind(Camera cam, Canvas canvas, GraphicRaycaster raycaster)
    {
        targetCamera = cam;
        targetCanvas = canvas;
        targetRaycaster = raycaster;

        // Canvas를 카메라 기반으로 보정
        if (targetCanvas)
        {
            if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                targetCanvas.renderMode = RenderMode.ScreenSpaceCamera;

            if (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera ||
                targetCanvas.renderMode == RenderMode.WorldSpace)
            {
                targetCanvas.worldCamera = targetCamera;
            }
        }
    }
    public void Unbind() { targetCamera = null; targetRaycaster = null; }

    // 공통: RawImage 내 로컬 → UV(0..1) → targetCam 픽셀 좌표로 변환
    bool TryMap(Vector2 screenPos, out Vector2 camScreenPos)
    {
        camScreenPos = default;
        if (!targetCamera || !targetRaycaster || !_rt || !_img || !_img.texture) return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, screenPos, null, out var local)) return false;

        var rect = _rt.rect;                        // RawImage 사각형
        var uv = new Vector2(
            Mathf.InverseLerp(rect.xMin, rect.xMax, local.x),
            Mathf.InverseLerp(rect.yMin, rect.yMax, local.y)
        );

        // uvRect(크롭) 보정
        var uvr = _img.uvRect;
        uv.x = Mathf.Lerp(uvr.xMin, uvr.xMax, uv.x);
        uv.y = Mathf.Lerp(uvr.yMin, uvr.yMax, uv.y);

        // RT 픽셀로
        var tex = _img.texture;
        var px = uv.x * tex.width;
        var py = uv.y * tex.height;

        camScreenPos = new Vector2(px, py);
        return true;
    }

    void RaycastAndDispatch(Vector2 camScreenPos, System.Action<GameObject, PointerEventData> dispatch)
    {
        var ev = new PointerEventData(EventSystem.current);
        ev.position = camScreenPos;
        ev.pressPosition = camScreenPos;
        ev.button = PointerEventData.InputButton.Left;
        ev.scrollDelta = Input.mouseScrollDelta;
        ev.pointerPressRaycast = new RaycastResult { module = targetRaycaster };

        var results = new List<RaycastResult>();
        targetRaycaster.Raycast(ev, results);
        if (results.Count > 0)
        {
            var go = results[0].gameObject;
            ev.pointerCurrentRaycast = results[0];
            dispatch(go, ev);
        }
    }

    public void OnPointerDown(PointerEventData e) { if (TryMap(e.position, out var sp)) RaycastAndDispatch(sp, (go, ev) => ExecuteEvents.Execute(go, ev, ExecuteEvents.pointerDownHandler)); }
    public void OnPointerUp(PointerEventData e) { if (TryMap(e.position, out var sp)) RaycastAndDispatch(sp, (go, ev) => ExecuteEvents.Execute(go, ev, ExecuteEvents.pointerUpHandler)); }
    public void OnPointerClick(PointerEventData e) { if (TryMap(e.position, out var sp)) RaycastAndDispatch(sp, (go, ev) => ExecuteEvents.Execute(go, ev, ExecuteEvents.pointerClickHandler)); }
    public void OnDrag(PointerEventData e) { if (TryMap(e.position, out var sp)) RaycastAndDispatch(sp, (go, ev) => ExecuteEvents.Execute(go, ev, ExecuteEvents.dragHandler)); }
    public void OnScroll(PointerEventData e) { if (TryMap(e.position, out var sp)) RaycastAndDispatch(sp, (go, ev) => ExecuteEvents.Execute(go, ev, ExecuteEvents.scrollHandler)); }
}
