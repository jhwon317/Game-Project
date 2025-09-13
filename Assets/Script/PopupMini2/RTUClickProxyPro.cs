// Assets/Script/PopupMini2/RTUIClickProxyPro.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PopupMini
{
    [DisallowMultipleComponent]
    public class RTUIClickProxyPro : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerMoveHandler
    {
        [Header("Targets (auto-filled if empty)")]
        public Camera targetCamera;     // 퍼즐 카메라
        public Transform targetRoot;    // 퍼즐 루트(해당 루트 아래 UI만 타겟)

        [Header("Auto settings")]
        public bool autoAssignOnEnable = true;
        public bool trackForAFewFrames = true;
        [Range(0, 30)] public int autoAssignFrameBudget = 10;
        public bool setCanvasWorldCamera = true;

        CamToRawImage _view;
        int _framesTried;

        // pointerId -> pressTarget, pressPosition(카메라 픽셀좌표) 캐시
        readonly Dictionary<int, GameObject> _pressTargetById = new();
        readonly Dictionary<int, Vector2> _pressPosById = new();

        readonly Dictionary<int, GameObject> _hoverTargetById = new();

        void Awake() => _view = GetComponent<CamToRawImage>();

        void OnEnable()
        {
            _framesTried = 0;
            if (autoAssignOnEnable)
            {
                TryAutoAssign();
                if (trackForAFewFrames && !HasTargets())
                    StartCoroutine(CoPollAssign());
            }
        }

        System.Collections.IEnumerator CoPollAssign()
        {
            while (!HasTargets() && _framesTried < autoAssignFrameBudget)
            {
                _framesTried++;
                TryAutoAssign();
                yield return null;
            }
        }

        bool HasTargets() => targetCamera && targetRoot;

        void TryAutoAssign()
        {
            if (!_view) _view = GetComponent<CamToRawImage>();
            if (!targetCamera && _view && _view.cam) targetCamera = _view.cam;

            if (!targetRoot && targetCamera)
            {
                var ctrl = targetCamera.GetComponentInParent<IPuzzleController>(true);
                if (ctrl is Component c) targetRoot = c.transform.root;
                if (!targetRoot) targetRoot = targetCamera.transform.root;
            }

            if (setCanvasWorldCamera && targetRoot && targetCamera)
            {
                foreach (var c in targetRoot.GetComponentsInChildren<Canvas>(true))
                {
                    if (c.renderMode == RenderMode.ScreenSpaceCamera || c.renderMode == RenderMode.WorldSpace)
                        if (c.worldCamera != targetCamera) c.worldCamera = targetCamera;
                }
            }
        }

        // ---------- 좌표 변환: Viewport RawImage → 퍼즐 카메라 픽셀좌표 ----------
        Vector2 ToCamScreenPos(PointerEventData e)
        {
            if (!targetCamera) return e.position;
            if (!(transform is RectTransform rtf)) return e.position;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rtf, e.position, e.pressEventCamera, out var lp))
                return e.position;

            var size = rtf.rect.size;
            var pivot = rtf.pivot;
            var local = lp + size * pivot;

            var uv = new Vector2(
                Mathf.Clamp01(size.x <= 0 ? .5f : local.x / size.x),
                Mathf.Clamp01(size.y <= 0 ? .5f : local.y / size.y)
            );

            return new Vector2(uv.x * targetCamera.pixelWidth, uv.y * targetCamera.pixelHeight);
        }

        // ---------- 레이캐스트(퍼즐 루트 하위만) ----------
        static bool IsUnder(Transform t, Transform root)
        {
            while (t)
            {
                if (t == root) return true;
                t = t.parent;
            }
            return false;
        }

        void UpdateHover(PointerEventData e, bool alsoSendMove)
        {
            RaycastToWorldUI(e, _hits);
            var top = _hits.Count > 0 ? _hits[0].gameObject : null;

            _hoverTargetById.TryGetValue(e.pointerId, out var prev);

            if (prev != top)
            {
                if (prev) Exec(prev, e, ExecuteEvents.pointerExitHandler);
                if (top) Exec(top, e, ExecuteEvents.pointerEnterHandler);
                if (top) _hoverTargetById[e.pointerId] = top;
                else _hoverTargetById.Remove(e.pointerId);
            }

            if (alsoSendMove && top)
                Exec(top, e, ExecuteEvents.pointerMoveHandler);
        }

        readonly List<RaycastResult> _all = new(64);
        readonly List<RaycastResult> _hits = new(32);

        void RaycastToWorldUI(PointerEventData src, List<RaycastResult> outHits)
        {
            outHits.Clear();
            var es = EventSystem.current;
            if (!es || !targetRoot) return;

            // 좌표만 변환해서 RaycastAll
            var pos = ToCamScreenPos(src);
            var tmp = new PointerEventData(es)
            {
                pointerId = src.pointerId,
                position = pos,
                button = src.button
            };

            _all.Clear();
            es.RaycastAll(tmp, _all);

            foreach (var h in _all)
            {
                var go = h.gameObject;
                if (!go) continue;
                if (go == gameObject) continue;               // 자기 자신 제외
                if (!IsUnder(go.transform, targetRoot)) continue; // 퍼즐 루트 밖 제외
                outHits.Add(h);
                if (outHits.Count >= 32) break;
            }
        }

        // ---------- 이벤트 전달 유틸 ----------
        void Exec<T>(GameObject go, PointerEventData src, ExecuteEvents.EventFunction<T> fn, bool isDown = false) where T : IEventSystemHandler
        {
            var es = EventSystem.current;
            var pos = ToCamScreenPos(src);

            var pe = new PointerEventData(es)
            {
                pointerId = src.pointerId,
                button = src.button,
                position = pos,
                delta = src.delta,
                clickCount = src.clickCount,
                clickTime = src.clickTime,
                dragging = src.dragging,
                useDragThreshold = src.useDragThreshold,
                scrollDelta = src.scrollDelta,
            };

            // pressPosition은 읽/셋 가능 → 첫 프레임 점프 방지용 캐시 사용
            if (isDown)
            {
                _pressPosById[src.pointerId] = pos;
                pe.pressPosition = pos;
            }
            else if (_pressPosById.TryGetValue(src.pointerId, out var p))
            {
                pe.pressPosition = p;
            }
            else
            {
                pe.pressPosition = src.pressPosition; // 백업
            }

            ExecuteEvents.Execute(go, pe, fn);
        }

        // ---------- IPointer/Drag 구현 ----------
        public void OnPointerDown(PointerEventData e)
        {
            RaycastToWorldUI(e, _hits);
            var top = _hits.Count > 0 ? _hits[0].gameObject : null;
            if (top) _pressTargetById[e.pointerId] = top;
            if (top) Exec(top, e, ExecuteEvents.pointerDownHandler, isDown: true);
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (!_pressTargetById.TryGetValue(e.pointerId, out var go) || !go)
            {
                RaycastToWorldUI(e, _hits);
                go = _hits.Count > 0 ? _hits[0].gameObject : null;
                if (go) _pressTargetById[e.pointerId] = go;
            }
            if (go) Exec(go, e, ExecuteEvents.beginDragHandler);
        }

        public void OnDrag(PointerEventData e)
        {
            if (_pressTargetById.TryGetValue(e.pointerId, out var go) && go)
                Exec(go, e, ExecuteEvents.dragHandler);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (_pressTargetById.TryGetValue(e.pointerId, out var go) && go)
                Exec(go, e, ExecuteEvents.endDragHandler);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (_pressTargetById.TryGetValue(e.pointerId, out var go) && go)
                Exec(go, e, ExecuteEvents.pointerUpHandler);
            _pressTargetById.Remove(e.pointerId);
            _pressPosById.Remove(e.pointerId);
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (_pressTargetById.TryGetValue(e.pointerId, out var go) && go)
                Exec(go, e, ExecuteEvents.pointerClickHandler);
        }

        public void OnPointerMove(PointerEventData e) { UpdateHover(e, alsoSendMove: true); }
    }
}
