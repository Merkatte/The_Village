namespace Game.Core.Enums
{
    /// <summary>
    /// 적의 공격 방식을 정의합니다.
    ///
    /// ─ 구현체 매핑 ────────────────────────────────────────────
    ///   Melee   → MeleeAttackStrategy
    ///   Ranged  → RangedAttackStrategy (Phase 6)
    /// </summary>
    public enum AttackType
    {
        /// <summary>근접 공격.</summary>
        Melee,

        /// <summary>원거리 공격 (투사체).</summary>
        Ranged,
    }
}
