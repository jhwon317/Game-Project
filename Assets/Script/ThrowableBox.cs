using UnityEngine;

public class ThrowableBox : MonoBehaviour, IInteractable
{
    private Rigidbody rb;
    private Collider boxCollider;
    private Transform originalParent;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<Collider>();
        originalParent = transform.parent;
        originalScale = transform.localScale;
    }

    // [핵심 수정!] 이제 특정 스크립트 이름 대신, "PickUpObject"라는 '명령어'를 방송함
    public void OnInteract(GameObject interactor)
    {
        // interactor(플레이어)에게 "PickUpObject"라는 메시지를 보내고,
        // 'this'(나 자신, 즉 이 ThrowableBox)를 데이터로 함께 보냄
        interactor.SendMessage("PickUpObject", this, SendMessageOptions.DontRequireReceiver);
    }

    public void BePickedUp(Transform holder)
    {
        rb.isKinematic = true;
        boxCollider.enabled = false;
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;
    }

    public void BeThrown(Vector3 throwForce)
    {
        transform.SetParent(originalParent);
        transform.localScale = originalScale;
        rb.isKinematic = false;
        boxCollider.enabled = true;
        rb.AddForce(throwForce, ForceMode.Impulse);
    }

    public Transform GetTransform() { return transform; }
    public void SetHighlighted(bool on) { /* 하이라이트 로직 */ }
}

