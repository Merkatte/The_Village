using System;
using NUnit.Framework;
using Game.Core.Enums;
using Game.Manager.Currency;

namespace Game.Tests.EditMode.Manager
{
    /// <summary>
    /// CurrencyManager 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 미등록 타입 조회 시 0 반환
    ///   [v] Add 정상 동작 및 누적 합산
    ///   [v] Add 유효성 검증 (0 이하)
    ///   [v] Add 이벤트 발행 및 인자 정확성
    ///   [v] TrySpend 정상 소비 및 잔액 변경
    ///   [v] TrySpend 잔액 부족 시 false 반환 및 잔액 유지
    ///   [v] TrySpend 유효성 검증 (0 이하)
    ///   [v] TrySpend 이벤트 발행 (성공/실패 분기)
    ///   [v] CurrencyType 별 독립 관리
    /// </summary>
    [TestFixture]
    public class CurrencyManagerTests
    {
        private CurrencyManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new CurrencyManager();
        }

        // ─ Get ────────────────────────────────────────────────

        [Test]
        [Description("미등록 타입은 0을 반환해야 한다.")]
        public void Get_미등록_타입은_0을_반환한다()
        {
            Assert.AreEqual(0, _manager.Get(CurrencyType.Gold));
        }

        [Test]
        [Description("Add 후 Get 은 추가된 금액을 반환해야 한다.")]
        public void Get_Add_후_추가된_잔액을_반환한다()
        {
            _manager.Add(CurrencyType.Gold, 100);

            Assert.AreEqual(100, _manager.Get(CurrencyType.Gold));
        }

        // ─ Add ────────────────────────────────────────────────

        [Test]
        [Description("Add 를 여러 번 호출하면 잔액이 누적 합산되어야 한다.")]
        public void Add_누적_호출시_잔액이_합산된다()
        {
            _manager.Add(CurrencyType.Gold, 100);
            _manager.Add(CurrencyType.Gold, 200);

            Assert.AreEqual(300, _manager.Get(CurrencyType.Gold));
        }

        [Test]
        [Description("Add 에 0 을 전달하면 ArgumentOutOfRangeException 이 발생해야 한다.")]
        public void Add_0은_예외가_발생한다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _manager.Add(CurrencyType.Gold, 0));
        }

        [Test]
        [Description("Add 에 음수를 전달하면 ArgumentOutOfRangeException 이 발생해야 한다.")]
        public void Add_음수는_예외가_발생한다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _manager.Add(CurrencyType.Gold, -1));
        }

        [Test]
        [Description("Add 성공 시 OnCurrencyChanged 이벤트가 발행되어야 한다.")]
        public void Add_성공시_OnCurrencyChanged_이벤트가_발행된다()
        {
            var eventFired = false;
            _manager.OnCurrencyChanged += (_, __) => eventFired = true;

            _manager.Add(CurrencyType.Gold, 100);

            Assert.IsTrue(eventFired);
        }

        [Test]
        [Description("Add 이벤트 인자에 변경된 타입과 변경 후 잔액이 전달되어야 한다.")]
        public void Add_이벤트에_올바른_타입과_잔액이_전달된다()
        {
            CurrencyType capturedType   = default;
            int          capturedAmount = 0;
            _manager.OnCurrencyChanged += (type, amount) => { capturedType = type; capturedAmount = amount; };

            _manager.Add(CurrencyType.Gold, 150);

            Assert.AreEqual(CurrencyType.Gold, capturedType);
            Assert.AreEqual(150, capturedAmount);
        }

        [Test]
        [Description("Add 를 두 번 호출하면 이벤트의 amount 인자는 누적 잔액이어야 한다.")]
        public void Add_이벤트_amount는_누적_잔액을_반영한다()
        {
            _manager.Add(CurrencyType.Gold, 100);

            int capturedAmount = 0;
            _manager.OnCurrencyChanged += (_, amount) => capturedAmount = amount;

            _manager.Add(CurrencyType.Gold, 50);

            Assert.AreEqual(150, capturedAmount);
        }

        // ─ TrySpend ───────────────────────────────────────────

        [Test]
        [Description("잔액이 충분하면 TrySpend 가 true 를 반환하고 잔액이 감소해야 한다.")]
        public void TrySpend_잔액이_충분하면_true를_반환하고_잔액이_감소한다()
        {
            _manager.Add(CurrencyType.Gold, 100);

            bool result = _manager.TrySpend(CurrencyType.Gold, 60);

            Assert.IsTrue(result);
            Assert.AreEqual(40, _manager.Get(CurrencyType.Gold));
        }

        [Test]
        [Description("잔액과 소비량이 동일하면 TrySpend 가 true 를 반환하고 잔액이 0 이 되어야 한다.")]
        public void TrySpend_잔액과_소비량이_같으면_true를_반환하고_잔액이_0이_된다()
        {
            _manager.Add(CurrencyType.Gold, 100);

            bool result = _manager.TrySpend(CurrencyType.Gold, 100);

            Assert.IsTrue(result);
            Assert.AreEqual(0, _manager.Get(CurrencyType.Gold));
        }

        [Test]
        [Description("잔액이 부족하면 TrySpend 가 false 를 반환하고 잔액이 유지되어야 한다.")]
        public void TrySpend_잔액이_부족하면_false를_반환하고_잔액이_유지된다()
        {
            _manager.Add(CurrencyType.Gold, 50);

            bool result = _manager.TrySpend(CurrencyType.Gold, 100);

            Assert.IsFalse(result);
            Assert.AreEqual(50, _manager.Get(CurrencyType.Gold));
        }

        [Test]
        [Description("미등록(잔액 0) 타입에서 TrySpend 를 호출하면 false 를 반환해야 한다.")]
        public void TrySpend_미등록_타입은_false를_반환한다()
        {
            bool result = _manager.TrySpend(CurrencyType.Gold, 1);

            Assert.IsFalse(result);
        }

        [Test]
        [Description("TrySpend 에 0 을 전달하면 ArgumentOutOfRangeException 이 발생해야 한다.")]
        public void TrySpend_0은_예외가_발생한다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _manager.TrySpend(CurrencyType.Gold, 0));
        }

        [Test]
        [Description("TrySpend 에 음수를 전달하면 ArgumentOutOfRangeException 이 발생해야 한다.")]
        public void TrySpend_음수는_예외가_발생한다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _manager.TrySpend(CurrencyType.Gold, -1));
        }

        [Test]
        [Description("TrySpend 성공 시 OnCurrencyChanged 이벤트가 발행되어야 한다.")]
        public void TrySpend_성공시_OnCurrencyChanged_이벤트가_발행된다()
        {
            _manager.Add(CurrencyType.Gold, 100);

            var eventFired = false;
            _manager.OnCurrencyChanged += (_, __) => eventFired = true;

            _manager.TrySpend(CurrencyType.Gold, 50);

            Assert.IsTrue(eventFired);
        }

        [Test]
        [Description("TrySpend 실패(잔액 부족) 시 OnCurrencyChanged 이벤트가 발행되지 않아야 한다.")]
        public void TrySpend_실패시_이벤트가_발행되지_않는다()
        {
            var eventFired = false;
            _manager.OnCurrencyChanged += (_, __) => eventFired = true;

            _manager.TrySpend(CurrencyType.Gold, 100);

            Assert.IsFalse(eventFired);
        }

        [Test]
        [Description("TrySpend 이벤트 인자에 변경된 타입과 변경 후 잔액이 전달되어야 한다.")]
        public void TrySpend_이벤트에_올바른_타입과_잔액이_전달된다()
        {
            _manager.Add(CurrencyType.Gold, 100);

            CurrencyType capturedType   = default;
            int          capturedAmount = -1;
            _manager.OnCurrencyChanged += (type, amount) => { capturedType = type; capturedAmount = amount; };

            _manager.TrySpend(CurrencyType.Gold, 60);

            Assert.AreEqual(CurrencyType.Gold, capturedType);
            Assert.AreEqual(40, capturedAmount);
        }

        // ─ CurrencyType 독립성 ────────────────────────────────

        [Test]
        [Description("서로 다른 CurrencyType 의 잔액은 독립적으로 관리되어야 한다.")]
        public void CurrencyType별_잔액이_독립적으로_관리된다()
        {
            _manager.Add(CurrencyType.Gold,       100);
            _manager.Add(CurrencyType.SkillPoint, 50);

            Assert.AreEqual(100, _manager.Get(CurrencyType.Gold));
            Assert.AreEqual(50,  _manager.Get(CurrencyType.SkillPoint));
        }

        [Test]
        [Description("Gold 소비가 SkillPoint 잔액에 영향을 주지 않아야 한다.")]
        public void TrySpend_Gold소비가_SkillPoint에_영향을_주지_않는다()
        {
            _manager.Add(CurrencyType.Gold,       100);
            _manager.Add(CurrencyType.SkillPoint, 50);

            _manager.TrySpend(CurrencyType.Gold, 100);

            Assert.AreEqual(0,  _manager.Get(CurrencyType.Gold));
            Assert.AreEqual(50, _manager.Get(CurrencyType.SkillPoint));
        }
    }
}
