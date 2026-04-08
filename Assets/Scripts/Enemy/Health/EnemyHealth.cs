using System;
using Game.Core.Interfaces;

namespace Game.Enemy.Health
{
    /// <summary>
    /// 적의 체력 관리를 담당하는 순수 C# 클래스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - IDamageable 구현 → MeleeAttackStrategy.Execute(IDamageable) 에서 호출
    ///   - MonoBehaviour 가 아니므로 EditMode 에서 단위 테스트 가능
    ///   - IsDead 후 TakeDamage 호출은 무시 (중복 사망 방지)
    ///   - CurrentHp 하한은 0
    /// </summary>
    public sealed class EnemyHealth : IDamageable
    {
        /// <inheritdoc/>
        public int CurrentHp { get; private set; }

        /// <inheritdoc/>
        public bool IsDead => CurrentHp <= 0;

        /// <inheritdoc/>
        public event Action OnDied;

        /// <param name="maxHp">최대 체력 (1 이상)</param>
        public EnemyHealth(int maxHp)
        {
            if (maxHp <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxHp), "[EnemyHealth] maxHp 은 1 이상이어야 합니다.");

            CurrentHp = maxHp;
        }

        /// <inheritdoc/>
        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "[EnemyHealth] amount 은 1 이상이어야 합니다.");

            CurrentHp = Math.Max(0, CurrentHp - amount);

            if (IsDead)
                OnDied?.Invoke();
        }
    }
}
