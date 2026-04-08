namespace Game.Core.Enums
{
    /// <summary>
    /// 적의 AI 상태를 정의합니다.
    ///
    /// ─ 전환 조건 ──────────────────────────────────────────────
    ///   Idle      : 거리 > AlertRange
    ///   Alert     : CombatRange  < 거리 ≤ AlertRange
    ///   Combat    : AttackRange  < 거리 ≤ CombatRange
    ///   Attacking : 거리 ≤ AttackRange + !isAttacking
    ///   Dead      : IsDead == true (어느 상태에서든 최우선)
    /// </summary>
    public enum EnemyState
    {
        /// <summary>플레이어를 감지하지 못한 대기 상태.</summary>
        Idle,

        /// <summary>플레이어를 감지해 접근하는 경계 상태.</summary>
        Alert,

        /// <summary>전투 거리 내에서 추적하는 상태.</summary>
        Combat,

        /// <summary>공격을 수행 중인 상태 (UniTask 공격 시퀀스 실행).</summary>
        Attacking,

        /// <summary>사망 상태. 모든 행동이 중단됩니다.</summary>
        Dead,
    }
}
