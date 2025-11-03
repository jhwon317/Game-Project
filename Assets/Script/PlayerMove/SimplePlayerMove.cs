using UnityEngine;
using Game.Inventory; // for EncumbranceComponent

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerMove : MonoBehaviour
{
    [Header("Move")]
    public bool useSideView2_5D = false; // 2.5D면 좌우만
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.5f;
    public float rotateSpeed = 540f;
    public float gravity = -9.81f;

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool toggleSprint = false;    // true면 토글, false면 누르는 동안만

    [Header("Interact")]
    public KeyCode interactKey = KeyCode.E; // E키로 상호작용 (PlayerController 참고)
    private PlayerInteractor _interactor;    // 현재 바라보는 IInteractable 접근

    [Header("Animation Params")]
    public Animator animator;
    public string isMovingBool = "IsMoving"; // Idle↔Locomotion 스위치
    public string speedInt = "Speed";        // 0=Idle, 1=Walk, 2=Run

    CharacterController _cc;
    Vector3 _vel; // y 중력용
    bool _sprintOn; // 토글 상태 보관

    // Encumbrance 연동
    EncumbranceComponent _encum;

    // ===== Extinguisher =====
    [Header("Extinguisher")]
    public ExtinguisherController extinguisher;   // 분사 컨트롤러 (자식에 있어도 OK)
    public string isExtModeBool = "IsExtinguisherMode"; // 애니메이터 파라미터명 (옵셔널)
    public string sprayBool = "Spray";               // 분사 중 표시 (옵셔널)
    bool _extMode;      // 소화기 모드 on/off (핀 퍼즐 성공 시 EnterExtinguisherMode() 호출)
    bool _spraying;     // 현재 분사 유지 중

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (animator) animator.applyRootMotion = false; // CC 기반 이동 권장

        _encum = GetComponent<EncumbranceComponent>();
        _interactor = GetComponent<PlayerInteractor>(); // PlayerController의 흐름과 동일하게 E 처리
        if (!extinguisher) extinguisher = GetComponentInChildren<ExtinguisherController>(true);
    }

    void Update()
    {
        // --- 상호작용(E) ---
        if (Input.GetKeyDown(interactKey))
        {
            // PlayerController에서 하던 것처럼, 손이 비어있다는 전제에서
            // 현재 인터랙터가 가리키는 오브젝트에 OnInteract 호출
            if (_interactor != null && _interactor.currentInteractable != null)
            {
                _interactor.currentInteractable.OnInteract(gameObject);
            }
        }

        // --- 이동 입력 ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = useSideView2_5D ? 0f : Input.GetAxisRaw("Vertical");
        bool hasMoveInput = (new Vector2(h, v).sqrMagnitude > 0.01f);

        // --- 스프린트 상태 갱신 ---
        if (toggleSprint)
        {
            if (Input.GetKeyDown(sprintKey)) _sprintOn = !_sprintOn;
        }
        else
        {
            _sprintOn = Input.GetKey(sprintKey);
        }

        // 과적으로 스프린트 금지
        if (_encum && !_encum.SprintAllowed)
        {
            _sprintOn = false;
        }

        bool applySprint = hasMoveInput && _sprintOn;

        // --- 이동 방향/회전 ---
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

        // --- 속도 결정 ---
        float speedMeters = 0f;
        if (hasMoveInput) speedMeters = applySprint ? sprintSpeed : walkSpeed;

        // Encumbrance 속도 계수 적용
        if (_encum) speedMeters *= _encum.SpeedScale;

        // --- 중력 & 이동 ---
        if (_cc.isGrounded && _vel.y < 0f) _vel.y = -2f;
        _vel.y += gravity * Time.deltaTime;

        Vector3 horizontal = moveDir * speedMeters;
        Vector3 motion = horizontal * Time.deltaTime + _vel * Time.deltaTime;
        _cc.Move(motion);

        // --- Extinguisher: 분사 처리 ---
        if (_extMode && extinguisher)
        {
            bool pressed = false, released = false;

#if ENABLE_INPUT_SYSTEM
            // 새 입력 시스템(우클릭/패드 RT) + 레거시 입력(동시에 지원)
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                pressed |= mouse.rightButton.isPressed;
                released |= mouse.rightButton.wasReleasedThisFrame;
            }
            var pad = UnityEngine.InputSystem.Gamepad.current;
            if (pad != null)
            {
                pressed |= pad.rightTrigger.ReadValue() > 0.3f;
                released |= pad.rightTrigger.wasReleasedThisFrame;
            }
#endif
            // 레거시 입력도 함께 허용
            pressed |= Input.GetMouseButton(1) || Input.GetButton("Fire2");
            released |= Input.GetMouseButtonUp(1) || Input.GetButtonUp("Fire2");

            if (pressed)
            {
                extinguisher.TrySpraying(Time.deltaTime);
                if (!_spraying)
                {
                    _spraying = true;
                    if (animator && !string.IsNullOrEmpty(sprayBool)) animator.SetBool(sprayBool, true);
                }
            }
            else if (released)
            {
                extinguisher.StopSpraying();
                if (_spraying)
                {
                    _spraying = false;
                    if (animator && !string.IsNullOrEmpty(sprayBool)) animator.SetBool(sprayBool, false);
                }
            }
            else if (_spraying)
            {
                // 구현이 매 프레임 호출을 요구하면 유지
                extinguisher.TrySpraying(Time.deltaTime);
            }
        }

        // --- 애니메이션 파라미터 ---
        if (animator)
        {
            animator.SetBool(isMovingBool, hasMoveInput);          // 입력 유무
            int speedState = 0;                                    // 0=Idle
            if (hasMoveInput) speedState = applySprint ? 2 : 1;    // 1=Walk, 2=Run
            animator.SetInteger(speedInt, speedState);
            if (!string.IsNullOrEmpty(isExtModeBool)) animator.SetBool(isExtModeBool, _extMode);
        }
    }

    // ===== Extinguisher public API =====
    public void EnterExtinguisherMode()
    {
        _extMode = true;
        if (animator && !string.IsNullOrEmpty(isExtModeBool)) animator.SetBool(isExtModeBool, true);
        Debug.Log("Entered Extinguisher Mode");
        // 이동/회전/스프린트는 기존 로직 그대로 유지
    }

    public void ExitExtinguisherMode()
    {
        _extMode = false;
        if (_spraying && extinguisher) extinguisher.StopSpraying();
        _spraying = false;
        if (animator && !string.IsNullOrEmpty(isExtModeBool)) animator.SetBool(isExtModeBool, false);
        if (animator && !string.IsNullOrEmpty(sprayBool)) animator.SetBool(sprayBool, false);
    }
}
