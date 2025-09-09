using System;
using UnityEngine;

namespace PopupMini
{
    /// <summary>��� ���� Ŀ��/���/�� ��ȯ ���� ��� �ٲٰ� �����ϴ� ���� Gate.</summary>
    public class InputModalGate : IDisposable
    {
        public struct Options
        {
            public string GameplayMap;  // ��� ���ص� ��(�Ÿ�)
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
            // ���� InputActionMap ��ȯ�� ������Ʈ�� ���� �߰��ϼ���.
        }

        public void Dispose()
        {
            Cursor.visible = _prevCursorVisible;
            Cursor.lockState = _prevLock;
        }
    }
}