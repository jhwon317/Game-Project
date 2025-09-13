// Assets/Script/PopupMini2/Sample/FirePin3DController.cs
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PopupMini.Sample
{
    /// <summary>
    /// RenderTexture→RawImage(Viewport) 위에서 드래그하여
    /// 3D 핀을 '한 축'으로만 빼내는 퍼즐 컨트롤러.
    /// - 마우스 다운 시 평면 교점(초기 지점)을 저장
    /// - 드래그 중 초기 지점과의 '축 방향 거리'가 successThreshold 이상이면 성공
    /// - 핀의 시각적 이동은 홈 기준 0..maxPullDistance로 제한
    /// - ESC로 취소
    /// </summary>
    public class FirePin3DController : MonoBehaviour, PopupMini.IPuzzleController
    {
        public event Action<PopupMini.PuzzleResult> Completed;

        [Header("Viewport / Camera")]
        [Tooltip("Panel/Content/Viewport (RawImage)의 RectTransform")]
        public RectTransform viewportRect;     // ★ 반드시 할당
        [Tooltip("퍼즐 전용 카메라 (PuzzleCam). 비워두면 자식에서 탐색")]
        public Camera puzzleCamera;

        [Header("Pin Refs")]
        [Tooltip("드래그할 핀(콜라이더 필수)")]
        public Transform pin;
        [Tooltip("핀 홈(시작 기준점)")]
        public Transform pinHome;

        [Header("Axis / Distances")]
        [Tooltip("축 기준 Transform. 지정 시 이 Transform.forward가 축이 됨")]
        public Transform axisRef;
        [Tooltip("axisRef가 없을 때 사용할 월드 방향")]
        public Vector3 pullDirection = new Vector3(0, 0, 1);
        [Min(0.001f)] public float maxPullDistance = 0.12f; // 홈 기준 최대 이동(m)
        [Min(0.001f)] public float successThreshold = 0.10f; // 초기 지점 기준 성공 거리(m)

        [Header("Interaction")]
        [Tooltip("핀 전용 레이어만 포함하세요 (메인 씬 오브젝트와 분리)")]
        public LayerMask draggableMask = -1;
        public float raycastMaxDistance = 6f;

        [Header("Release")]
        public bool snapBackOnRelease = true;
        public float returnSpeed = 2.5f; // m/s

        [Header("SFX (optional)")]
        public AudioSource sfx;
        public AudioClip sfxGrab, sfxRelease, sfxSuccess;

        // ---- state ----
        CancellationToken _ct;
        bool _dragging;
        bool _finished;
        Vector3 _homePos;
        Vector3 _axisN;        // 이동 축 (정규화)
        float _t;            // 홈 기준 0..maxPullDistance

        // 드래그 시작(초기) 평면 교점
        bool _haveDownWorld;
        Vector3 _downWorld;

        void Awake()
        {
            if (!puzzleCamera) puzzleCamera = GetComponentInChildren<Camera>(true);
            RecalcAxis();
        }

        void RecalcAxis()
        {
            _axisN = axisRef ? axisRef.forward.normalized
                             : (pullDirection.sqrMagnitude < 1e-6f ? Vector3.forward : pullDirection.normalized);
        }

        public void Begin(object args, CancellationToken ct)
        {
            _ct = ct;

            if (!viewportRect)
            {
                Debug.LogError("[FirePin3D] viewportRect 미지정 (Panel/Content/Viewport RectTransform 할당 필요)");
                SafeComplete(PuzzleResult.Error("setup:viewport"));
                return;
            }
            if (!pin || !pinHome)
            {
                Debug.LogError("[FirePin3D] pin/pinHome 연결 필요");
                SafeComplete(PuzzleResult.Error("setup:pin"));
                return;
            }
            if (!puzzleCamera)
            {
                Debug.LogError("[FirePin3D] puzzleCamera 누락");
                SafeComplete(PuzzleResult.Error("setup:camera"));
                return;
            }

            _homePos = pinHome.position;
            _t = 0f;
            pin.position = _homePos;
            _finished = false;
            _dragging = false;
            _haveDownWorld = false;

            StartCoroutine(CoWatchCancel());
        }

        System.Collections.IEnumerator CoWatchCancel()
        {
            while (!_ct.IsCancellationRequested && !_finished) yield return null;
            if (!_finished && _ct.IsCancellationRequested)
                SafeComplete(PuzzleResult.Cancel("abort:external"));
        }

        void Update()
        {
            if (_finished || _ct.IsCancellationRequested) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Play(sfxRelease);
                SafeComplete(PuzzleResult.Cancel("abort:user"));
                return;
            }

            bool down = Input.GetMouseButtonDown(0);
            bool hold = Input.GetMouseButton(0);
            bool up = Input.GetMouseButtonUp(0);
            Vector2 screenPos = Input.mousePosition;

            if (down)
            {
                if (TryHitPin(screenPos))
                {
                    _dragging = true;
                    _haveDownWorld = TryIntersectOnAxisPlane(screenPos, out _downWorld);
                    Play(sfxGrab);
                }
            }

            if (_dragging && hold)
            {
                if (TryIntersectOnAxisPlane(screenPos, out var cur))
                {
                    // 1) 성공 판정: 초기 교점 기준으로 '축 방향 거리'만 확인
                    if (_haveDownWorld)
                    {
                        float fromDown = Vector3.Dot(cur - _downWorld, _axisN); // ±m
                        if (Mathf.Abs(fromDown) >= successThreshold)
                        {
                            _finished = true;
                            Play(sfxSuccess ?? sfxRelease);
                            SafeComplete(PuzzleResult.Ok("{\"event\":\"pin_pulled\"}"));
                            return;
                        }
                    }

                    // 2) 핀 이동(표시): 홈 기준 0..max 범위로 제한
                    float tHome = Mathf.Clamp(Vector3.Dot(cur - _homePos, _axisN), 0f, maxPullDistance);
                    _t = tHome;
                    pin.position = _homePos + _axisN * _t;
                }
            }

            if (up)
            {
                _dragging = false;
                _haveDownWorld = false;
                if (!_finished) Play(sfxRelease);
            }

            if (!_dragging && !_finished && snapBackOnRelease)
            {
                _t = Mathf.MoveTowards(_t, 0f, returnSpeed * Time.deltaTime);
                pin.position = _homePos + _axisN * _t;
            }
        }

        // ---------- 좌표 변환: Screen → Viewport 로컬 → UV → 퍼즐카메라 픽셀 ----------
        bool TryGetCamRay(Vector2 screenPos, out Ray ray)
        {
            ray = default;
            if (!viewportRect || !puzzleCamera) return false;

            // UI 카메라 결정 (Overlay면 null, ScreenSpaceCamera/WorldSpace면 worldCamera)
            Camera uiCam = null;
            var canvas = viewportRect.GetComponentInParent<Canvas>();
            if (canvas && (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace))
                uiCam = canvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewportRect, screenPos, uiCam, out var lp))
                return false;

            var size = viewportRect.rect.size;
            var pivot = viewportRect.pivot;
            var local = lp + size * pivot; // (0..w, 0..h)
            var uv = new Vector2(
                Mathf.Clamp01(size.x <= 0 ? 0.5f : local.x / size.x),
                Mathf.Clamp01(size.y <= 0 ? 0.5f : local.y / size.y)
            );

            // 퍼즐 카메라 픽셀 좌표 (RT 크기와 일치)
            var px = new Vector2(uv.x * puzzleCamera.pixelWidth, uv.y * puzzleCamera.pixelHeight);
            ray = puzzleCamera.ScreenPointToRay(px);
            return true;
        }

        bool TryHitPin(Vector2 screenPos)
        {
            if (!TryGetCamRay(screenPos, out var ray)) return false;
            if (Physics.Raycast(ray, out var hit, raycastMaxDistance, draggableMask.value))
            {
                return hit.transform == pin || hit.transform.IsChildOf(pin);
            }
            return false;
        }

        // 축에 수직인 평면(axisN, homePos)과의 교점
        bool TryIntersectOnAxisPlane(Vector2 screenPos, out Vector3 world)
        {
            world = default;
            if (!TryGetCamRay(screenPos, out var ray)) return false;
            var plane = new Plane(_axisN, _homePos);
            if (plane.Raycast(ray, out float enter))
            {
                world = ray.GetPoint(enter);
                return true;
            }
            return false;
        }

        void Play(AudioClip clip)
        {
            if (clip && sfx) sfx.PlayOneShot(clip);
        }

        void SafeComplete(PuzzleResult r)
        {
            if (_finished) return;
            _finished = true;
            Completed?.Invoke(r);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            RecalcAxis();
            var pos = pinHome ? pinHome.position : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos, pos + _axisN * maxPullDistance);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos, pos + _axisN * successThreshold);
        }
#endif
    }
}
