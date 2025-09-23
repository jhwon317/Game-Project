using UnityEngine;


namespace Game.Inventory
{
    public class EncumbranceComponent : MonoBehaviour
    {
        [Header("Links")]
        public InventoryComponent inventory;


        [Header("Capacity (kg)")]
        [Min(0f)] public float BaseCapacityKg = 30f;


        [Header("Speed Scale Output")]
        [Range(0f, 2f)] public float SpeedScale = 1f; // read this from your mover
        public bool SprintAllowed = true; // and this before sprint logic


        float _wCached;


        void Awake()
        {
            if (!inventory) inventory = GetComponent<InventoryComponent>();
            if (inventory) inventory.OnInventoryChanged += Recompute;
            Recompute();
        }


        void OnDestroy()
        {
            if (inventory) inventory.OnInventoryChanged -= Recompute;
        }


        void Update()
        {
            // If inventory is dynamic (picked via triggers), you may poll occasionally.
            // For now, event-driven recompute is enough. Keep Update in case designers tweak BaseCapacity at runtime.
            if (!Application.isPlaying) Recompute();
        }


        public void Recompute()
        {
            float W = (inventory ? inventory.ComputeTotalWeightKg() : 0f);
            _wCached = W;
            float Cap = Mathf.Max(0.0001f, BaseCapacityKg);
            float r = Mathf.Max(0f, (W - Cap) / Cap);


            // Piecewise linear mapping
            float s;
            if (r <= 0f)
            {
                s = 1f; SprintAllowed = true;
            }
            else if (r <= 0.25f)
            {
                // 1.00 -> 0.85
                float t = r / 0.25f;
                s = Mathf.Lerp(1.0f, 0.85f, t); SprintAllowed = true;
            }
            else if (r <= 0.75f)
            {
                // 0.85 -> 0.60
                float t = (r - 0.25f) / 0.5f;
                s = Mathf.Lerp(0.85f, 0.60f, t); SprintAllowed = true;
            }
            else
            {
                // 0.60 -> 0.40 (heavy) & sprint off
                float t = Mathf.Min(1f, (r - 0.75f) / 0.75f);
                s = Mathf.Lerp(0.60f, 0.40f, t); SprintAllowed = false;
            }


            SpeedScale = Mathf.Clamp(s, 0.1f, 2f);
        }


        // Optional debug accessor
        public float GetCachedWeightKg() => _wCached;
    }
}