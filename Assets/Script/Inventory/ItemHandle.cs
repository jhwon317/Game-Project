using System;
using UnityEngine;


namespace Game.Inventory
{
    [Serializable]
    public class ItemHandle
    {
        public ItemDefinition def;
        [Min(1)] public int count = 1;


        public ItemHandle(ItemDefinition def, int count = 1)
        {
            this.def = def;
            this.count = Mathf.Max(1, count);
        }


        public float TotalWeightKg => (def ? def.UnitWeightKg : 0f) * count;
    }
}