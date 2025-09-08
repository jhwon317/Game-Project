using UnityEngine;
using System.Threading.Tasks;
using PopupMini;

[RequireComponent(typeof(Collider))]
public class InteractablePuzzleRequest : MonoBehaviour, IInteractable
{
    public InteractionRouter router;

    [Header("Use either one")]
    public PuzzleDefinition definition;   // ��ȣ: SO
    public GameObject prefabFallback;     // ���: SO ���� �����ո�

    [TextArea] public string jsonArgs;
    public bool preventReentry = true;

    bool _busy;

    void Awake()
    {
        if (!router)
#if UNITY_2023_1_OR_NEWER
            router = FindFirstObjectByType<InteractionRouter>();
#else
            router = FindObjectOfType<InteractionRouter>();
#endif
    }

    public Transform GetTransform() => transform;
    public void SetHighlighted(bool on) { }

    public async void OnInteract(GameObject interactor)
    {
        if (_busy && preventReentry) return;
        if (!router) { Debug.LogWarning("[InteractablePuzzleRequest] router ������"); return; }

        var def = definition ? definition : (prefabFallback ? MakeRuntimeDefinition(prefabFallback) : null);
        if (!def) { Debug.LogWarning("[InteractablePuzzleRequest] definition/prefab ������"); return; }

        _busy = true;
        try
        {
            var req = new PuzzleRequest { Definition = def, Args = string.IsNullOrEmpty(jsonArgs) ? null : jsonArgs };
            var r = await router.RequestOpen(req);
            Debug.Log($"[InteractablePuzzleRequest] id={(string.IsNullOrEmpty(def.Id) ? def.Prefab?.name : def.Id)} success={r.Success} reason={r.Reason}");
        }
        finally { _busy = false; }
    }

    PuzzleDefinition MakeRuntimeDefinition(GameObject prefab)
    {
        var tmp = ScriptableObject.CreateInstance<PuzzleDefinition>();
        tmp.Id = prefab.name; tmp.Prefab = prefab;
        tmp.AspectMode = AspectMode.FillCrop; tmp.Aspect = 1f;
        tmp.Modal = true; tmp.BackdropClosable = false; tmp.TimeoutSec = 0f;
        tmp.AntiAliasing = 1; tmp.FilterMode = FilterMode.Point; tmp.BackgroundColor = new Color(0, 0, 0, 0);
        return tmp;
    }
}
