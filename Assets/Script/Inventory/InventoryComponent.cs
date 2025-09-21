using System;
using System.Collections.Generic;
using UnityEngine;


namespace Game.Inventory
{
    public class InventoryComponent : MonoBehaviour
    {
        [SerializeField] private List<ItemHandle> _items = new List<ItemHandle>();


        public event Action OnInventoryChanged;


        public IReadOnlyList<ItemHandle> Items => _items;


        // Add: merges by definition when stacking is allowed
        public void Add(ItemDefinition def, int count = 1)
        {
            if (!def || count <= 0) return;


            // Try merge only if MaxStack > 1
            if (def.MaxStack > 1)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var h = _items[i];
                    if (h.def == def && h.count < def.MaxStack)
                    {
                        int canAdd = Mathf.Min(count, def.MaxStack - h.count);
                        h.count += canAdd;
                        count -= canAdd;
                        if (count <= 0) { OnInventoryChanged?.Invoke(); return; }
                    }
                }
            }


            // Add remaining as new stacks (or singletons for MaxStack==1)
            while (count > 0)
            {
                int toAdd = (def.MaxStack > 1) ? Mathf.Min(count, def.MaxStack) : 1;
                _items.Add(new ItemHandle(def, toAdd));
                count -= toAdd;
            }


            OnInventoryChanged?.Invoke();
        }


        public bool Remove(ItemDefinition def, int count = 1)
        {
            if (!def || count <= 0) return false;
            for (int i = 0; i < _items.Count && count > 0; i++)
            {
                var h = _items[i];
                if (h.def != def) continue;
                int take = Mathf.Min(count, h.count);
                h.count -= take;
                count -= take;
                if (h.count <= 0) { _items.RemoveAt(i); i--; }
            }
            bool changed = count <= 0;
            if (changed) OnInventoryChanged?.Invoke();
            return changed;
        }


        public float ComputeTotalWeightKg()
        {
            float w = 0f;
            for (int i = 0; i < _items.Count; i++)
                w += _items[i].TotalWeightKg;
            return w;
        }
    }
}