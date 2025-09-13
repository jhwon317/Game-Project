// Assets/Script/PopupMini2/Sample/FirePin3DController.cs
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PopupMini.Sample
{
    /// <summary>
    /// RenderTexture��RawImage(Viewport) ������ �巡���Ͽ�
    /// 3D ���� '�� ��'���θ� ������ ���� ��Ʈ�ѷ�.
    /// - ���콺 �ٿ� �� ��� ����(�ʱ� ����)�� ����
    /// - �巡�� �� �ʱ� �������� '�� ���� �Ÿ�'�� successThreshold �̻��̸� ����
    /// - ���� �ð��� �̵��� Ȩ ���� 0..maxPullDistance�� ����
    /// - ESC�� ���
    /// </summary>
    public class FirePin3DController : MonoBehaviour, PopupMini.IPuzzleController
    {
        public event Action<PopupMini.PuzzleResult> Completed;

        [Header("Viewport / Camera")]
        [Tooltip("Panel/Content/Viewport (RawImage)�� RectTransform")]
        public RectTransform viewportRect;     // �� �ݵ�� �Ҵ�
        [Tooltip("���� ���� ī�޶� (PuzzleCam). ����θ� �ڽĿ��� Ž��")]
        public Camera puzzleCamera;

        [Header("Pin Refs")]
        [Tooltip("�巡���� ��(�ݶ��̴� �ʼ�)")]
        public Transform pin;
        [Tooltip("�� Ȩ(���� ������)")]
        public Transform pinHome;

        [Header("Axis / Distances")]
        [Tooltip("�� ���� Transform. ���� �� �� Transform.forward�� ���� ��")]
        public Transform axisRef;
        [Tooltip("axisRef�� ���� �� ����� ���� ����")]
        public Vector3 pullDirection = new Vector3(0, 0, 1);
        [Min(0.001f)] public float maxPullDistance = 0.12f; // Ȩ ���� �ִ� �̵�(m)
        [Min(0.001f)] public float successThreshold = 0.10f; // �ʱ� ���� ���� ���� �Ÿ�(m)

        [Header("Interaction")]
        [Tooltip("�� ���� ���̾ �����ϼ��� (���� �� ������Ʈ�� �и�)")]
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
        Vector3 _axisN;        // �̵� �� (����ȭ)
        float _t;            // Ȩ ���� 0..maxPullDistance

        // �巡�� ����(�ʱ�) ��� ����
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
                Debug.LogError("[FirePin3D] viewportRect ������ (Panel/Content/Viewport RectTransform �Ҵ� �ʿ�)");
                SafeComplete(PuzzleResult.Error("setup:viewport"));
                return;
            }
            if (!pin || !pinHome)
            {
                Debug.LogError("[FirePin3D] pin/pinHome ���� �ʿ�");
                SafeComplete(PuzzleResult.Error("setup:pin"));
                return;
            }
            if (!puzzleCamera)
            {
                Debug.LogError("[FirePin3D] puzzleCamera ����");
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
                    // 1) ���� ����: �ʱ� ���� �������� '�� ���� �Ÿ�'�� Ȯ��
                    if (_haveDownWorld)
                    {
                        float fromDown = Vector3.Dot(cur - _downWorld, _axisN); // ��m
                        if (Mathf.Abs(fromDown) >= successThreshold)
                        {
                            _finished = true;
                            Play(sfxSuccess ?? sfxRelease);
                            SafeComplete(PuzzleResult.Ok("{\"event\":\"pin_pulled\"}"));
                            return;
                        }
                    }

                    // 2) �� �̵�(ǥ��): Ȩ ���� 0..max ������ ����
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

        // ---------- ��ǥ ��ȯ: Screen �� Viewport ���� �� UV �� ����ī�޶� �ȼ� ----------
        bool TryGetCamRay(Vector2 screenPos, out Ray ray)
        {
            ray = default;
            if (!viewportRect || !puzzleCamera) return false;

            // UI ī�޶� ���� (Overlay�� null, ScreenSpaceCamera/WorldSpace�� worldCamera)
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

            // ���� ī�޶� �ȼ� ��ǥ (RT ũ��� ��ġ)
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

        // �࿡ ������ ���(axisN, homePos)���� ����
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
