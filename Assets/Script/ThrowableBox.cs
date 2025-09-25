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

    // [�ٽ� ����!] ���� Ư�� ��ũ��Ʈ �̸� ���, "PickUpObject"��� '��ɾ�'�� �����
    public void OnInteract(GameObject interactor)
    {
        // interactor(�÷��̾�)���� "PickUpObject"��� �޽����� ������,
        // 'this'(�� �ڽ�, �� �� ThrowableBox)�� �����ͷ� �Բ� ����
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
    public void SetHighlighted(bool on) { /* ���̶���Ʈ ���� */ }
}

