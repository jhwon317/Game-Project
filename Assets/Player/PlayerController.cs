using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 인스펙터 창에서 조절할 수 있는 변수들
    public float moveSpeed = 7f;    // 이동 속도
    public float jumpForce = 10f;   // 점프 힘

    private Rigidbody rb;
    private bool isGrounded;

    // 게임 시작 시 딱 한 번 호출됨
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 매 프레임마다 호출됨
    void Update()
    {
        // 바닥에 있는지 체크
        CheckGround();

        // 키보드 입력 받기
        float moveX = Input.GetAxis("Horizontal"); // A, D 또는 화살표 좌우
        float moveZ = Input.GetAxis("Vertical");   // W, S 또는 화살표 위아래

        // 이동 방향 계산
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        // 계산된 방향으로 캐릭터 이동시키기
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);

        // 점프! (바닥에 있을 때만 가능)
        if (isGrounded && Input.GetButtonDown("Jump")) // Jump는 스페이스바
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void CheckGround()
    {
        // 캐릭터 발밑 1.1m까지 아래로 광선을 쏴서 땅이 있는지 확인
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
