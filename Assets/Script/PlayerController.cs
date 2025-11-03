using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- 이동/점프 변수 ---
    [Header("이동 & 점프 설정")]
    public float moveSpeed = 7f;
    public float jumpForce = 10f;
    private Rigidbody rb;
    private bool isGrounded;

    // --- 들기/던지기 변수 (ThrowableBox 전용) ---
    [Header("들기/던지기 설정")]
    public Transform holdPoint;
    public float throwForce = 15f;
    public ThrowableBox heldObject = null;

    // --- 소화기 모드 (별도 독립) ---
    [Header("소화기 모드")]
    private ExtinguisherItem _equippedExtinguisher = null;
    private bool _inExtinguisherMode = false;

    private PlayerInteractor interactor;
    private float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interactor = GetComponent<PlayerInteractor>();
    }

    void Update()
    {
        CheckGround();
        Jump();

        moveInput = Input.GetAxis("Horizontal");

        // E키 입력 처리 (ThrowableBox 전용)
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 소화기 모드가 아닐 때만 ThrowableBox 처리
            if (!_inExtinguisherMode)
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
        }

        // 소화기 분사 (소화기 모드 전용)
        if (_inExtinguisherMode && _equippedExtinguisher != null)
        {
            if (Input.GetButton("Fire2"))
            {
                if (_equippedExtinguisher.controller)
                    _equippedExtinguisher.controller.TrySpraying(Time.deltaTime);
            }
            else if (Input.GetButtonUp("Fire2"))
            {
                if (_equippedExtinguisher.controller)
                    _equippedExtinguisher.controller.StopSpraying();
            }
        }

        // 캐릭터 방향 전환
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        // 이번 프레임에 적용할 '최대 속도'를 계산
        float currentMaxSpeed = moveSpeed;

        // ThrowableBox 무게 반영
        if (heldObject != null)
        {
            HeavyObject heavy = heldObject.GetComponent<HeavyObject>();
            if (heavy != null)
            {
                currentMaxSpeed = moveSpeed * heavy.speedModifier;
            }
        }
        // 소화기 모드 무게 반영
        else if (_inExtinguisherMode && _equippedExtinguisher != null)
        {
            HeavyObject heavy = _equippedExtinguisher.GetComponent<HeavyObject>();
            if (heavy != null)
            {
                currentMaxSpeed = moveSpeed * heavy.speedModifier;
            }
        }

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

    // === ThrowableBox 전용 메서드 (원본 유지) ===
    public void PickUpObject(ThrowableBox box)
    {
        heldObject = box;
        heldObject.BePickedUp(holdPoint);
    }

    private void ThrowObject()
    {
        Vector3 throwDirection = transform.right * transform.localScale.x;
        heldObject.BeThrown(throwDirection * throwForce);
        heldObject = null;
    }

    // === 소화기 모드 전용 메서드 (헬퍼가 호출) ===
    
    /// <summary>
    /// 소화기 모드 진입 (ExtinguisherHelper가 호출)
    /// </summary>
    public bool EnterExtinguisherMode(ExtinguisherItem extinguisher)
    {
        // 이미 다른 모드 활성화 중이면 거부
        if (_inExtinguisherMode || heldObject != null)
        {
            Debug.LogWarning("[PlayerController] 이미 다른 물건을 들고 있습니다!");
            return false;
        }

        _equippedExtinguisher = extinguisher;
        _inExtinguisherMode = true;

        // 손에 부착
        if (_equippedExtinguisher != null)
        {
            _equippedExtinguisher.transform.SetParent(holdPoint);
            _equippedExtinguisher.transform.localPosition = Vector3.zero;
            _equippedExtinguisher.transform.localRotation = Quaternion.identity;

            // 컨트롤러 활성화
            if (_equippedExtinguisher.controller)
                _equippedExtinguisher.controller.enabled = true;
        }

        Debug.Log("[PlayerController] 소화기 모드 진입!");
        return true;
    }

    /// <summary>
    /// 소화기 모드 해제 (ExtinguisherHelper가 호출)
    /// </summary>
    public void ExitExtinguisherMode()
    {
        if (!_inExtinguisherMode) return;

        // 분사 중지
        if (_equippedExtinguisher != null && _equippedExtinguisher.controller)
        {
            _equippedExtinguisher.controller.StopSpraying();
            _equippedExtinguisher.controller.enabled = false;
        }

        // 소화기 제거 (파괴 또는 비활성화는 호출자가 결정)
        _equippedExtinguisher = null;
        _inExtinguisherMode = false;

        Debug.Log("[PlayerController] 소화기 모드 해제!");
    }

    /// <summary>
    /// 현재 소화기 모드인지
    /// </summary>
    public bool IsInExtinguisherMode => _inExtinguisherMode;

    /// <summary>
    /// 현재 장착된 소화기
    /// </summary>
    public ExtinguisherItem EquippedExtinguisher => _equippedExtinguisher;
}
