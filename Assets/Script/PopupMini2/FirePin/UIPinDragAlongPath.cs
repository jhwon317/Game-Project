// Assets/Script/PopupMini2/FirePin/UIPinDragAlongPath.cs
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PopupMini.Sample
{
    [RequireComponent(typeof(RectTransform))]
    public class UIPinDraggableHover : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Axis (in parent space)")]
        public Vector2 dragAxisInParent = Vector2.right; // (-1,0) = 왼쪽으로 당김
        public bool oneWay = true;        // true면 0~+max만, false면 -max~+max
        [Min(1f)] public float maxDistance = 120f;
        [Min(1f)] public float successDistance = 100f;

        [Header("Return")]
        public bool snapBackOnRelease = true;
        public float returnSpeed = 600f; // px/sec (unscaled)

        [Header("Hover fx")]
        public Graphic targetGraphic;
        public Color normalColor = Color.white;
        public Color hoverColor = new(1f, 1f, 1f, 1f);
        [Range(1f, 1.5f)] public float hoverScale = 1.06f;
        public float hoverLerpSpeed = 12f;

        [Header("SFX")]
        public AudioSource sfx;
        public AudioClip sfxGrab, sfxRelease, sfxSuccess, sfxHover;

        public event Action<float> OnProgress; // 0..1
        public event Action OnGrab, OnRelease, OnSuccess;

        RectTransform _rt, _parent;
        Vector2 _axisN;
        bool _dragging, _finished, _hover;
        Vector2 _startAnchored, _downLocalParent;
        float _t;
        Vector3 _baseScale;

        void Awake()
        {
            _rt = (RectTransform)transform;
            _parent = _rt.parent as RectTransform;
            _axisN = dragAxisInParent.sqrMagnitude < 1e-6f ? Vector2.right : dragAxisInParent.normalized;
            if (!targetGraphic) targetGraphic = GetComponent<Graphic>();
            _baseScale = _rt.localScale;
        }

        Camera GetEventCam(PointerEventData e)
        {
            if (e != null && e.pressEventCamera) return e.pressEventCamera;
            var root = _parent ? _parent.GetComponentInParent<Canvas>()?.rootCanvas : null;
            if (!root) return null;
            if (root.renderMode == RenderMode.ScreenSpaceOverlay) return null;
            return root.worldCamera;
        }

        public void ResetState()
        {
            _finished = _dragging = false;
            _t = 0f;
            _startAnchored = _rt.anchoredPosition;
            if (targetGraphic) targetGraphic.color = normalColor;
            _rt.localScale = _baseScale;
            OnProgress?.Invoke(0f);
        }

        // Hover
        public void OnPointerEnter(PointerEventData e)
        {
            if (_finished) return;
            _hover = true;
            if (sfx && sfxHover) sfx.PlayOneShot(sfxHover);
        }
        public void OnPointerExit(PointerEventData e)
        {
            if (_finished) return;
            if (!_dragging) _hover = false;
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (_finished || _parent == null) return;

            _startAnchored = _rt.anchoredPosition;

            var cam = GetEventCam(e);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, e.position, cam, out var local))
            {
                _downLocalParent = local;
                _dragging = true;
                OnGrab?.Invoke();
                if (sfx && sfxGrab) sfx.PlayOneShot(sfxGrab);
            }
        }

        public void OnDrag(PointerEventData e)
        {
            if (_finished || !_dragging || _parent == null) return;

            var cam = GetEventCam(e);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, e.position, cam, out var local))
                return;

            var delta = local - _downLocalParent;
            float t = Vector2.Dot(delta, _axisN);
            t = oneWay ? Mathf.Clamp(t, 0f, maxDistance)
                       : Mathf.Clamp(t, -maxDistance, maxDistance);

            ApplyDistance(t);

            if (_t >= successDistance)
            {
                _finished = true; _hover = false; _dragging = false;
                OnSuccess?.Invoke();
                if (sfx && sfxSuccess) sfx.PlayOneShot(sfxSuccess);
            }
        }

        public void OnPointerUp(PointerEventData e)
        {
            _dragging = false;
            OnRelease?.Invoke();
            if (!_finished && sfx && sfxRelease) sfx.PlayOneShot(sfxRelease);
        }

        void Update()
        {
            // 리턴 애니메이션
            if (!_finished && !_dragging && snapBackOnRelease && !Mathf.Approximately(_t, 0f))
            {
                float step = returnSpeed * Time.unscaledDeltaTime;
                ApplyDistance(Mathf.MoveTowards(_t, 0f, step));
            }

            // Hover 효과
            if (targetGraphic)
            {
                var cur = targetGraphic.color;
                var tar = _hover ? hoverColor : normalColor;
                targetGraphic.color = Color.Lerp(cur, tar, 1f - Mathf.Exp(-hoverLerpSpeed * Time.unscaledDeltaTime));
            }
            _rt.localScale = Vector3.Lerp(_rt.localScale, _hover ? _baseScale * hoverScale : _baseScale,
                                          1f - Mathf.Exp(-hoverLerpSpeed * Time.unscaledDeltaTime));
        }

        void ApplyDistance(float t)
        {
            _t = t;
            _rt.anchoredPosition = _startAnchored + _axisN * _t;
            var denom = Mathf.Max(1f, successDistance);
            OnProgress?.Invoke(Mathf.InverseLerp(0f, denom, Mathf.Abs(_t)));
        }
    }
}
