using UnityEngine;
using Game.Inventory; // for EncumbranceComponent

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerMoveCopy : MonoBehaviour
{
    [Header("Move")]
    public bool useSideView2_5D = false; // 2.5D�� �¿츸
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.5f;
    public float rotateSpeed = 540f;
    public float gravity = -9.81f;

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool toggleSprint = false;    // true�� ���, false�� ������ ���ȸ�

    // --- ���/������ ���� ---
    [Header("���/������ ����")]
    public Transform holdPoint;
    public float throwForce = 15f;
    public ThrowableBox heldObject = null;

    [Header("Interact")]
    public KeyCode interactKey = KeyCode.E; // EŰ�� ��ȣ�ۿ� (PlayerController ����)
    private PlayerInteractor _interactor;    // ���� �ٶ󺸴� IInteractable ����

    [Header("Animation Params")]
    public Animator animator;
    public string isMovingBool = "IsMoving"; // Idle��Locomotion ����ġ
    public string speedInt = "Speed";        // 0=Idle, 1=Walk, 2=Run

    CharacterController _cc;
    Vector3 _vel; // y �߷¿�
    bool _sprintOn; // ��� ���� ����

    // Encumbrance ����
    EncumbranceComponent _encum;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (animator) animator.applyRootMotion = false; // CC ��� �̵� ����

        _encum = GetComponent<EncumbranceComponent>();
        _interactor = GetComponent<PlayerInteractor>(); // PlayerController�� �帧�� �����ϰ� E ó��
    }

    void Update()
    {
        // --- ��ȣ�ۿ�(E) ---
        // EŰ �Է� ó��
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject != null)
            {
                ThrowObject();
            }
            else if (_interactor.currentInteractable != null)
            {
                _interactor.currentInteractable.OnInteract(gameObject);
            }
        }

        // --- �̵� �Է� ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = useSideView2_5D ? 0f : Input.GetAxisRaw("Vertical");
        bool hasMoveInput = (new Vector2(h, v).sqrMagnitude > 0.01f);

        // --- ������Ʈ ���� ���� ---
        if (toggleSprint)
        {
            if (Input.GetKeyDown(sprintKey)) _sprintOn = !_sprintOn;
        }
        else
        {
            _sprintOn = Input.GetKey(sprintKey);
        }

        // �������� ������Ʈ ����
        if (_encum && !_encum.SprintAllowed)
        {
            _sprintOn = false;
        }

        bool applySprint = hasMoveInput && _sprintOn;

        // --- �̵� ����/ȸ�� ---
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

        // --- �ӵ� ���� ---
        float speedMeters = 0f;
        if (hasMoveInput) speedMeters = applySprint ? sprintSpeed : walkSpeed;

        // Encumbrance �ӵ� ��� ����
        if (_encum) speedMeters *= _encum.SpeedScale;

        // --- �߷� & �̵� ---
        if (_cc.isGrounded && _vel.y < 0f) _vel.y = -2f;
        _vel.y += gravity * Time.deltaTime;

        Vector3 horizontal = moveDir * speedMeters;
        Vector3 motion = horizontal * Time.deltaTime + _vel * Time.deltaTime;
        _cc.Move(motion);

        // --- �ִϸ��̼� �Ķ���� ---
        if (animator)
        {
            animator.SetBool(isMovingBool, hasMoveInput);          // �Է� ����
            int speedState = 0;                                    // 0=Idle
            if (hasMoveInput) speedState = applySprint ? 2 : 1;    // 1=Walk, 2=Run
            animator.SetInteger(speedInt, speedState);
        }
    }

    public void PickUpObject(ThrowableBox box)
    {
        heldObject = box;
        heldObject.BePickedUp(holdPoint);
    }

    private void ThrowObject()
    {
        // [�ٽ� ����!] ĳ���Ͱ� �ٶ󺸴� ���� ����(Z�� ����)���� ����
        Vector3 throwDirection = transform.forward;

        heldObject.BeThrown(throwDirection * throwForce);
        heldObject = null;
    }
}