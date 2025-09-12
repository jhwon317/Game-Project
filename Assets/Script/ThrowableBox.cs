using UnityEngine;

public class ThrowableBox : MonoBehaviour, IInteractable
{
    private Rigidbody rb;
    private Collider boxCollider;
    private Transform originalParent;
    private Vector3 originalScale; // [���� �߰�] ���� ũ�⸦ ����� ����

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<Collider>();
        originalParent = transform.parent;
        originalScale = transform.localScale; // [���� �߰�] ������ �� �� ���� ũ�⸦ ���
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
        transform.localRotation = Quaternion.identity; // ȸ������ �ʱ�ȭ
        transform.localScale = originalScale; // [�ٽ� ����!] ũ�⸦ ������� ���� ����!
    }

    public void BeThrown(Vector3 throwForce)
    {
        transform.SetParent(originalParent);
        transform.localScale = originalScale; // [�ٽ� ����!] ���� ���� ũ�⸦ �������!
        rb.isKinematic = false;
        boxCollider.enabled = true;
        rb.AddForce(throwForce, ForceMode.Impulse);
    }

    public Transform GetTransform() { return transform; }
    public void SetHighlighted(bool on) { /* ���̶���Ʈ ���� */ }
}