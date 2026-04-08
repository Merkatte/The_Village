using NUnit.Framework;
using Game.Core.Enums;
using Game.Enemy.Core;

namespace Game.Tests.EditMode.Enemy
{
    /// <summary>
    /// EnemyBrain.Tick() 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 거리 > AlertRange → Idle 반환
    ///   [v] CombatRange &lt; 거리 ≤ AlertRange → Alert 반환
    ///   [v] AttackRange &lt; 거리 ≤ CombatRange → Combat 반환
    ///   [v] 거리 ≤ AttackRange, isAttacking=false → Attacking 반환
    ///   [v] 거리 ≤ AttackRange, isAttacking=true  → Attacking 유지
    ///   [v] Dead 상태는 어떤 거리에서도 Dead 유지
    ///   [v] 경계값: distance == AlertRange
    ///   [v] 경계값: distance == AttackRange
    ///   [v] Combat 상태에서 CombatRange 초과 시 Alert 반환 (Idle 아님)
    ///   [v] Alert 상태에서 AlertRange 초과 시 Idle 반환
    ///   [v] Idle 상태에서 AttackRange 이하여도 Alert 먼저 반환 (즉시 Attacking 불가)
    /// </summary>
    [TestFixture]
    public class EnemyBrainTests
    {
        // AlertRange=10, CombatRange=6, AttackRange=2
        private EnemyBrain _brain;
        private EnemyData  _data;

        [SetUp]
        public void SetUp()
        {
            _data = new EnemyData(
                enemyId:        "test_enemy",
                enemyType:      EnemyType.Slime,
                maxHp:          30,
                moveSpeed:      2f,
                alertRange:     10f,
                combatRange:    6f,
                attackRange:    2f,
                attackDamage:   5,
                attackCooldown: 2f,
                preAttackDelay: 0.2f,
                postAttackStun: 0.4f,
                attackType:     AttackType.Melee);

            _brain = new EnemyBrain(_data);
        }

        // ─ Idle ───────────────────────────────────────────────

        [Test]
        [Description("거리가 AlertRange 초과이면 Idle 을 반환해야 한다.")]
        public void 거리가_AlertRange_초과이면_Idle_반환()
        {
            var state = _brain.Tick(EnemyState.Idle, distance: 10.1f, isAttacking: false);

            Assert.AreEqual(EnemyState.Idle, state);
        }

        // ─ Alert ──────────────────────────────────────────────

        [Test]
        [Description("거리가 CombatRange 초과 AlertRange 이하이면 Alert 를 반환해야 한다.")]
        public void 거리가_CombatRange_초과_AlertRange_이하이면_Alert_반환()
        {
            var state = _brain.Tick(EnemyState.Idle, distance: 8f, isAttacking: false);

            Assert.AreEqual(EnemyState.Alert, state);
        }

        // ─ Combat ─────────────────────────────────────────────

        [Test]
        [Description("거리가 AttackRange 초과 CombatRange 이하이면 Combat 을 반환해야 한다.")]
        public void 거리가_AttackRange_초과_CombatRange_이하이면_Combat_반환()
        {
            var state = _brain.Tick(EnemyState.Alert, distance: 4f, isAttacking: false);

            Assert.AreEqual(EnemyState.Combat, state);
        }

        // ─ Attacking ──────────────────────────────────────────

        [Test]
        [Description("거리가 AttackRange 이하이고 isAttacking=false 이면 Attacking 을 반환해야 한다.")]
        public void 거리가_AttackRange_이하이고_isAttacking_false이면_Attacking_반환()
        {
            var state = _brain.Tick(EnemyState.Combat, distance: 1.5f, isAttacking: false);

            Assert.AreEqual(EnemyState.Attacking, state);
        }

        [Test]
        [Description("거리가 AttackRange 이하이고 isAttacking=true 이면 Attacking 을 유지해야 한다.")]
        public void 거리가_AttackRange_이하이고_isAttacking_true이면_Attacking_유지()
        {
            var state = _brain.Tick(EnemyState.Attacking, distance: 1.5f, isAttacking: true);

            Assert.AreEqual(EnemyState.Attacking, state);
        }

        // ─ Dead ───────────────────────────────────────────────

        [Test]
        [Description("Dead 상태는 어떤 거리에서도 Dead 를 유지해야 한다.")]
        public void Dead_상태는_어떤_거리에서도_Dead_유지()
        {
            Assert.AreEqual(EnemyState.Dead, _brain.Tick(EnemyState.Dead, distance: 0f,   isAttacking: false));
            Assert.AreEqual(EnemyState.Dead, _brain.Tick(EnemyState.Dead, distance: 1f,   isAttacking: false));
            Assert.AreEqual(EnemyState.Dead, _brain.Tick(EnemyState.Dead, distance: 100f, isAttacking: false));
        }

        // ─ 경계값 ─────────────────────────────────────────────

        [Test]
        [Description("거리가 AlertRange 와 정확히 같으면 Alert 를 반환해야 한다.")]
        public void 경계값_거리가_AlertRange와_같으면_Alert_반환()
        {
            var state = _brain.Tick(EnemyState.Idle, distance: 10f, isAttacking: false);

            Assert.AreEqual(EnemyState.Alert, state);
        }

        [Test]
        [Description("거리가 AttackRange 와 정확히 같으면 Attacking 을 반환해야 한다.")]
        public void 경계값_거리가_AttackRange와_같으면_Attacking_반환()
        {
            var state = _brain.Tick(EnemyState.Combat, distance: 2f, isAttacking: false);

            Assert.AreEqual(EnemyState.Attacking, state);
        }

        // ─ FSM 전환 ───────────────────────────────────────────

        [Test]
        [Description("Combat 상태에서 CombatRange 초과 시 Idle 이 아닌 Alert 를 반환해야 한다.")]
        public void Combat_상태에서_CombatRange_초과시_Alert_반환()
        {
            var state = _brain.Tick(EnemyState.Combat, distance: 7f, isAttacking: false);

            Assert.AreEqual(EnemyState.Alert, state);
        }

        [Test]
        [Description("Alert 상태에서 AlertRange 초과 시 Idle 을 반환해야 한다.")]
        public void Alert_상태에서_AlertRange_초과시_Idle_반환()
        {
            var state = _brain.Tick(EnemyState.Alert, distance: 10.1f, isAttacking: false);

            Assert.AreEqual(EnemyState.Idle, state);
        }

        [Test]
        [Description("Idle 상태에서 AttackRange 이하여도 Alert 를 먼저 반환해야 한다. (즉시 Attacking 불가)")]
        public void Idle_상태에서_AttackRange_이하여도_Alert_먼저_반환()
        {
            var state = _brain.Tick(EnemyState.Idle, distance: 1f, isAttacking: false);

            Assert.AreEqual(EnemyState.Alert, state);
        }
    }
}
