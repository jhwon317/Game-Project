using UnityEngine;


namespace Game.Inventory
{
    public class InventoryDebugHUD : MonoBehaviour
    {
        public InventoryComponent inventory;
        public EncumbranceComponent encum;
        public float uiScale = 1f;


        void Awake()
        {
            if (!inventory) inventory = FindObjectOfType<InventoryComponent>();
            if (!encum) encum = inventory ? inventory.GetComponent<EncumbranceComponent>() : null;
        }


        void OnGUI()
        {
            if (!inventory || !encum) return;
            var weight = inventory.ComputeTotalWeightKg();
            var cap = encum.BaseCapacityKg;
            float r = Mathf.Max(0f, (weight - cap) / Mathf.Max(0.0001f, cap));


            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = Mathf.RoundToInt(14 * uiScale);
            style.normal.textColor = Color.white;


            GUILayout.BeginArea(new Rect(12, 12, 420, 200));
            GUILayout.Label($"Weight: {weight:0.##} kg | Cap: {cap:0.#} kg", style);
            GUILayout.Label($"r: {r:0.##} | speedScale: {encum.SpeedScale:0.00} | sprint: {(encum.SprintAllowed ? "ON" : "OFF")}", style);
            GUILayout.EndArea();
        }
    }
}