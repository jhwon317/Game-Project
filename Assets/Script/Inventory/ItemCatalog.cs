using UnityEngine;


namespace Game.Inventory
{
    [CreateAssetMenu(menuName = "Game/Inventory/Item Catalog", fileName = "ItemCatalog")]
    public class ItemCatalog : ScriptableObject
    {
        [System.Serializable] public struct Entry { public string id; public ItemDefinition def; }
        public Entry[] entries;


        public ItemDefinition Find(string key)
        {
            if (string.IsNullOrEmpty(key) || entries == null) return null;
            string k = key.Trim();
            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (!e.def) continue;
                if (string.Equals(e.id, k, System.StringComparison.OrdinalIgnoreCase)) return e.def;
                if (!string.IsNullOrEmpty(e.def.Id) && string.Equals(e.def.Id, k, System.StringComparison.OrdinalIgnoreCase)) return e.def;
                if (string.Equals(e.def.name, k, System.StringComparison.OrdinalIgnoreCase)) return e.def;
            }
            return null;
        }


        static ItemCatalog _cached;
        public static ItemDefinition Resolve(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (_cached == null)
            {
                _cached = Resources.Load<ItemCatalog>("ItemCatalog");
                if (_cached == null) Debug.LogWarning("[ItemCatalog] Resources/ItemCatalog.asset not found. Create one and fill entries.");
            }
            return _cached ? _cached.Find(key) : null;
        }
    }
}