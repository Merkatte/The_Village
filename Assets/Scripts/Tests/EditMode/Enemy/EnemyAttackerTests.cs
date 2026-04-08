using System;
using NUnit.Framework;
using Game.Core.Interfaces;
using Game.Enemy.Combat;

namespace Game.Tests.EditMode.Enemy
{
    /// <summary>
    /// EnemyAttacker 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 초기 IsOnCooldown = false
    ///   [v] strategy null 전달 시 ArgumentNullException
    ///   [v] FakeStrategy.Execute 호출 여부 (delay 0 으로 설정)
    ///   [v] IsDead 타겟에는 Execute 호출되지 않음
    ///
    /// ─ 비고 ──────────────────────────────────────────────────
    ///   delay > 0 인 비동기 시퀀스(IsOnCooldown 전환)는
    ///   PlayMode 테스트에서 검증합니다.
    /// </summary>
    [TestFixture]
    public class EnemyAttackerTests
    {
        // ─ Fake ───────────────────────────────────────────────

        private sealed class FakeStrategy : IEnemyAttackStrategy
        {
            public int ExecuteCallCount { get; private set; }

            public void Execute(IDamageable target) => ExecuteCallCount++;
        }

        private sealed class FakeDamageable : IDamageable
        {
            public int     CurrentHp { get; set; } = 10;
            public bool    IsDead    { get; set; }
            public event Action OnDied;

            public void TakeDamage(int amount) { }
        }

        // ─ 초기 상태 ──────────────────────────────────────────

        [Test]
        [Description("생성 직후 IsOnCooldown 은 false 이어야 한다.")]
        public void 초기_IsOnCooldown은_false()
        {
            var strategy = new FakeStrategy();
            var attacker = new EnemyAttacker(strategy, 0f, 0f, 0f);

            Assert.IsFalse(attacker.IsOnCooldown);
        }

        // ─ 생성자 ─────────────────────────────────────────────

        [Test]
        [Description("strategy 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_strategy_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EnemyAttacker(null, 0f, 0f, 0f));
        }

        // ─ Execute 호출 여부 ───────────────────────────────────

        [Test]
        [Description("모든 delay 가 0 일 때 RequestAttackAsync 는 strategy.Execute 를 호출해야 한다.")]
        public void 모든_delay가_0일때_Execute가_호출된다()
        {
            var strategy = new FakeStrategy();
            var attacker = new EnemyAttacker(strategy, 0f, 0f, 0f);
            var target   = new FakeDamageable();

            // delay 가 모두 0 이므로 UniTask.Delay 내부 await 없이 동기적으로 실행됨
            attacker.RequestAttackAsync(target, default).Forget();
 
            Assert.AreEqual(1, strategy.ExecuteCallCount);
        }

        [Test]
        [Description("target 이 IsDead 이면 strategy.Execute 가 호출되지 않아야 한다.")]
        public void IsDead_타겟에는_Execute가_호출되지_않는다()
        {
            var strategy = new FakeStrategy();
            var attacker = new EnemyAttacker(strategy, 0f, 0f, 0f);
            var target   = new FakeDamageable { IsDead = true };

            attacker.RequestAttackAsync(target, default).Forget();

            Assert.AreEqual(0, strategy.ExecuteCallCount);
        }
    }
}
