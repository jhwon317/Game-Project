using System;
using UnityEngine;

namespace PopupMini
{
    /// <summary>모달 동안 커서/잠금/맵 전환 등을 잠깐 바꾸고 복구하는 간단 Gate.</summary>
    public class InputModalGate : IDisposable
    {
        public struct Options
        {
            public string GameplayMap;  // 사용 안해도 됨(훅만)
            public string UIMap;
            public bool ShowCursor;
        }

        readonly bool _prevCursorVisible;
        readonly CursorLockMode _prevLock;

        public static InputModalGate Acquire(Options opt)
        {
            return new InputModalGate(opt);
        }

        InputModalGate(Options opt)
        {
            _prevCursorVisible = Cursor.visible;
            _prevLock = Cursor.lockState;

            if (opt.ShowCursor)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            // 실제 InputActionMap 전환은 프로젝트에 맞춰 추가하세요.
        }

        public void Dispose()
        {
            Cursor.visible = _prevCursorVisible;
            Cursor.lockState = _prevLock;
        }
    }
}