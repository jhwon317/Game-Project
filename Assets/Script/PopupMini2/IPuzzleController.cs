using System;
using System.Threading;

namespace PopupMini
{
    public interface IPuzzleController
    {
        event Action<PuzzleResult> Completed;
        void Begin(object args, CancellationToken ct);
    }
}
