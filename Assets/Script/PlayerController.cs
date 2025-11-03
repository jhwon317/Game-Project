// PlayerController.cs  (SimpleMove 스타일 + 호환 API 복구)
// - 이동/회전/애니메이션: SimplePlayerMove 방식
// - ExtinguisherUI/Helper가 기대하는 API 제공:
//     * public GameObject heldObject { get; }
//     * public ExtinguisherItem EquippedExtinguisher { get; }
//     * public bool EnterExtinguisherMode(ExtinguisherItem item)
//     * public void ExitExtinguisherMode(bool destroyExtinguisher = true)
// - 기존 Enter/ExitExtinguisherMode() (파라미터 없는 버전)도 유지

using UnityEngine;
using Game.Inventory; // EncumbranceComponent

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public bool useSideView2_5D = false; // 2.5D면 좌우만
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.5f;
    public float rotateSpeed = 540f;
    public float gravity = -9.81f;

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool toggleSprint = false;    // true: 토글, false: 누르는 동안만

    [Header("Interact")]
    public KeyCode interactKey = KeyCode.E;
    private PlayerInteractor _interactor;

    [Header("Animation Params")]
    public Animator animator;
    public string isMovingBool = "IsMoving"; // bool
    public string speedInt = "Speed";        // int (0=Idle,1=Walk,2=Run)

    // ---- Extinguisher ----
    [Header("Extinguisher")]
    public ExtinguisherController extinguisher;         // 실제 분사 컨트롤러
    public string isExtModeBool = "IsExtinguisherMode"; // 선택 파라미터
    public string sprayBool = "Spray";               // 선택 파라미터

    // === Helper/UI 호환을 위한 공개 프로퍼티 ===
    public ExtinguisherItem EquippedExtinguisher { get; private set; }
    public GameObject heldObject => EquippedExtinguisher ? EquippedExtinguisher.gameObject : null;

    public bool IsInExtinguisherMode { get; private set; }
    bool _spraying;

    // ---- Internal ----
    CharacterController _cc;
    Vector3 _vel;
    bool _sprintOn;
    EncumbranceComponent _encum;

    // Animator parameter guards
    bool _hasIsMoving, _hasSpeedInt, _hasIsExtMode, _hasSpray;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (animator) animator.applyRootMotion = false;

        _interactor = GetComponent<PlayerInteractor>();
        _encum = GetComponent<EncumbranceComponent>();
        if (!extinguisher) extinguisher = GetComponentInChildren<ExtinguisherController>(true);

        if (animator)
        {
            foreach (var p in animator.parameters)
            {
                if (p.type == AnimatorControllerParameterType.Bool && p.name == isMovingBool) _hasIsMoving = true;
                if (p.type == AnimatorControllerParameterType.Int && p.name == speedInt) _hasSpeedInt = true;
                if (p.type == AnimatorControllerParameterType.Bool && p.name == isExtModeBool) _hasIsExtMode = true;
                if (p.type == AnimatorControllerParameterType.Bool && p.name == sprayBool) _hasSpray = true;
            }
        }
    }

    void Update()
    {
        // --- 상호작용(E) ---
        if (Input.GetKeyDown(interactKey))
        {
            if (_interactor != null && _interactor.currentInteractable != null)
                _interactor.currentInteractable.OnInteract(gameObject);
        }

        // --- 입력 ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = useSideView2_5D ? 0f : Input.GetAxisRaw("Vertical");
        bool hasMoveInput = (new Vector2(h, v).sqrMagnitude > 0.01f);

        // --- 스프린트 ---
        if (toggleSprint) { if (Input.GetKeyDown(sprintKey)) _sprintOn = !_sprintOn; }
        else _sprintOn = Input.GetKey(sprintKey);
        if (_encum && !_encum.SprintAllowed) _sprintOn = false;
        bool applySprint = hasMoveInput && _sprintOn;

        // --- 이동 방향/회전 (Simple 스타일) ---
        Vector3 moveDir = Vector3.zero;
        if (hasMoveInput)
        {
            if (useSideView2_5D)
            {
                moveDir = new Vector3(Mathf.Sign(h), 0f, 0f);
                if (Mathf.Abs(h) > 0.01f)
                {
                    var face = new Vector3(h, 0f, 0f);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, Quaternion.LookRotation(face, Vector3.up),
                        rotateSpeed * Time.deltaTime);
                }
            }
            else
            {
                Vector3 input = new Vector3(h, 0f, v).normalized;
                var cam = Camera.main;
                if (cam)
                {
                    Vector3 fwd = cam.transform.forward; fwd.y = 0f; fwd.Normalize();
                    Vector3 right = cam.transform.right; right.y = 0f; right.Normalize();
                    moveDir = fwd * input.z + right * input.x;
                }
                else moveDir = input;

                if (moveDir.sqrMagnitude > 0.0001f)
                {
                    var target = Quaternion.LookRotation(moveDir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, target, rotateSpeed * Time.deltaTime);
                }
            }
        }

        // --- 속도/중력/이동 ---
        float speedMeters = 0f;
        if (hasMoveInput) speedMeters = applySprint ? sprintSpeed : walkSpeed;
        if (_encum) speedMeters *= _encum.SpeedScale;

        if (_cc.isGrounded && _vel.y < 0f) _vel.y = -2f;
        _vel.y += gravity * Time.deltaTime;

        Vector3 horizontal = moveDir * speedMeters;
        Vector3 motion = horizontal * Time.deltaTime + _vel * Time.deltaTime;
        _cc.Move(motion);

        // --- 소화기 분사 ---
        HandleExtinguisher();

        // --- 애니메이션 ---
        if (animator)
        {
            if (_hasIsMoving) animator.SetBool(isMovingBool, hasMoveInput);
            if (_hasSpeedInt)
            {
                int speedState = 0;
                if (hasMoveInput) speedState = applySprint ? 2 : 1;
                animator.SetInteger(speedInt, speedState);
            }
            if (_hasIsExtMode) animator.SetBool(isExtModeBool, IsInExtinguisherMode);
        }
    }

    // ===== Extinguisher =====
    void HandleExtinguisher()
    {
        if (!IsInExtinguisherMode || !extinguisher) return;

        bool pressed = false, released = false;

#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null) { pressed |= mouse.rightButton.isPressed; released |= mouse.rightButton.wasReleasedThisFrame; }
        var pad = Gamepad.current;
        if (pad != null) { pressed |= pad.rightTrigger.ReadValue() > 0.3f; released |= pad.rightTrigger.wasReleasedThisFrame; }
#endif
        pressed |= Input.GetMouseButton(1) || Input.GetButton("Fire2");
        released |= Input.GetMouseButtonUp(1) || Input.GetButtonUp("Fire2");

        if (pressed)
        {
            extinguisher.TrySpraying(Time.deltaTime);
            if (!_spraying)
            {
                _spraying = true;
                if (_hasSpray && animator) animator.SetBool(sprayBool, true);
            }
        }
        else if (released)
        {
            extinguisher.StopSpraying();
            if (_spraying)
            {
                _spraying = false;
                if (_hasSpray && animator) animator.SetBool(sprayBool, false);
            }
        }
        else if (_spraying)
        {
            extinguisher.TrySpraying(Time.deltaTime);
        }
    }

    // ===== 모드 API (ExtinguisherHelper/ExtinguisherUI 호환) =====

    // Helper가 호출하는 버전 (ExtinguisherItem을 넘겨줌)
    public bool EnterExtinguisherMode(ExtinguisherItem item)
    {
        if (!item || !item.controller)
        {
            Debug.LogError("[PlayerController] EnterExtinguisherMode(item): invalid item/controller");
            return false;
        }
        if (IsInExtinguisherMode)
        {
            Debug.LogWarning("[PlayerController] already in extinguisher mode");
            return false;
        }

        EquippedExtinguisher = item;
        extinguisher = item.controller;

        IsInExtinguisherMode = true;
        if (_hasIsExtMode && animator) animator.SetBool(isExtModeBool, true);
        Debug.Log("[PlayerController] EnterExtinguisherMode(item) OK");
        return true;
    }

    // 파라미터 없는 버전(기존 코드 호환/디버그)
    public void EnterExtinguisherMode()
    {
        IsInExtinguisherMode = true;
        if (_hasIsExtMode && animator) animator.SetBool(isExtModeBool, true);
        Debug.Log("[PlayerController] EnterExtinguisherMode()");
    }

    // Helper가 호출하는 시그니처 (destroyExtinguisher 옵션)
    public void ExitExtinguisherMode(bool destroyExtinguisher = true)
    {
        if (!IsInExtinguisherMode && !EquippedExtinguisher)
        {
            // 이미 꺼져 있어도 정리는 진행
        }

        // 분사 정지
        if (_spraying && extinguisher) extinguisher.StopSpraying();
        _spraying = false;

        // 애니 상태
        if (_hasIsExtMode && animator) animator.SetBool(isExtModeBool, false);
        if (_hasSpray && animator) animator.SetBool(sprayBool, false);

        // 파괴 옵션 처리
        if (destroyExtinguisher && EquippedExtinguisher)
        {
            var go = EquippedExtinguisher.gameObject;
            EquippedExtinguisher = null;
            if (go) Destroy(go);
            extinguisher = null;
        }

        // 해제만 하고 보존하는 경우
        if (!destroyExtinguisher)
        {
            EquippedExtinguisher = null;
            extinguisher = null;
        }

        IsInExtinguisherMode = false;
        Debug.Log("[PlayerController] ExitExtinguisherMode()");
    }
}
