using UnityEngine;

namespace Game.Manager.Interfaces.Movement
{
    /// <summary>
    /// 이동 가능한 객체의 동작을 추상화하는 인터페이스입니다.
    ///
    /// ─ 위치 결정 이유 ─────────────────────────────────────────
    ///   Vector2(UnityEngine 타입)를 사용하므로 Game.Manager 레이어에 배치합니다.
    ///   Game.Core(noEngineReferences:true) 에는 Unity 타입을 쓸 수 없습니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - PlayerController 는 이 인터페이스를 통해 이동 구현에 의존하지 않습니다. (DIP)
    ///   - 동료 AI 이동도 동일한 인터페이스를 구현하면
    ///     PlayerController 와 동일한 방식으로 처리 가능합니다. (LSP)
    ///
    /// 구현체: PlayerMover
    /// 향후 구현체: CompanionMover (AI 동료 이동)
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// 현재 논리 위치를 반환합니다.
        /// Transform.position 과는 독립적으로 관리됩니다.
        /// </summary>
        Vector2 Position { get; }

        /// <summary>
        /// 지정한 방향으로 한 프레임 이동을 계산합니다.
        /// 실제 Transform 반영은 PlayerController 가 담당합니다.
        /// </summary>
        /// <param name="direction">이동 방향 벡터 (정규화 불필요, 내부에서 처리)</param>
        /// <param name="deltaTime">경과 시간 (Time.deltaTime)</param>
        void Move(Vector2 direction, float deltaTime);

        /// <summary>
        /// 위치를 강제로 설정합니다.
        /// 씬 전환 또는 스폰 시 사용합니다.
        /// </summary>
        /// <param name="position">설정할 월드 좌표</param>
        void SetPosition(Vector2 position);
    }
}
