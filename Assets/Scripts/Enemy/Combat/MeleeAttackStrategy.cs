using System;
using Game.Core.Interfaces;

namespace Game.Enemy.Combat
{
    /// <summary>
    /// 근접 공격을 수행하는 전략 클래스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - EnemyBrain 이 이미 AttackRange 이내임을 보장한 상태에서 호출됩니다.
    ///   - 피해량은 생성 시 EnemyData.AttackDamage 로 주입됩니다.
    ///   - target 이 null 이거나 IsDead 이면 피해를 주지 않습니다.
    /// </summary>
    public sealed class MeleeAttackStrategy : IEnemyAttackStrategy
    {
        private readonly int _damage;

        /// <param name="damage">1회 피해량 (1 이상)</param>
        public MeleeAttackStrategy(int damage)
        {
            if (damage <= 0)
                throw new ArgumentOutOfRangeException(nameof(damage), "[MeleeAttackStrategy] damage 는 1 이상이어야 합니다.");

            _damage = damage;
        }

        /// <inheritdoc/>
        public void Execute(IDamageable target)
        {
            if (target == null || target.IsDead) return;
            target.TakeDamage(_damage);
        }
    }
}
