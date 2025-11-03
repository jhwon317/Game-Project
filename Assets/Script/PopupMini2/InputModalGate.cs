using System;
using UnityEngine;

namespace PopupMini
{
    /// <summary>
    /// 모달 팝업 시 커서/입력맵 전환을 위해 상태를 바꾸고 복원하는 일회용 Gate
    /// </summary>
    public class InputModalGate : IDisposable
    {
        public struct Options
        {
            public string GameplayMap;  // 팝업 활성화 시 비활성화할 맵(플레이)
            public string UIMap;        // 팝업 활성화 시 활성화할 맵(UI)
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
            
            // TODO: InputActionMap 전환은 프로젝트에 맞게 추가하세요.
            // 예시:
            // if (!string.IsNullOrEmpty(opt.GameplayMap))
            //     InputManager.DisableMap(opt.GameplayMap);
            // if (!string.IsNullOrEmpty(opt.UIMap))
            //     InputManager.EnableMap(opt.UIMap);
        }

        public void Dispose()
        {
            Cursor.visible = _prevCursorVisible;
            Cursor.lockState = _prevLock;
            
            // TODO: InputActionMap 복원
            // InputManager.RestorePreviousMaps();
        }
    }
}
