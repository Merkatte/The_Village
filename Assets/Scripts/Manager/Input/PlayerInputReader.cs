using System;
using Game.Manager.Interfaces.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Manager.Input
{
    /// <summary>
    /// IPlayerInputReader 의 구체 구현입니다.
    /// New Input System 의 InputAction 을 이 클래스 내부에만 캡슐화합니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 외부는 IPlayerInputReader 인터페이스만 알고, InputAction 은 모릅니다. (DIP)
    ///   - IDisposable 구현으로 InputAction 메모리를 안전하게 해제합니다.
    ///   - InputManager 가 Enable / Disable 을 호출하여 컨텍스트를 제어합니다.
    ///
    /// ─ 바인딩 규칙 ────────────────────────────────────────────
    ///   Move     → WASD / 왼쪽 스틱 (2D Vector Composite)
    /// </summary>
    public class PlayerInputReader : IPlayerInputReader, IDisposable
    {
        /// <inheritdoc/>
        public event Action<Vector2> OnMovePerformed;

        /// <inheritdoc/>
        public event Action OnMoveCancelled;

        /// <inheritdoc/>
        public event Action OnHarvestStarted;

        /// <inheritdoc/>
        public event Action OnHarvestCancelled;

        /// <summary>이동 입력을 담당하는 InputAction</summary>
        private readonly InputAction _moveAction;

        /// <summary>채취 홀드 입력을 담당하는 InputAction (키 미정 — 현재 스페이스 임시 바인딩)</summary>
        private readonly InputAction _harvestAction;

        /// <summary>
        /// PlayerInputReader 생성자.
        /// WASD 와 좌측 스틱을 2D Vector Composite 으로 바인딩합니다.
        /// </summary>
        public PlayerInputReader()
        {
            _moveAction = new InputAction("Move", InputActionType.Value);

            // WASD 키보드 바인딩 (2D Vector Composite)
            var composite = _moveAction.AddCompositeBinding("2DVector");
            composite.With("Up",    "<Keyboard>/w");
            composite.With("Down",  "<Keyboard>/s");
            composite.With("Left",  "<Keyboard>/a");
            composite.With("Right", "<Keyboard>/d");

            // 방향키 추가 바인딩
            var arrowComposite = _moveAction.AddCompositeBinding("2DVector");
            arrowComposite.With("Up",    "<Keyboard>/upArrow");
            arrowComposite.With("Down",  "<Keyboard>/downArrow");
            arrowComposite.With("Left",  "<Keyboard>/leftArrow");
            arrowComposite.With("Right", "<Keyboard>/rightArrow");

            // 게임패드 왼쪽 스틱 바인딩
            _moveAction.AddBinding("<Gamepad>/leftStick");

            _moveAction.performed += OnMoveActionPerformed;
            _moveAction.canceled  += OnMoveActionCancelled;

            // 채취 키 — 임시: 스페이스바. 키 확정 후 아래 바인딩을 교체하세요.
            _harvestAction = new InputAction("Harvest", InputActionType.Button);
            _harvestAction.AddBinding("<Keyboard>/space"); // TODO: 임시 바인딩 (스페이스바)
            _harvestAction.AddBinding("<Gamepad>/buttonSouth"); // 게임패드 A(남쪽) 버튼
            _harvestAction.started  += OnHarvestActionStarted;
            _harvestAction.canceled += OnHarvestActionCanceled;
        }

        /// <inheritdoc/>
        public void Enable()
        {
            _moveAction.Enable();
            _harvestAction.Enable();
        }

        /// <inheritdoc/>
        public void Disable()
        {
            _moveAction.Disable();
            _harvestAction.Disable();
        }

        /// <summary>
        /// InputSystem 의 performed 콜백을 받아 OnMovePerformed 이벤트로 변환합니다.
        /// </summary>
        private void OnMoveActionPerformed(InputAction.CallbackContext ctx)
            => OnMovePerformed?.Invoke(ctx.ReadValue<Vector2>());

        /// <summary>
        /// InputSystem 의 canceled 콜백을 받아 OnMoveCancelled 이벤트로 변환합니다.
        /// </summary>
        private void OnMoveActionCancelled(InputAction.CallbackContext ctx)
            => OnMoveCancelled?.Invoke();

        /// <summary>
        /// 채취 키를 누를 때 OnHarvestStarted 이벤트를 발행합니다.
        /// </summary>
        private void OnHarvestActionStarted(InputAction.CallbackContext ctx)
            => OnHarvestStarted?.Invoke();

        /// <summary>
        /// 채취 키를 뗄 때 OnHarvestCancelled 이벤트를 발행합니다.
        /// </summary>
        private void OnHarvestActionCanceled(InputAction.CallbackContext ctx)
            => OnHarvestCancelled?.Invoke();

        /// <summary>
        /// InputAction 리소스를 해제합니다.
        /// InputManager.OnDestroy() 에서 반드시 호출하세요.
        /// </summary>
        public void Dispose()
        {
            _moveAction.performed -= OnMoveActionPerformed;
            _moveAction.canceled  -= OnMoveActionCancelled;
            _moveAction.Dispose();

            _harvestAction.started  -= OnHarvestActionStarted;
            _harvestAction.canceled -= OnHarvestActionCanceled;
            _harvestAction.Dispose();
        }
    }
}
