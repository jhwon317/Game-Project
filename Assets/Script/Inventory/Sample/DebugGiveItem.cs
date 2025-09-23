using UnityEngine;
using Game.Inventory; // InventoryComponent, ItemDefinition


public class DebugGiveItem : MonoBehaviour
{
    [Header("Refs")]
    public InventoryComponent inventory; // auto-find if null
    public EncumbranceComponent encumbrance; // optional; for logging speed scale


    [Header("Item Source")]
    public ItemDefinition itemDefinition; // if left null, a temp definition will be created at runtime
    public bool createTempDefinitionIfNull = true;


    [Header("Temp Definition (used only if above is null)")]
    public string tempId = "TEMP_DEBUG";
    public string tempDisplayName = "юс╫цеш";
    [Min(0)] public float tempUnitWeightKg = 65f; // try 7 for extinguisher, 65 for human carry
    [Min(1)] public int tempMaxStack = 1;


    [Header("Controls")]
    public KeyCode addKey = KeyCode.F5; // add N
    public KeyCode removeKey = KeyCode.F6; // remove N
    [Min(1)] public int amount = 1;


    void Awake()
    {
        if (!inventory) inventory = FindObjectOfType<InventoryComponent>();
        if (!encumbrance && inventory) encumbrance = inventory.GetComponent<EncumbranceComponent>();


        if (!itemDefinition && createTempDefinitionIfNull)
        {
            itemDefinition = ScriptableObject.CreateInstance<ItemDefinition>();
            itemDefinition.Id = tempId;
            itemDefinition.DisplayName = tempDisplayName;
            itemDefinition.UnitWeightKg = tempUnitWeightKg;
            itemDefinition.MaxStack = tempMaxStack;
        }
    }


    void Update()
    {
        if (!inventory || !itemDefinition) return;


        if (Input.GetKeyDown(addKey))
        {
            inventory.Add(itemDefinition, amount);
            LogState($"+{amount} {itemDefinition.DisplayName}");
        }
        if (Input.GetKeyDown(removeKey))
        {
            bool ok = inventory.Remove(itemDefinition, amount);
            LogState(ok ? $"-{amount} {itemDefinition.DisplayName}" : "(remove failed)");
        }
    }


    void LogState(string action)
    {
        float w = inventory.ComputeTotalWeightKg();
        if (encumbrance)
        {
            encumbrance.Recompute();
            Debug.Log($"[DebugGiveItem] {action} | Weight {w:0.##}kg | speedScale {encumbrance.SpeedScale:0.00} | sprint {(encumbrance.SprintAllowed ? "ON" : "OFF")} ");
        }
        else
        {
            Debug.Log($"[DebugGiveItem] {action} | Weight {w:0.##}kg");
        }
    }
}