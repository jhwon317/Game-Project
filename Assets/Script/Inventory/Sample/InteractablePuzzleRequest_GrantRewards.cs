using UnityEngine;
using System.Threading.Tasks;
using PopupMini;
using Game.Inventory;


[RequireComponent(typeof(Collider))]
public class InteractablePuzzleRequest_GrantRewards : MonoBehaviour, IInteractable
{
    [Header("Puzzle")]
    public PopupSessionManager session;
    public PuzzleDefinition definition;
    [TextArea] public string jsonArgs;


    [Header("Reward Options")]
    public InventoryComponent inventoryOverride; // optional: explicit target
    public bool oneTimeConsume = true; // prevent repeat reward
    public bool disableAfterConsume = false; // disable this object after reward


    bool _busy; bool _consumed;


    public Transform GetTransform() => transform;
    public void SetHighlighted(bool on) { /* optional highlight */ }


    public async void OnInteract(GameObject interactor)
    {
        if (_busy || !session || !definition) return;
        if (_consumed && oneTimeConsume) return;
        _busy = true;


        var req = new PuzzleRequest
        {
            Definition = definition,
            Args = string.IsNullOrEmpty(jsonArgs) ? null : jsonArgs
        };


        var result = await session.OpenAsync(req);
        if (result.Success && !string.IsNullOrEmpty(result.Payload))
        {
            int n = RewardGrantHelper.TryGrant(
            payload: result.Payload,
            interactor: interactor,
            inventoryOverride: inventoryOverride,
            oneTime: oneTimeConsume,
            afterConsume: () =>
            {
                _consumed = true;
                if (disableAfterConsume)
                {
                    foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = false;
                    gameObject.SetActive(false);
                }
            },
            logPrefix: "[PuzzleReward]");
        }


        _busy = false;
    }
}