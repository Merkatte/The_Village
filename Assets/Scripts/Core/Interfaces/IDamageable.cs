using System;

namespace Game.Core.Interfaces
{
    /// <summary>
    /// 피해를 입을 수 있는 객체의 인터페이스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 플레이어(PlayerHealth)와 적(EnemyHealth) 모두 이 인터페이스를 구현합니다.
    ///   - Unity 타입에 의존하지 않으므로 EditMode 테스트가 가능합니다.
    ///   - TakeDamage 는 외부(공격 판정 로직)에서 호출합니다.
    ///
    /// ─ 사용처 ─────────────────────────────────────────────────
    ///   IEnemyAttackStrategy.Execute(IDamageable) 로 공격 대상 참조
    /// </summary>
    public interface IDamageable
    {
        /// <summary>현재 체력.</summary>
        int CurrentHp { get; }

        /// <summary>체력이 0 이하이면 true.</summary>
        bool IsDead { get; }

        /// <summary>
        /// amount 만큼 피해를 입힙니다.
        /// IsDead 가 true 이면 무시합니다.
        /// </summary>
        /// <param name="amount">피해량 (1 이상)</param>
        void TakeDamage(int amount);

        /// <summary>CurrentHp 가 0 이 되어 사망할 때 발행합니다.</summary>
        event Action OnDied;
    }
}
