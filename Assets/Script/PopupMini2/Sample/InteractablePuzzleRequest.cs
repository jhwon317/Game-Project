using UnityEngine;
using System.Threading.Tasks;

namespace PopupMini
{
    [RequireComponent(typeof(Collider))]
    public class InteractablePuzzleRequest : MonoBehaviour, IInteractable
    {
        public PopupSessionManager session;   // 씬의 세션 매니저 드래그
        public PuzzleDefinition definition;
        [TextArea] public string jsonArgs;

        bool _busy;

        public Transform GetTransform() => transform;
        public void SetHighlighted(bool on) { /* optional highlight */ }

        public async void OnInteract(GameObject interactor)
        {
            if (_busy || !session || !definition) return;
            _busy = true;

            var req = new PuzzleRequest
            {
                Definition = definition,
                Args = string.IsNullOrEmpty(jsonArgs) ? null : jsonArgs
            };

            var result = await session.OpenAsync(req);
            Debug.Log($"[Puzzle] success={result.Success} reason={result.Reason} payload={result.Payload}");
            _busy = false;
        }
    }
}