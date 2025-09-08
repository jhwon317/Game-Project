using UnityEngine;
using System.Threading.Tasks;

namespace PopupMini
{
    public class InteractionRouter : MonoBehaviour
    {
        public PopupSessionManager Session;

        void Awake() { if (!Session) Session = FindFirstObjectByType<PopupSessionManager>(); }

        public Task<PuzzleResult> RequestOpen(PuzzleRequest req) => Session ? Session.OpenAsync(req) : Task.FromResult(PuzzleResult.Error("no_session"));
    }
}