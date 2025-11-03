using UnityEngine;
using Game.Inventory; // EncumbranceComponent

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Move (Simple style)")]
    public bool useSideView2_5D = false;
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.5f;
    public float rotateSpeed = 540f;
    public float gravity = -9.81f;

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool toggleSprint = false;

    [Header("Interact")]
    public KeyCode interactKey = KeyCode.E;
    private PlayerInteractor _interactor;

    [Header("Animation")]
    public Animator animator;
    public string isMovingBool = "IsMoving";
    public string speedInt = "Speed";
    public string isExtModeBool = "IsExtinguisherMode";
    public string sprayBool = "Spray";

    [Header("Extinguisher")]
    public ExtinguisherController extinguisher;       // 플레이어 자식에 하나 존재
    public ExtinguisherItem EquippedExtinguisher { get; private set; }
    public GameObject heldObject => EquippedExtinguisher ? EquippedExtinguisher.gameObject : null;
    public bool IsInExtinguisherMode { get; private set; }

    // internal
    CharacterController _cc;
    EncumbranceComponent _encum;
    Vector3 _vel;
    bool _sprintOn, _spraying;
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
        // Interact
        if (Input.GetKeyDown(interactKey))
        {
            if (_interactor != null && _interactor.currentInteractable != null)
                _interactor.currentInteractable.OnInteract(gameObject);
        }

        // Move input
        float h = Input.GetAxisRaw("Horizontal");
        float v = useSideView2_5D ? 0f : Input.GetAxisRaw("Vertical");
        bool hasMoveInput = (new Vector2(h, v).sqrMagnitude > 0.01f);

        // Sprint
        if (toggleSprint) { if (Input.GetKeyDown(sprintKey)) _sprintOn = !_sprintOn; }
        else _sprintOn = Input.GetKey(sprintKey);
        if (_encum && !_encum.SprintAllowed) _sprintOn = false;
        bool applySprint = hasMoveInput && _sprintOn;

        // Direction & rotate
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

        // Speed
        float speedMeters = 0f;
        if (hasMoveInput) speedMeters = applySprint ? sprintSpeed : walkSpeed;
        if (_encum) speedMeters *= _encum.SpeedScale;

        // Gravity & move
        if (_cc.isGrounded && _vel.y < 0f) _vel.y = -2f;
        _vel.y += gravity * Time.deltaTime;

        Vector3 horizontal = moveDir * speedMeters;
        Vector3 motion = horizontal * Time.deltaTime + _vel * Time.deltaTime;
        _cc.Move(motion);

        // Extinguisher
        HandleExtinguisher();

        // Anim
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

    // ===== Extinguisher helpers =====
    void EnsureExtinguisherBound()
    {
        if (!extinguisher) extinguisher = GetComponentInChildren<ExtinguisherController>(true);
        if (!extinguisher) { Debug.LogError("[PC] No ExtinguisherController under player"); return; }

        if (!extinguisher.emitter)
            extinguisher.emitter = extinguisher.GetComponentInChildren<SprayEmitter>(true);

        // ExtinguisherController.player(Transform) 필드가 있으면 채움
        var fld = typeof(ExtinguisherController).GetField("player");
        if (fld != null) fld.SetValue(extinguisher, this.transform);
    }

    void HandleExtinguisher()
    {
        if (IsInExtinguisherMode && extinguisher == null) EnsureExtinguisherBound();
        if (!IsInExtinguisherMode || extinguisher == null) return;

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

    // ===== Mode API (Helper/UI 호환) =====
    public bool EnterExtinguisherMode(ExtinguisherItem item)
    {
        if (item && item.controller) extinguisher = item.controller;
        EquippedExtinguisher = item;
        IsInExtinguisherMode = true;
        EnsureExtinguisherBound();
        if (_hasIsExtMode && animator) animator.SetBool(isExtModeBool, true);
        return extinguisher != null;
    }

    public void EnterExtinguisherMode()
    {
        IsInExtinguisherMode = true;
        EnsureExtinguisherBound();
        if (_hasIsExtMode && animator) animator.SetBool(isExtModeBool, true);
    }

    public void ExitExtinguisherMode(bool destroyExtinguisher = false)
    {
        if (_spraying && extinguisher) extinguisher.StopSpraying();
        _spraying = false;

        if (_hasIsExtMode && animator) animator.SetBool(isExtModeBool, false);
        if (_hasSpray && animator) animator.SetBool(sprayBool, false);

        if (destroyExtinguisher && EquippedExtinguisher)
        {
            var go = EquippedExtinguisher.gameObject;
            EquippedExtinguisher = null;
            if (go) Destroy(go);
            extinguisher = null;
        }
        else
        {
            EquippedExtinguisher = null;
        }

        IsInExtinguisherMode = false;
    }
}
