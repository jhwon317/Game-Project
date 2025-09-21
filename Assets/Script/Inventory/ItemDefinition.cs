using UnityEngine;

namespace Game.Inventory
{
    [CreateAssetMenu(menuName = "Game/Inventory/Item Definition", fileName = "ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public string DisplayName;


        [Header("Gameplay")]
        [Min(0f)] public float UnitWeightKg = 0f; // e.g., Extinguisher 7, HumanCarry 65
        [Min(1)] public int MaxStack = 1; // keep 1 for most items in this project


        [Header("Editor/UX (optional)")]
        public Sprite Icon;
        [TextArea] public string Tooltip;
    }
}