using UnityEngine;

/// <summary>
/// 던질 수 있는 상자. 들고 옮기다가 던지는 용도.
/// </summary>
public class ThrowableBox : MonoBehaviour, IInteractable, IHoldable
{
    [Header("Physics")]
    public Rigidbody rb;
    public Collider boxCollider;

    [Header("Speed Modifier")]
    [Tooltip("이 물건을 들었을 때 이동속도 배율 (1.0 = 변화없음)")]
    [Range(0.1f, 1f)] public float speedModifier = 1f;

    private Transform _originalParent;
    private Vector3 _originalScale;
    private bool _isHeld = false;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!boxCollider) boxCollider = GetComponent<Collider>();
        _originalParent = transform.parent;
        _originalScale = transform.localScale;
    }

    #region IInteractable
    public Transform GetTransform() => transform;
    public void SetHighlighted(bool on) { /* 하이라이트 구현 */ }

    public void OnInteract(GameObject interactor)
    {
        // PlayerController의 PickUpObject 호출
        interactor.SendMessage("PickUpObject", this, SendMessageOptions.DontRequireReceiver);
    }
    #endregion

    #region IHoldable
    public float GetSpeedModifier()
    {
        // HeavyObject가 있으면 그걸 우선 사용
        var heavy = GetComponent<HeavyObject>();
        if (heavy) return heavy.speedModifier;
        return speedModifier;
    }

    public void OnPickedUp(Transform holdPoint)
    {
        _isHeld = true;
        rb.isKinematic = true;
        boxCollider.enabled = false;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = _originalScale;
    }

    public void OnPutDown(Vector3 dropPosition, Vector3 playerForward)
    {
        _isHeld = false;
        transform.SetParent(_originalParent);
        transform.localScale = _originalScale;

        rb.isKinematic = false;
        boxCollider.enabled = true;

        // 던지기 (playerForward 방향으로 힘 가해짐)
        // dropPosition은 사용 안함 (물리로 자연스럽게 떨어짐)
        rb.AddForce(playerForward, ForceMode.Impulse);
    }
    #endregion

    #region Legacy Support (기존 코드 호환)
    public void BePickedUp(Transform holder) => OnPickedUp(holder);
    public void BeThrown(Vector3 throwForce)
    {
        _isHeld = false;
        transform.SetParent(_originalParent);
        transform.localScale = _originalScale;
        rb.isKinematic = false;
        boxCollider.enabled = true;
        rb.AddForce(throwForce, ForceMode.Impulse);
    }
    #endregion

    public bool IsHeld => _isHeld;
}
