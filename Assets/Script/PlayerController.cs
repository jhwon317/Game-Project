using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- 이동/점프 변수 ---
    [Header("이동 & 점프 설정")]
    public float moveSpeed = 7f;
    public float jumpForce = 10f;
    private Rigidbody rb;
    private bool isGrounded;

    // --- 들기/던지기 변수 ---
    [Header("들기/던지기 설정")]
    public Transform holdPoint;
    public float throwForce = 15f;
    public ThrowableBox heldObject = null;

    private PlayerInteractor interactor;
    private float moveInput; // 키보드 입력을 저장할 변수

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interactor = GetComponent<PlayerInteractor>();
    }

    void Update()
    {
        // Update에서는 '입력'과 '점프'처럼 즉각적인 반응이 필요한 것만 처리
        CheckGround();
        Jump();

        moveInput = Input.GetAxis("Horizontal");

        // E키 입력 처리
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject != null)
            {
                ThrowObject();
            }
            else if (interactor.currentInteractable != null)
            {
                interactor.currentInteractable.OnInteract(gameObject);
            }
        }

        // [핵심 수정!] 캐릭터 방향 전환을 다시 localScale 방식으로 변경 (가장 안정적)
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
        }
    }

    // 물리적인 움직임은 모두 FixedUpdate에서 처리한다
    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        // 이번 프레임에 적용할 '최대 속도'를 계산
        float currentMaxSpeed = moveSpeed;
        if (heldObject != null)
        {
            HeavyObject heavy = heldObject.GetComponent<HeavyObject>();
            if (heavy != null)
            {
                currentMaxSpeed = moveSpeed * heavy.speedModifier;
            }
        }

        // [탐정 코드!] 실제로 적용된 최종 수평 속도를 콘솔에 보고!
        Debug.Log($"Actual Horizontal Speed: {rb.linearVelocity.x}");
        // Update에서 받아온 키보드 입력값을 사용해서 속도를 설정
        rb.linearVelocity = new Vector3(moveInput * currentMaxSpeed, rb.linearVelocity.y, 0);
    }

    void Jump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    public void PickUpObject(ThrowableBox box)
    {
        heldObject = box;
        heldObject.BePickedUp(holdPoint);
    }

    private void ThrowObject()
    {
        // [핵심 수정!] localScale 방식에 맞는 올바른 던지기 방향 계산
        Vector3 throwDirection = transform.right * transform.localScale.x;
        heldObject.BeThrown(throwDirection * throwForce);
        heldObject = null;
    }
}

