using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractablePopupSpawnAndBind : MonoBehaviour, IInteractable
{
    [Header("Scene-shared")]
    public PopupHost host;              // Panel/Content/Viewport ���� (�Ʒ� ��)
    public CamToRawImage camToImage;    // Panel/Content/Viewport(RawImage)�� ���� ���δ�

    [Header("Prefab to spawn (choose one)")]
    public GameObject prefabRef;        // ���� ����
    public string resourcesKey;         // �Ǵ� Resources Ű (��: "MiniWorlds/MW_A")

    [Header("Behavior")]
    public bool preventReentry = true;

    GameObject _instance;
    Camera _puzzleCam;
    bool _busy;

    public Transform GetTransform() => transform;
    public void SetHighlighted(bool on) { /* ���� */ }

    public void OnInteract(GameObject interactor)
    {
        if (_busy && preventReentry) return;
        _busy = true;

        if (!host || !camToImage)
        {
            Debug.LogError("[SpawnAndBind] host/camToImage ������");
            _busy = false; return;
        }

        host.Show(); // �˾� ON

        // 1) ������ �ε�/����
        var prefab = prefabRef ? prefabRef : (!string.IsNullOrEmpty(resourcesKey) ? Resources.Load<GameObject>(resourcesKey) : null);
        if (!prefab) { Debug.LogError("[SpawnAndBind] ������ ����"); host.Hide(); _busy = false; return; }

        _instance = Instantiate(prefab);

        // 2) PuzzleCam ã�� (�̸� �켱, ������ ù ī�޶�)
        _puzzleCam = FindPuzzleCam(_instance);
        if (!_puzzleCam) { Debug.LogError("[SpawnAndBind] PuzzleCam ����"); Cleanup(); return; }

        // 3) ���� ���� & ���ε�
        SanitizeCamera(_puzzleCam);
        camToImage.Bind(_puzzleCam); // �� ���� �Ҵ� (RawImage�� ��� ǥ��)

        _busy = false;
    }

    Camera FindPuzzleCam(GameObject root)
    {
        var cams = root.GetComponentsInChildren<Camera>(true);
        foreach (var c in cams) if (c.name == "PuzzleCam") return c;
        return cams.Length > 0 ? cams[0] : null;
    }

    void SanitizeCamera(Camera cam)
    {
        if (cam.CompareTag("MainCamera")) cam.tag = "Untagged";
        var al = cam.GetComponent<AudioListener>(); if (al) Destroy(al);
#if UNITY_RENDER_PIPELINE_UNIVERSAL
        var urp = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (urp) { urp.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Base; urp.cameraStack.Clear(); }
#endif
        cam.clearFlags = CameraClearFlags.SolidColor;
        var c = cam.backgroundColor; c.a = 0f; cam.backgroundColor = c; // ���� ���
        cam.depth = -100; // Ȥ�� ȭ�� ���� ���� ����
    }

    public void Close() => Cleanup();

    void Cleanup()
    {
        if (camToImage) camToImage.Unbind();
        if (host) host.Hide();
        if (_instance) Destroy(_instance);
        _instance = null; _puzzleCam = null; _busy = false;
    }

    void OnDisable() { if (_instance) Cleanup(); }
}
