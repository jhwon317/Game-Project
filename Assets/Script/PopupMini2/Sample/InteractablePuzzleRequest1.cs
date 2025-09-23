using UnityEngine;
using System.Threading.Tasks;
using Game.Inventory; // InventoryComponent, ItemDefinition

namespace PopupMini
{
    [RequireComponent(typeof(Collider))]
    public class InteractablePuzzleRequest1 : MonoBehaviour, IInteractable
    {
        [Header("Puzzle")]
        public PopupSessionManager session;   // ���� ���� �Ŵ��� �巡��
        public PuzzleDefinition definition;
        [TextArea] public string jsonArgs;

        [Header("Reward (on Success)")]
        public ItemDefinition rewardItem;     // ������ ������(SO). ��: HumanCarry �Ǵ� Extinguisher
        [Min(1)] public int rewardCount = 1;  // ���� ����(��κ� 1)
        public bool oneTimeReward = true;     // �� ���� ����(���� ��ȣ�ۿ� �ݺ� ����)
        public bool disableAfterReward = false; // ���� �� ������Ʈ ��Ȱ��ȭ ����

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

            // �� ���� �� ���� ����
            if (result.Success && rewardItem)
            {
                var inv = interactor.GetComponent<InventoryComponent>();
                if (!inv)
                {
                    Debug.LogWarning($"[InteractablePuzzleRequest] InventoryComponent�� {interactor.name}�� �����ϴ�. ���� ���� ����.");
                }
                else
                {
                    inv.Add(rewardItem, rewardCount);
                    _consumed = true;
                    Debug.Log($"[Reward] {rewardItem.DisplayName} x{rewardCount} ���� (���� +{rewardItem.UnitWeightKg * rewardCount}kg)");
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
