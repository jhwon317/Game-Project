using UnityEngine;

namespace PopupMini
{
    [RequireComponent(typeof(Collider))]
    public class InteractablePuzzle : MonoBehaviour, IInteractable
    {
        public InteractionRouter Router;
        public PuzzleDefinition Definition;
        [TextArea] public string DebugArgsJson; // 선택: 간단 인자

        void Awake() { if (!Router) Router = FindFirstObjectByType<InteractionRouter>(); }

        public Transform GetTransform() => transform;
        public void SetHighlighted(bool on) { /* 선택: 하이라이트 */ }

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