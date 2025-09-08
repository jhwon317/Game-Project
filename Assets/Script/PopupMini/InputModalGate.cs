using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PopupMini
{
    public static class InputModalGate
    {
        public struct Options { public string GameplayMap; public string UIMap; public bool ShowCursor; }

        public static IDisposable Acquire(Options opt)
        {
#if ENABLE_INPUT_SYSTEM
            var pi = UnityEngine.Object.FindFirstObjectByType<PlayerInput>();
            if (pi)
            {
                var gm = pi.actions.FindActionMap(string.IsNullOrEmpty(opt.GameplayMap) ? "Gameplay" : opt.GameplayMap, true);
                var ui = pi.actions.FindActionMap(string.IsNullOrEmpty(opt.UIMap) ? "UI" : opt.UIMap, true);
                gm?.Disable(); ui?.Enable();
            }
#endif
            if (opt.ShowCursor) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            return new Releaser(() =>
            {
#if ENABLE_INPUT_SYSTEM
                var pi = UnityEngine.Object.FindFirstObjectByType<PlayerInput>();
                if (pi)
                {
                    var gm = pi.actions.FindActionMap(string.IsNullOrEmpty(opt.GameplayMap) ? "Gameplay" : opt.GameplayMap, true);
                    var ui = pi.actions.FindActionMap(string.IsNullOrEmpty(opt.UIMap) ? "UI" : opt.UIMap, true);
                    gm?.Enable(); ui?.Enable();
                }
#endif
                if (opt.ShowCursor) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            });
        }

        private sealed class Releaser : IDisposable { readonly Action r; public Releaser(Action rr) => r = rr; public void Dispose() { r?.Invoke(); } }
    }
}