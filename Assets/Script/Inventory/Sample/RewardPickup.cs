using UnityEngine;
using Game.Inventory;


public class GiveRewardOnInteract : MonoBehaviour, IInteractable
{
    [Header("Reward")]
    public ItemDefinition rewardItem; // 예: HumanCarry(65kg), Extinguisher(7kg)
    [Min(1)] public int rewardCount = 1;
    public bool oneTime = true;
    public bool disableAfter = false;


    [Header("Highlight (optional)")]
    public Renderer[] highlightRenderers;
    public Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f);


    bool _used;
    static readonly int _Color = Shader.PropertyToID("_Color");


    public Transform GetTransform() => transform;


    public void SetHighlighted(bool on)
    {
        if (highlightRenderers == null || highlightRenderers.Length == 0)
            highlightRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in highlightRenderers)
        {
            if (!r) continue;
            try { var m = r.material; if (m.HasProperty(_Color)) m.SetColor(_Color, on ? highlightColor : Color.white); } catch { }
        }
    }


    public void OnInteract(GameObject interactor)
    {
        if (_used && oneTime) return;
        if (!rewardItem)
        {
            Debug.LogWarning($"[GiveRewardOnInteract] rewardItem not set on {name}");
            return;
        }
        var inv = interactor.GetComponent<InventoryComponent>();
        if (!inv)
        {
            Debug.LogWarning($"[GiveRewardOnInteract] InventoryComponent missing on {interactor.name}");
            return;
        }


        inv.Add(rewardItem, rewardCount);
        _used = true;
        SetHighlighted(false);
        Debug.Log($"[Reward] {rewardItem.DisplayName} x{rewardCount} 지급 (무게 +{rewardItem.UnitWeightKg * rewardCount}kg)");


        if (disableAfter)
        {
            foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = false;
            gameObject.SetActive(false);
        }
    }
}