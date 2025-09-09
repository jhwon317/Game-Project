using UnityEngine;

namespace PopupMini
{
    public struct PuzzleResult
    {
        public bool Success;
        public string Reason;   // "ok" | "cancel:user" | "abort:external" | "timeout" | "error"
        public string Payload;  // optional JSON/string

        public static PuzzleResult Ok(string payload = null)
            => new PuzzleResult { Success = true, Reason = "ok", Payload = payload };

        public static PuzzleResult Cancel(string why)
            => new PuzzleResult { Success = false, Reason = why, Payload = null };

        public static PuzzleResult Error(string why)
            => new PuzzleResult { Success = false, Reason = "error:" + why, Payload = null };
    }
}
