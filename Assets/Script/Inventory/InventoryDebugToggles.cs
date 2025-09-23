using UnityEngine;


namespace Game.Inventory
{
    public class InventoryDebugToggles : MonoBehaviour
    {
        public InventoryComponent inventory;
        [Header("Test Items (assign in Inspector)")]
        public ItemDefinition Extinguisher;
        public ItemDefinition HumanCarry;


        void Awake()
        {
            if (!inventory) inventory = FindObjectOfType<InventoryComponent>();
        }


        void Update()
        {
            if (!inventory) return;


            if (Input.GetKeyDown(KeyCode.F1) && Extinguisher)
            {
                // Toggle extinguisher (MaxStack=1 recommended)
                bool removed = inventory.Remove(Extinguisher, 1);
                if (!removed) inventory.Add(Extinguisher, 1);
            }
            if (Input.GetKeyDown(KeyCode.F2) && HumanCarry)
            {
                bool removed = inventory.Remove(HumanCarry, 1);
                if (!removed) inventory.Add(HumanCarry, 1);
            }
        }
    }
}