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

    private PlayerInteractor interactor; // Interactor를 기억할 변수

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interactor = GetComponent<PlayerInteractor>(); // 시작할 때 Interactor를 찾아둠
    }

    void Update()
    {
        CheckGround();
        Move();
        Jump();

        // [핵심!] E키에 대한 모든 결정은 여기서만 한다!
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 1. 내가 뭔가를 들고 있을 때 -> 던진다
            if (heldObject != null)
            {
                ThrowObject();
            }
            // 2. 내 손이 비어있고, Interactor가 뭔가를 보고 있을 때 -> 집는다
            else if (interactor.currentInteractable != null)
            {
                interactor.currentInteractable.OnInteract(gameObject);
            }
        }
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector3(moveX * moveSpeed, rb.linearVelocity.y, 0);

        // moveX가 0이 아닐 때만, 즉 움직일 때만 캐릭터 방향 전환
        if (moveX != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveX), 1, 1);
        }
    }

    void Jump()
    {
        // 땅에 있을 때만, 그리고 Jump 버튼(스페이스바)을 눌렀을 때만
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            // 위쪽으로 힘을 가해서 점프!
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void CheckGround()
    {
        // 캐릭터 발밑으로 짧은 레이저를 쏴서 땅에 닿았는지 확인
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    // OnInteract에서 호출될 '들기' 전용 함수
    public void PickUpObject(ThrowableBox box)
    {
        heldObject = box;
        heldObject.BePickedUp(holdPoint);
    }

    // '던지기' 전용 함수
    private void ThrowObject()
    {
        Vector3 throwDirection = transform.right * transform.localScale.x;
        heldObject.BeThrown(throwDirection * throwForce);
        heldObject = null;
    }

} // <-- PlayerController 클래스는 여기서 끝! (여분의 괄호 제거)