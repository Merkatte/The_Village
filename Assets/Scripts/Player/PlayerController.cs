using Game.Core.Infrastructure;
using Game.Manager.Interfaces.Input;
using Game.Manager.Interfaces.Movement;
using Game.Player.Movement;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 플레이어 GameObject 에 붙는 MonoBehaviour 브릿지입니다.
    ///
    /// ─ 역할 (SRP) ─────────────────────────────────────────────
    ///   - ServiceLocator 에서 IPlayerInputReader 를 받아 이동 이벤트를 구독합니다.
    ///   - 매 프레임 PlayerMover.Move() 를 호출하여 논리 좌표를 계산합니다.
    ///   - 계산된 Position 을 transform.position 에 반영합니다.
    ///   - 이동 로직 자체는 PlayerMover(IMovable) 에 위임합니다. (DIP)
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   moveSpeed : 초당 이동 속도 (기본값 5)
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <summary>초당 이동 속도 (Inspector 에서 설정 가능)</summary>
        [SerializeField] private float moveSpeed = 5f;

        /// <summary>체력 관리 컴포넌트 — 같은 GameObject 에 부착합니다.</summary>
        [SerializeField] private PlayerHealth playerHealth;

        /// <summary>이동 계산을 담당하는 논리 객체 (IMovable 인터페이스로만 참조)</summary>
        private IMovable _mover;

        /// <summary>입력 이벤트 공급자 (IPlayerInputReader 인터페이스로만 참조)</summary>
        private IPlayerInputReader _inputReader;

        /// <summary>현재 프레임의 이동 방향 벡터</summary>
        private Vector2 _moveDirection;

        private void Awake()
        {
            // IMovable 구현체를 생성합니다. (PlayerMover)
            _mover = new PlayerMover(moveSpeed, transform.position);

            // ServiceLocator 에서 IPlayerInputReader 를 주입받습니다. (DIP)
            _inputReader = ServiceLocator.Get<IPlayerInputReader>();

            SubscribeInput();
        }

        private void Update()
        {
            // 사망 시 이동 중단
            if (playerHealth != null && playerHealth.IsDead) return;

            // 매 프레임 이동 계산을 PlayerMover 에 위임합니다.
            _mover.Move(_moveDirection, Time.deltaTime);

            // 계산된 논리 위치를 Transform 에 반영합니다.
            transform.position = new Vector3(
                _mover.Position.x,
                _mover.Position.y,
                transform.position.z);
        }

        private void OnDestroy() => UnsubscribeInput();

        /// <summary>
        /// IPlayerInputReader 의 이동 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeInput()
        {
            _inputReader.OnMovePerformed  += OnMovePerformed;
            _inputReader.OnMoveCancelled  += OnMoveCancelled;
        }

        /// <summary>
        /// IPlayerInputReader 의 이동 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeInput()
        {
            if (_inputReader == null) return;
            _inputReader.OnMovePerformed  -= OnMovePerformed;
            _inputReader.OnMoveCancelled  -= OnMoveCancelled;
        }

        /// <summary>
        /// 이동 입력이 들어왔을 때 방향 벡터를 저장합니다.
        /// </summary>
        /// <param name="direction">입력 방향 벡터</param>
        private void OnMovePerformed(Vector2 direction) => _moveDirection = direction;

        /// <summary>
        /// 이동 입력이 해제되었을 때 방향 벡터를 초기화합니다.
        /// </summary>
        private void OnMoveCancelled() => _moveDirection = Vector2.zero;
    }
}
