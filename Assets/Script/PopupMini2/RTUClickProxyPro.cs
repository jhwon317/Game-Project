using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PopupMini
{
    [DisallowMultipleComponent]
    public class RTUIClickProxyPro : MonoBehaviour,
        IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        [Header("Targets (auto-filled if empty)")]
        public Camera targetCamera;              // 퍼즐 카메라 (자동)
        public Transform targetRoot;               // 퍼즐 루트 (자동)

        [Header("Auto settings")]
        public bool autoAssignOnEnable = true;
        public bool trackForAFewFrames = true;
        [Range(0, 30)] public int autoAssignFrameBudget = 10;

        [Tooltip("WorldSpace/ScreenSpaceCamera Canvas의 worldCamera를 targetCamera로 설정")]
        public bool setCanvasWorldCamera = true;

        CamToRawImage _view;
        int _framesTried;
        bool _dispatching; // 재진입 가드

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
        public void RefreshAuto() => TryAutoAssign();

        void TryAutoAssign()
        {
            if (!_view) _view = GetComponent<CamToRawImage>();
            if (!targetCamera && _view && _view.cam) targetCamera = _view.cam;

            if (!targetRoot && targetCamera)
            {
                var ctrl = targetCamera.GetComponentInParent<IPuzzleController>(true);
                if (ctrl is Component comp) targetRoot = comp.transform.root;
                if (!targetRoot) targetRoot = targetCamera.transform.root;
            }

            if (setCanvasWorldCamera && targetRoot && targetCamera)
            {
                var canvases = targetRoot.GetComponentsInChildren<Canvas>(true);
                foreach (var c in canvases)
                {
                    if (c.renderMode == RenderMode.WorldSpace || c.renderMode == RenderMode.ScreenSpaceCamera)
                        if (c.worldCamera != targetCamera) c.worldCamera = targetCamera;
                }
            }

#if UNITY_EDITOR
            if (!EventSystem.current)
                Debug.LogWarning("[RTUIClickProxyPro] EventSystem 없음 — UI 클릭 레이캐스트가 동작하지 않습니다.");
#endif
        }

        // ---------- 좌표 변환 ----------
        Vector2 ToCamScreenPos(PointerEventData e)
        {
            if (!targetCamera) return e.position;
            var rtf = transform as RectTransform;
            if (!rtf) return e.position;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rtf, e.position, e.pressEventCamera, out var lp))
                return e.position;

            var size = rtf.rect.size;
            var pivot = rtf.pivot;
            var local = lp + size * pivot;

            var uv = new Vector2(
                Mathf.Clamp01(size.x <= 0 ? 0.5f : local.x / size.x),
                Mathf.Clamp01(size.y <= 0 ? 0.5f : local.y / size.y)
            );
            return new Vector2(uv.x * targetCamera.pixelWidth, uv.y * targetCamera.pixelHeight);
        }

        // ---------- 퍼즐 루트 하위만 레이캐스트 ----------
        static bool IsUnder(Transform t, Transform root)
        {
            while (t)
            {
                if (t == root) return true;
                t = t.parent;
            }
            return false;
        }

        void RaycastToWorldUI(PointerEventData e, List<RaycastResult> outResults)
        {
            outResults.Clear();
            if (!targetCamera || !targetRoot) return;

            var es = EventSystem.current;
            if (!es) return;

            var pe = new PointerEventData(es)
            {
                pointerId = e.pointerId,
                button = e.button,
                position = ToCamScreenPos(e),
                delta = e.delta,
                clickCount = e.clickCount
            };

            var all = _tempHits ??= new List<RaycastResult>(32);
            all.Clear();
            es.RaycastAll(pe, all);

            // ★ 자기 자신(뷰포트/프록시) 및 퍼즐 루트 외 오브젝트는 제거
            foreach (var h in all)
            {
                var go = h.gameObject;
                if (!go) continue;
                if (go == gameObject) continue;                 // 자기 자신 제외
                if (!IsUnder(go.transform, targetRoot)) continue; // 퍼즐 루트 외 제외
                outResults.Add(h);
                if (outResults.Count >= 32) break; // 세이프가드
            }
        }
        List<RaycastResult> _tempHits;

        // ---------- 이벤트 전달 (재귀 방지) ----------
        void Forward<T>(PointerEventData e, ExecuteEvents.EventFunction<T> fn) where T : IEventSystemHandler
        {
            if (_dispatching) return; // 재진입 방지
            _dispatching = true;

            try
            {
                var hits = _fwdHits ??= new List<RaycastResult>(16);
                RaycastToWorldUI(e, hits);
                for (int i = 0; i < hits.Count; i++)
                {
                    var go = hits[i].gameObject;
                    if (!go || go == gameObject) continue; // 이중 안전장치
                    ExecuteEvents.Execute(go, e, fn);
                }
            }
            finally
            {
                _dispatching = false;
            }
        }
        List<RaycastResult> _fwdHits;

        public void OnPointerClick(PointerEventData e) => Forward<IPointerClickHandler>(e, ExecuteEvents.pointerClickHandler);
        public void OnPointerDown(PointerEventData e) => Forward<IPointerDownHandler>(e, ExecuteEvents.pointerDownHandler);
        public void OnPointerUp(PointerEventData e) => Forward<IPointerUpHandler>(e, ExecuteEvents.pointerUpHandler);
        public void OnPointerMove(PointerEventData e) { /* hover/drag 필요 시 확장 */ }
    }
}