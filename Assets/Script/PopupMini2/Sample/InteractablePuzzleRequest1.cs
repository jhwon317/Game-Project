using UnityEngine;
using System.Threading.Tasks;
using Game.Inventory; // InventoryComponent, ItemDefinition

namespace PopupMini
{
    [RequireComponent(typeof(Collider))]
    public class InteractablePuzzleRequest1 : MonoBehaviour, IInteractable
    {
        [Header("Puzzle")]
        public PopupSessionManager session;   // 씬의 세션 매니저 드래그
        public PuzzleDefinition definition;
        [TextArea] public string jsonArgs;

        [Header("Reward (on Success)")]
        public ItemDefinition rewardItem;     // 지급할 아이템(SO). 예: HumanCarry 또는 Extinguisher
        [Min(1)] public int rewardCount = 1;  // 지급 수량(대부분 1)
        public bool oneTimeReward = true;     // 한 번만 지급(같은 상호작용 반복 방지)
        public bool disableAfterReward = false; // 보상 후 오브젝트 비활성화 여부

        bool _busy;
        bool _consumed;

        public Transform GetTransform() => transform;
        public void SetHighlighted(bool on) { /* optional highlight */ }

        public async void OnInteract(GameObject interactor)
        {
            if (_busy || !session || !definition) return;
            if (_consumed && oneTimeReward) return;
            _busy = true;

            var req = new PuzzleRequest
            {
                Definition = definition,
                Args = string.IsNullOrEmpty(jsonArgs) ? null : jsonArgs
            };

            var result = await session.OpenAsync(req);
            Debug.Log($"[Puzzle] success={result.Success} reason={result.Reason} payload={result.Payload}");

            // ★ 성공 시 보상 지급
            if (result.Success && rewardItem)
            {
                var inv = interactor.GetComponent<InventoryComponent>();
                if (!inv)
                {
                    Debug.LogWarning($"[InteractablePuzzleRequest] InventoryComponent가 {interactor.name}에 없습니다. 보상 지급 실패.");
                }
                else
                {
                    inv.Add(rewardItem, rewardCount);
                    _consumed = true;
                    Debug.Log($"[Reward] {rewardItem.DisplayName} x{rewardCount} 지급 (무게 +{rewardItem.UnitWeightKg * rewardCount}kg)");
                    if (disableAfterReward)
                    {
                        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = false;
                        gameObject.SetActive(false);
                    }
                }
            }

            _busy = false;
        }
    }
}
