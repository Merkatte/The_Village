using System;
using Game.Manager.Interfaces.Input;
using UnityEngine.InputSystem;

namespace Game.Manager.Input
{
    /// <summary>
    /// IUIInputReader 의 구체 구현입니다.
    /// New Input System 의 InputAction 을 이 클래스 내부에만 캡슐화합니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 외부는 IUIInputReader 인터페이스만 알고, InputAction 은 모릅니다. (DIP)
    ///   - IDisposable 구현으로 InputAction 메모리를 안전하게 해제합니다.
    ///
    /// ─ 바인딩 규칙 ────────────────────────────────────────────
    ///   Submit → 스페이스, 엔터, 게임패드 South(A)
    ///   Cancel → ESC, 게임패드 East(B)
    /// </summary>
    public class UIInputReader : IUIInputReader, IDisposable
    {
        /// <inheritdoc/>
        public event Action OnSubmitPerformed;

        /// <inheritdoc/>
        public event Action OnCancelPerformed;

        /// <summary>확인 입력을 담당하는 InputAction</summary>
        private readonly InputAction _submitAction;

        /// <summary>취소 입력을 담당하는 InputAction</summary>
        private readonly InputAction _cancelAction;

        /// <summary>
        /// UIInputReader 생성자.
        /// Submit / Cancel 액션을 바인딩합니다.
        /// </summary>
        public UIInputReader()
        {
            _submitAction = new InputAction("Submit", InputActionType.Button);
            _submitAction.AddBinding("<Keyboard>/space");
            _submitAction.AddBinding("<Keyboard>/enter");
            _submitAction.AddBinding("<Gamepad>/buttonSouth");

            _cancelAction = new InputAction("Cancel", InputActionType.Button);
            _cancelAction.AddBinding("<Keyboard>/escape");
            _cancelAction.AddBinding("<Gamepad>/buttonEast");

            _submitAction.performed += _ => OnSubmitPerformed?.Invoke();
            _cancelAction.performed += _ => OnCancelPerformed?.Invoke();
        }

        /// <inheritdoc/>
        public void Enable()
        {
            _submitAction.Enable();
            _cancelAction.Enable();
        }

        /// <inheritdoc/>
        public void Disable()
        {
            _submitAction.Disable();
            _cancelAction.Disable();
        }

        /// <summary>
        /// InputAction 리소스를 해제합니다.
        /// InputManager.OnDestroy() 에서 반드시 호출하세요.
        /// </summary>
        public void Dispose()
        {
            _submitAction.Dispose();
            _cancelAction.Dispose();
        }
    }
}
