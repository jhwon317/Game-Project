using UnityEngine;

namespace PopupMini
{
    [RequireComponent(typeof(Collider))]
    public class InteractablePuzzle : MonoBehaviour, IInteractable
    {
        public InteractionRouter Router;
        public PuzzleDefinition Definition;
        [TextArea] public string DebugArgsJson; // ����: ���� ����

        void Awake() { if (!Router) Router = FindFirstObjectByType<InteractionRouter>(); }

        public Transform GetTransform() => transform;
        public void SetHighlighted(bool on) { /* ����: ���̶���Ʈ */ }

        public async void OnInteract(GameObject interactor)
        {
            var req = new PuzzleRequest
            {
                Definition = Definition,
                Args = string.IsNullOrEmpty(DebugArgsJson) ? null : DebugArgsJson
            };
            var r = await Router.RequestOpen(req);
            Debug.Log($"[InteractablePuzzle] result success={r.Success}, reason={r.Reason}, payload={r.Payload}");
        }
    }
}