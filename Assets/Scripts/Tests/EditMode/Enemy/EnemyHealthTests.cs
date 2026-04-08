using System;
using NUnit.Framework;
using Game.Enemy.Health;

namespace Game.Tests.EditMode.Enemy
{
    /// <summary>
    /// EnemyHealth 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] TakeDamage → CurrentHp 감소
    ///   [v] TakeDamage 초과 → CurrentHp = 0, IsDead = true
    ///   [v] OnDied 이벤트 발행
    ///   [v] IsDead 후 TakeDamage 무시 (중복 사망 방지)
    ///   [v] CurrentHp 하한 0
    ///   [v] maxHp 0 이하 → ArgumentOutOfRangeException
    ///   [v] amount 0 이하 → ArgumentOutOfRangeException
    /// </summary>
    [TestFixture]
    public class EnemyHealthTests
    {
        // ─ 생성자 ─────────────────────────────────────────────

        [Test]
        [Description("maxHp 가 0 이하이면 ArgumentOutOfRangeException 이 발생해야 한다.")]
        public void maxHp가_0이하이면_ArgumentOutOfRangeException_발생()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyHealth(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyHealth(-1));
        }

        // ─ TakeDamage ─────────────────────────────────────────

        [Test]
        [Description("TakeDamage 호출 시 CurrentHp 가 amount 만큼 감소해야 한다.")]
        public void TakeDamage_호출시_CurrentHp가_감소한다()
        {
            var health = new EnemyHealth(30);
            health.TakeDamage(10);

            Assert.AreEqual(20, health.CurrentHp);
        }

        [Test]
        [Description("TakeDamage 가 CurrentHp 를 초과하면 CurrentHp 가 0 이 되어야 한다.")]
        public void TakeDamage_초과시_CurrentHp는_0()
        {
            var health = new EnemyHealth(30);
            health.TakeDamage(50);

            Assert.AreEqual(0, health.CurrentHp);
        }

        [Test]
        [Description("TakeDamage 가 CurrentHp 를 초과하면 IsDead 가 true 이어야 한다.")]
        public void TakeDamage_초과시_IsDead는_true()
        {
            var health = new EnemyHealth(30);
            health.TakeDamage(50);

            Assert.IsTrue(health.IsDead);
        }

        [Test]
        [Description("CurrentHp 가 0 이 되면 OnDied 이벤트가 발행되어야 한다.")]
        public void CurrentHp가_0이되면_OnDied_이벤트_발행()
        {
            var health  = new EnemyHealth(10);
            var invoked = false;
            health.OnDied += () => invoked = true;

            health.TakeDamage(10);

            Assert.IsTrue(invoked);
        }

        [Test]
        [Description("OnDied 는 CurrentHp 가 0 이 되기 전에는 발행되지 않아야 한다.")]
        public void OnDied는_사망_전까지_발행되지_않는다()
        {
            var health  = new EnemyHealth(30);
            var invoked = false;
            health.OnDied += () => invoked = true;

            health.TakeDamage(10);

            Assert.IsFalse(invoked);
        }

        [Test]
        [Description("IsDead 상태에서 TakeDamage 를 호출해도 CurrentHp 가 변하지 않아야 한다 (중복 사망 방지).")]
        public void IsDead_후_TakeDamage는_무시된다()
        {
            var health = new EnemyHealth(10);
            health.TakeDamage(10);  // IsDead = true
            health.TakeDamage(10);  // 이 호출은 무시되어야 함

            Assert.AreEqual(0, health.CurrentHp);
        }

        [Test]
        [Description("OnDied 이벤트는 IsDead 이후 추가 TakeDamage 에서 다시 발행되지 않아야 한다.")]
        public void IsDead_후_OnDied_이벤트는_재발행되지_않는다()
        {
            var health     = new EnemyHealth(10);
            var callCount  = 0;
            health.OnDied += () => callCount++;

            health.TakeDamage(10);  // 첫 사망 → callCount = 1
            health.TakeDamage(10);  // 무시

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("amount 가 0 이하이면 ArgumentOutOfRangeException 이 발생해야 한다.")]
        public void amount가_0이하이면_ArgumentOutOfRangeException_발생()
        {
            var health = new EnemyHealth(30);

            Assert.Throws<ArgumentOutOfRangeException>(() => health.TakeDamage(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => health.TakeDamage(-1));
        }
    }
}
