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
        Debug.Log("출구 문이 활성화되었습니다!");
    }

    public void Deactivate()
    {
        isActivated = false;
        SetHighlighted(false);
        gameObject.layer = LayerMask.NameToLayer("Default");
        Debug.Log("출구 문이 비활성화되었습니다.");
    }

    // E키를 눌렀을 때
    public void OnInteract(GameObject interactor)
    {
        if (isActivated)
        {
            // [수정] 다음 씬이 진짜 있는지 확인하는 안전장치 추가!
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log("다음 스테이지로 이동합니다. 로드할 씬 인덱스: " + nextSceneIndex);
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogError("다음 스테이지가 없습니다! 'Build Profiles'에 씬을 추가했는지 확인해주세요!");
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