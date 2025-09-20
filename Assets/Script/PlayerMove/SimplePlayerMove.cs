using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerMove : MonoBehaviour
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

    [Header("Animation Params")]
    public Animator animator;
    public string isMovingBool = "IsMoving"; // Idle��Locomotion ����ġ
    public string speedInt = "Speed";        // 0=Idle, 1=Walk, 2=Run

    CharacterController _cc;
    Vector3 _vel; // y �߷¿�
    bool _sprintOn; // ��� ���� ����

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (animator) animator.applyRootMotion = false; // CC ��� �̵� ����
    }

    void Update()
    {
        // --- �Է� �б� ---
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

        // �̵� �Է� ������ ������Ʈ ȿ�� ����
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
}
