using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(RawImage))]
public class RTUIClickProxyPro : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IScrollHandler
{
    [Header("Targets (세션에서 Bind)")]
    public Camera targetCamera;                 // PuzzleCam
    public Canvas targetCanvas;                 // ★ 추가
    public GraphicRaycaster targetRaycaster;    // 프리팹 Canvas의 GR

    RectTransform _rt; RawImage _img;
    GameObject _hovered, _pressed;
    bool _dragging;

    void Awake()
    {
        _rt = (RectTransform)transform;
        _img = GetComponent<RawImage>();
        if (!_img.raycastTarget) _img.raycastTarget = true;
    }

    public void Bind(Camera cam, Canvas canvas, GraphicRaycaster raycaster)
    {
        targetCamera = cam;
        targetCanvas = canvas;
        targetRaycaster = raycaster;

        // ★ 핵심: Canvas가 카메라 기반으로 렌더되도록 보정
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

    public void BindAuto(Camera cam, Transform prefabRoot)
    {
        targetCamera = cam;

        if (!targetCanvas && prefabRoot)
            targetCanvas = prefabRoot.GetComponentInChildren<Canvas>(true);

        if (targetCanvas && !targetRaycaster)
            targetRaycaster = targetCanvas.GetComponent<GraphicRaycaster>()
                              ?? targetCanvas.gameObject.AddComponent<GraphicRaycaster>();

        EnsureCanvasCamera();
    }

    // 공통 보정
    void EnsureCanvasCamera()
    {
        if (!targetCanvas) return;

        if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            targetCanvas.renderMode = RenderMode.ScreenSpaceCamera;

        if (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera ||
            targetCanvas.renderMode == RenderMode.WorldSpace)
            targetCanvas.worldCamera = targetCamera;

        // 디버그
        Debug.Log($"[RTUIProxy] bind cam={(targetCamera ? targetCamera.name : "null")}, " +
                  $"canvas={(targetCanvas ? targetCanvas.name : "null")}, " +
                  $"gr={(targetRaycaster ? targetRaycaster.name : "null")}");
    }

    public void Unbind()
    {
        targetCamera = null;
        targetCanvas = null;
        targetRaycaster = null;
        _hovered = _pressed = null;
        _dragging = false;
    }

    bool TryMapToCamScreen(Vector2 rawImageScreenPos, out Vector2 camScreenPos)
    {
        camScreenPos = default;
        if (!targetCamera || !targetRaycaster || !_rt || !_img || !_img.texture) return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, rawImageScreenPos, null, out var local))
            return false;

        var rect = _rt.rect;
        var uv = new Vector2(
            Mathf.InverseLerp(rect.xMin, rect.xMax, local.x),
            Mathf.InverseLerp(rect.yMin, rect.yMax, local.y)
        );

        var uvr = _img.uvRect;
        uv.x = Mathf.Lerp(uvr.xMin, uvr.xMax, uv.x);
        uv.y = Mathf.Lerp(uvr.yMin, uvr.yMax, uv.y);

        var tex = _img.texture;
        camScreenPos = new Vector2(uv.x * tex.width, uv.y * tex.height);
        return true;
    }

    bool TryRaycast(Vector2 camScreenPos, out RaycastResult hit, out PointerEventData ev)
    {
        hit = default; ev = null;
        var es = EventSystem.current;
        if (!es || !targetRaycaster) return false;

        // GraphicRaycaster는 내부적으로 targetCanvas.worldCamera를 사용
        ev = new PointerEventData(es)
        {
            position = camScreenPos,
            pressPosition = camScreenPos,
            button = PointerEventData.InputButton.Left,
            pointerId = -1
        };

        var results = new List<RaycastResult>();
        targetRaycaster.Raycast(ev, results);

        if (results.Count > 0)
        {
            hit = results[0];
            ev.pointerCurrentRaycast = hit;
            return true;
        }
        return false;
    }

    static void Exec(GameObject go, PointerEventData ev,
        ExecuteEvents.EventFunction<IPointerDownHandler> fnDown = null,
        ExecuteEvents.EventFunction<IPointerUpHandler> fnUp = null,
        ExecuteEvents.EventFunction<IPointerClickHandler> fnClick = null,
        ExecuteEvents.EventFunction<IPointerEnterHandler> fnEnter = null,
        ExecuteEvents.EventFunction<IPointerExitHandler> fnExit = null,
        ExecuteEvents.EventFunction<IPointerMoveHandler> fnMove = null,
        ExecuteEvents.EventFunction<IBeginDragHandler> fnBeginDrag = null,
        ExecuteEvents.EventFunction<IDragHandler> fnDrag = null,
        ExecuteEvents.EventFunction<IEndDragHandler> fnEndDrag = null,
        ExecuteEvents.EventFunction<IScrollHandler> fnScroll = null)
    {
        if (!go) return;
        if (fnDown != null) ExecuteEvents.ExecuteHierarchy(go, ev, fnDown);
        if (fnUp != null) ExecuteEvents.ExecuteHierarchy(go, ev, fnUp);
        if (fnClick != null) ExecuteEvents.ExecuteHierarchy(go, ev, fnClick);
        if (fnEnter != null) ExecuteEvents.ExecuteHierarchy(go, ev, fnEnter);
        if (fnExit != null) ExecuteEvents.ExecuteHierarchy(go, ev, fnExit);
        if (fnMove != null) ExecuteEvents.ExecuteHierarchy(go, ev, fnMove);
        if (fnBeginDrag != null) ExecuteEvents.ExecuteHierarchy(go, ev, fnBeginDrag);
        if (fnDrag != null) ExecuteEvents.ExecuteHierarchy(go, ev, fnDrag);
        if (fnEndDrag != null) ExecuteEvents.ExecuteHierarchy(go, ev, fnEndDrag);
        if (fnScroll != null) ExecuteEvents.ExecuteHierarchy(go, ev, fnScroll);
    }

    // ----- RawImage에서 받은 이벤트를 타깃으로 전달 -----

    public void OnPointerEnter(PointerEventData e)
    {
        if (!TryMapToCamScreen(e.position, out var sp)) return;
        if (!TryRaycast(sp, out var hit, out var ev)) return;

        if (_hovered != hit.gameObject)
        {
            // 이전 대상 exit
            if (_hovered) Exec(_hovered, ev, fnExit: ExecuteEvents.pointerExitHandler);
            _hovered = hit.gameObject;
            Exec(_hovered, ev, fnEnter: ExecuteEvents.pointerEnterHandler);
        }
    }

    public void OnPointerMove(PointerEventData e)
    {
        if (!TryMapToCamScreen(e.position, out var sp)) return;
        if (!TryRaycast(sp, out var hit, out var ev))
        {
            // 타깃 UI 벗어나면 exit
            if (_hovered) { Exec(_hovered, ev, fnExit: ExecuteEvents.pointerExitHandler); _hovered = null; }
            return;
        }

        // hover 대상 변경 처리
        if (_hovered != hit.gameObject)
        {
            if (_hovered) Exec(_hovered, ev, fnExit: ExecuteEvents.pointerExitHandler);
            _hovered = hit.gameObject;
            Exec(_hovered, ev, fnEnter: ExecuteEvents.pointerEnterHandler);
        }

        Exec(hit.gameObject, ev, fnMove: ExecuteEvents.pointerMoveHandler);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (!EventSystem.current) return;
        var ev = new PointerEventData(EventSystem.current);
        if (_hovered) { Exec(_hovered, ev, fnExit: ExecuteEvents.pointerExitHandler); _hovered = null; }
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (!TryMapToCamScreen(e.position, out var sp)) return;
        if (!TryRaycast(sp, out var hit, out var ev)) return;

        _pressed = hit.gameObject;
        Exec(_pressed, ev, fnDown: ExecuteEvents.pointerDownHandler);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (_pressed == null) return;
        if (!TryMapToCamScreen(e.position, out var sp)) return;
        if (!TryRaycast(sp, out var hit, out var ev)) return;

        if (!_dragging)
        {
            _dragging = true;
            Exec(_pressed, ev, fnBeginDrag: ExecuteEvents.beginDragHandler);
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_dragging) return;
        if (!TryMapToCamScreen(e.position, out var sp)) return;
        if (!TryRaycast(sp, out var hit, out var ev)) return;

        Exec(_pressed, ev, fnDrag: ExecuteEvents.dragHandler);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!_dragging) return;
        if (!TryMapToCamScreen(e.position, out var sp)) return;
        if (!TryRaycast(sp, out var hit, out var ev)) return;

        _dragging = false;
        Exec(_pressed, ev, fnEndDrag: ExecuteEvents.endDragHandler);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!TryMapToCamScreen(e.position, out var sp)) return;
        if (!TryRaycast(sp, out var hit, out var ev)) return;

        Exec(hit.gameObject, ev, fnUp: ExecuteEvents.pointerUpHandler);

        // 클릭 판정은 OnPointerClick에서
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (_pressed == null) return;
        if (!TryMapToCamScreen(e.position, out var sp)) return;
        if (!TryRaycast(sp, out var hit, out var ev)) return;

        // 눌렀던 대상과 같을 때만 클릭
        if (hit.gameObject == _pressed)
            Exec(hit.gameObject, ev, fnClick: ExecuteEvents.pointerClickHandler);

        _pressed = null;
    }

    public void OnScroll(PointerEventData e)
    {
        if (!TryMapToCamScreen(e.position, out var sp)) return;
        if (!TryRaycast(sp, out var hit, out var ev)) return;

        ev.scrollDelta = e.scrollDelta;
        Exec(hit.gameObject, ev, fnScroll: ExecuteEvents.scrollHandler);
    }
}
