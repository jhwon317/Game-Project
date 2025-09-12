using UnityEngine;

public class ThrowableBox : MonoBehaviour, IInteractable
{
    private Rigidbody rb;
    private Collider boxCollider;
    private Transform originalParent;
    private Vector3 originalScale; // [새로 추가] 원래 크기를 기억할 변수

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<Collider>();
        originalParent = transform.parent;
        originalScale = transform.localScale; // [새로 추가] 시작할 때 내 원래 크기를 기억
    }

    public void OnInteract(GameObject interactor)
    {
        interactor.GetComponent<PlayerController>().PickUpObject(this);
    }

    public void BePickedUp(Transform holder)
    {
        rb.isKinematic = true;
        boxCollider.enabled = false;
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity; // 회전값도 초기화
        transform.localScale = originalScale; // [핵심 수정!] 크기를 원래대로 강제 고정!
    }

    public void BeThrown(Vector3 throwForce)
    {
        transform.SetParent(originalParent);
        transform.localScale = originalScale; // [핵심 수정!] 던질 때도 크기를 원래대로!
        rb.isKinematic = false;
        boxCollider.enabled = true;
        rb.AddForce(throwForce, ForceMode.Impulse);
    }

    public Transform GetTransform() { return transform; }
    public void SetHighlighted(bool on) { /* 하이라이트 로직 */ }
}