using UnityEngine.SceneManagement;
using UnityEngine;

public class ExitDoor : MonoBehaviour, IInteractable
{
    private bool isActivated = false;
    public Material defaultMaterial;
    public Material highlightMaterial;

    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public void Activate()
    {
        isActivated = true;
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        Debug.Log("�ⱸ ���� Ȱ��ȭ�Ǿ����ϴ�!");
    }

    public void Deactivate()
    {
        isActivated = false;
        SetHighlighted(false);
        gameObject.layer = LayerMask.NameToLayer("Default");
        Debug.Log("�ⱸ ���� ��Ȱ��ȭ�Ǿ����ϴ�.");
    }

    // EŰ�� ������ ��
    public void OnInteract(GameObject interactor)
    {
        if (isActivated)
        {
            // [����] ���� ���� ��¥ �ִ��� Ȯ���ϴ� ������ġ �߰�!
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log("���� ���������� �̵��մϴ�. �ε��� �� �ε���: " + nextSceneIndex);
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogError("���� ���������� �����ϴ�! 'Build Profiles'�� ���� �߰��ߴ��� Ȯ�����ּ���!");
            }
        }
    }

    public void SetHighlighted(bool on)
    {
        if (isActivated && meshRenderer != null)
        {
            meshRenderer.material = on ? highlightMaterial : defaultMaterial;
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}