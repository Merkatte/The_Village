using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Interfaces;

namespace Game.Enemy.Combat
{
    /// <summary>
    /// 적의 공격 시퀀스를 관리하는 순수 C# 클래스입니다.
    ///
    /// ─ 공격 시퀀스 (UniTask) ──────────────────────────────────
    ///   ① await PreAttackDelay  — 선딜
    ///   ② strategy.Execute()   — 피격 판정
    ///   ③ await PostAttackStun  — 후딜(멍)
    ///   ④ await AttackCooldown  — 쿨타임
    ///
    /// ─ 사용 방법 ──────────────────────────────────────────────
    ///   EnemyController.Update() 에서
    ///   state == Attacking &amp;&amp; !attacker.IsOnCooldown 일 때
    ///   RequestAttackAsync(target, ct).Forget() 호출
    /// </summary>
    public sealed class EnemyAttacker
    {
        private readonly IEnemyAttackStrategy _strategy;
        private readonly int _preAttackDelayMs;
        private readonly int _postAttackStunMs;
        private readonly int _attackCooldownMs;

        /// <summary>공격 시퀀스(선딜~쿨타임) 진행 중이면 true.</summary>
        public bool IsOnCooldown { get; private set; }

        /// <param name="strategy">공격 전략</param>
        /// <param name="preAttackDelay">선딜 (초)</param>
        /// <param name="postAttackStun">후딜(멍) (초)</param>
        /// <param name="attackCooldown">쿨타임 (초)</param>
        public EnemyAttacker(
            IEnemyAttackStrategy strategy,
            float preAttackDelay,
            float postAttackStun,
            float attackCooldown)
        {
            _strategy         = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _preAttackDelayMs = (int)(Math.Max(0f, preAttackDelay)  * 1000);
            _postAttackStunMs = (int)(Math.Max(0f, postAttackStun)  * 1000);
            _attackCooldownMs = (int)(Math.Max(0f, attackCooldown)  * 1000);
        }

        /// <summary>
        /// 공격 시퀀스를 비동기로 실행합니다.
        /// 이미 IsOnCooldown 이면 즉시 반환합니다.
        /// </summary>
        /// <param name="target">공격 대상</param>
        /// <param name="ct">취소 토큰 (EnemyController 의 OnDestroy 에서 취소)</param>
        public async UniTaskVoid RequestAttackAsync(IDamageable target, CancellationToken ct)
        {
            if (IsOnCooldown) return;

            IsOnCooldown = true;
            try
            {
                if (_preAttackDelayMs > 0)
                    await UniTask.Delay(_preAttackDelayMs, cancellationToken: ct);

                if (target != null && !target.IsDead)
                    _strategy.Execute(target);

                if (_postAttackStunMs > 0)
                    await UniTask.Delay(_postAttackStunMs, cancellationToken: ct);

                if (_attackCooldownMs > 0)
                    await UniTask.Delay(_attackCooldownMs, cancellationToken: ct);
            }
            finally
            {
                IsOnCooldown = false;
            }
        }
    }
}
