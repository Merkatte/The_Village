using Game.Manager.Interfaces.Movement;
using UnityEngine;

namespace Game.Enemy.Movement
{
    /// <summary>
    /// 적 이동 계산을 담당하는 순수 C# 클래스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - PlayerMover 와 동일한 구조 (IMovable 인터페이스 구현)
    ///   - MonoBehaviour 가 아니므로 EditMode 에서 단위 테스트 가능
    ///   - 이동 공식 계산만 책임집니다. Transform 반영은 EnemyController 담당. (SRP)
    ///
    /// ─ 이동 공식 ──────────────────────────────────────────────
    ///   Position += direction.normalized * Speed * deltaTime
    ///   direction 이 zero 이면 이동하지 않습니다.
    /// </summary>
    public sealed class EnemyMover : IMovable
    {
        /// <inheritdoc/>
        public Vector2 Position { get; private set; }

        /// <summary>초당 이동 속도 (Unity 유닛/초)</summary>
        public float Speed { get; }

        /// <param name="speed">초당 이동 속도. 음수이면 0 으로 처리합니다.</param>
        /// <param name="startPosition">초기 위치 (기본값: 원점)</param>
        public EnemyMover(float speed, Vector2 startPosition = default)
        {
            Speed    = Mathf.Max(0f, speed);
            Position = startPosition;
        }

        /// <inheritdoc/>
        public void Move(Vector2 direction, float deltaTime)
        {
            if (direction == Vector2.zero) return;
            Position += direction.normalized * Speed * deltaTime;
        }

        /// <inheritdoc/>
        public void SetPosition(Vector2 position) => Position = position;
    }
}
