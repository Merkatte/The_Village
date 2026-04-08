using System;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 플레이어의 체력을 관리하는 MonoBehaviour 브릿지입니다.
    ///
    /// ─ 역할 (SRP) ─────────────────────────────────────────────
    ///   - IDamageable 구현으로 EnemyAttacker 가 직접 피해를 입힙니다.
    ///   - Awake 에서 ServiceLocator 에 자신을 등록합니다.
    ///   - EnemyController 는 ServiceLocator 를 통해 이 컴포넌트를 참조합니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   maxHp : 최대 체력 (기본값 100)
    /// </summary>
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHp = 100;

        /// <inheritdoc/>
        public int CurrentHp { get; private set; }

        /// <inheritdoc/>
        public bool IsDead => CurrentHp <= 0;

        /// <inheritdoc/>
        public event Action OnDied;

        private void Awake()
        {
            CurrentHp = maxHp;
            ServiceLocator.Register<PlayerHealth>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<PlayerHealth>();
        }

        /// <inheritdoc/>
        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            if (amount <= 0) return;

            CurrentHp = Mathf.Max(0, CurrentHp - amount);

            if (IsDead)
                OnDied?.Invoke();
        }
    }
}
