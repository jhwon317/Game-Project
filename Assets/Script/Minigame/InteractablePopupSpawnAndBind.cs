using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractablePopupSpawnAndBind : MonoBehaviour, IInteractable
{
    [Header("Scene-shared")]
    public PopupHost host;              // Panel/Content/Viewport 보유 (아래 ②)
    public CamToRawImage camToImage;    // Panel/Content/Viewport(RawImage)에 붙은 바인더

    [Header("Prefab to spawn (choose one)")]
    public GameObject prefabRef;        // 직접 참조
    public string resourcesKey;         // 또는 Resources 키 (예: "MiniWorlds/MW_A")

    [Header("Behavior")]
    public bool preventReentry = true;

    GameObject _instance;
    Camera _puzzleCam;
    bool _busy;

    public Transform GetTransform() => transform;
    public void SetHighlighted(bool on) { /* 선택 */ }

    public void OnInteract(GameObject interactor)
    {
        if (_busy && preventReentry) return;
        _busy = true;

        if (!host || !camToImage)
        {
            Debug.LogError("[SpawnAndBind] host/camToImage 미지정");
            _busy = false; return;
        }

        host.Show(); // 팝업 ON

        // 1) 프리팹 로드/생성
        var prefab = prefabRef ? prefabRef : (!string.IsNullOrEmpty(resourcesKey) ? Resources.Load<GameObject>(resourcesKey) : null);
        if (!prefab) { Debug.LogError("[SpawnAndBind] 프리팹 없음"); host.Hide(); _busy = false; return; }

        _instance = Instantiate(prefab);

        // 2) PuzzleCam 찾기 (이름 우선, 없으면 첫 카메라)
        _puzzleCam = FindPuzzleCam(_instance);
        if (!_puzzleCam) { Debug.LogError("[SpawnAndBind] PuzzleCam 없음"); Cleanup(); return; }

        // 3) 안전 세팅 & 바인딩
        SanitizeCamera(_puzzleCam);
        camToImage.Bind(_puzzleCam); // ★ 동적 할당 (RawImage에 즉시 표시)

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
        var c = cam.backgroundColor; c.a = 0f; cam.backgroundColor = c; // 투명 배경
        cam.depth = -100; // 혹시 화면 렌더 개입 방지
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
