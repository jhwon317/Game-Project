using System.Collections.Generic;
using UnityEngine;
using Game.Inventory;


public static class RewardGrantHelper
{
    /// <summary>
    /// Parse payload ¡æ resolve ItemDefinition via ItemCatalog ¡æ grant to target inventory.
    /// </summary>
    /// <returns>number of items granted (sum of counts)</returns>
    public static int TryGrant(string payload, GameObject interactor,
    InventoryComponent inventoryOverride = null,
    bool oneTime = false, System.Action afterConsume = null,
    string logPrefix = "[Reward]")
    {
        if (string.IsNullOrWhiteSpace(payload)) return 0;
        if (!PopupMini.RewardPayload.TryParse(payload, out List<PopupMini.ItemReward> list))
        {
            Debug.LogWarning($"{logPrefix} Payload parse failed.");
            return 0;
        }


        // find inventory target
        var inv = inventoryOverride
        ?? (interactor ? interactor.GetComponent<InventoryComponent>() : null)
        ?? (interactor ? interactor.GetComponentInParent<InventoryComponent>() : null);
        if (!inv)
        {
            Debug.LogWarning($"{logPrefix} InventoryComponent not found on interactor or parents.");
            return 0;
        }


        int granted = 0;
        foreach (var r in list)
        {
            if (r == null || string.IsNullOrEmpty(r.id)) continue;
            var def = ItemCatalog.Resolve(r.id);
            if (!def)
            {
                Debug.LogWarning($"{logPrefix} Item id not found in catalog: {r.id}");
                continue;
            }
            int c = Mathf.Max(1, r.count);
            inv.Add(def, c);
            granted += c;
            Debug.Log($"{logPrefix} {def.DisplayName} x{c} (+{def.UnitWeightKg * c}kg)");
        }


        if (granted > 0 && oneTime)
        {
            afterConsume?.Invoke(); // caller can disable collider/object etc.
        }
        return granted;
    }
}