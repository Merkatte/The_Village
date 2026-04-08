using Game.Core.Interfaces;

namespace Game.Enemy.Combat
{
    /// <summary>
    /// 적의 공격 방식을 추상화하는 인터페이스입니다.
    ///
    /// ─ 구현체 ─────────────────────────────────────────────────
    ///   MeleeAttackStrategy  — 근접 공격 (OverlapCircle 판정)
    ///   RangedAttackStrategy — 원거리 공격 투사체 (Phase 6)
    ///
    /// ─ 사용처 ─────────────────────────────────────────────────
    ///   EnemyAttacker.RequestAttackAsync 내부에서 선딜 이후 호출됩니다.
    /// </summary>
    public interface IEnemyAttackStrategy
    {
        /// <summary>
        /// 공격을 실행합니다.
        /// </summary>
        /// <param name="target">피해를 입힐 대상</param>
        void Execute(IDamageable target);
    }
}
