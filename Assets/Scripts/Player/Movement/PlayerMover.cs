using Game.Manager.Interfaces.Movement;
using UnityEngine;

namespace Game.Player.Movement
{
    /// <summary>
    /// 플레이어 이동 계산을 담당하는 순수 C# 클래스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - MonoBehaviour 가 아니므로 EditMode 에서 단위 테스트 가능
    ///   - 이동 공식 계산만 책임집니다. Transform 반영은 PlayerController 담당. (SRP)
    ///   - IMovable 인터페이스 구현으로 AI 동료 이동과 동일 방식 적용 가능. (LSP)
    ///
    /// ─ 이동 공식 ──────────────────────────────────────────────
    ///   Position += direction.normalized * Speed * deltaTime
    ///   direction 이 zero 이면 이동하지 않습니다.
    /// </summary>
    public class PlayerMover : IMovable
    {
        /// <inheritdoc/>
        public Vector2 Position { get; private set; }

        /// <summary>초당 이동 속도 (Unity 유닛/초)</summary>
        public float Speed { get; }

        /// <summary>
        /// PlayerMover 생성자.
        /// </summary>
        /// <param name="speed">초당 이동 속도. 0 이상이어야 합니다.</param>
        /// <param name="startPosition">초기 위치 (기본값: 원점)</param>
        public PlayerMover(float speed, Vector2 startPosition = default)
        {
            Speed    = Mathf.Max(0f, speed);
            Position = startPosition;
        }

        /// <summary>
        /// 입력 방향과 경과 시간을 기반으로 Position 을 갱신합니다.
        /// direction 이 zero 이면 아무것도 하지 않습니다.
        /// </summary>
        /// <param name="direction">이동 방향 벡터 (정규화 불필요)</param>
        /// <param name="deltaTime">경과 시간 (초)</param>
        public void Move(Vector2 direction, float deltaTime)
        {
            if (direction == Vector2.zero) return;
            Position += direction.normalized * Speed * deltaTime;
        }

        /// <summary>
        /// Position 을 지정한 좌표로 즉시 설정합니다.
        /// 씬 전환, 스폰, 텔레포트 시 사용합니다.
        /// </summary>
        /// <param name="position">설정할 월드 좌표</param>
        public void SetPosition(Vector2 position) => Position = position;
    }
}
