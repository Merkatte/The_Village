using System;
using System.Collections.Generic;
using NUnit.Framework;
using Game.Core.Enums;
using Game.Core.Interfaces;
using Game.Item;
using Game.Shop;
using Game.UI.Shop;

namespace Game.Tests.EditMode.UI
{
    /// <summary>
    /// ShopPresenter 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 생성자 null 인자 예외 검증
    ///   [v] Bind — null View 예외, 즉시 갱신
    ///   [v] Unbind — 이후 View 갱신 미호출
    ///   [v] OnBuyRequested — ShopService.TryBuy 위임
    ///   [v] Big/Small 슬롯 분리 — 아이템 수별 배분 검증
    ///   [v] OnCurrencyChanged — Gold 타입만 RefreshGold 호출
    ///   [v] Dispose — 이벤트 구독 해제
    /// </summary>
    [TestFixture]
    public class ShopPresenterTests
    {
        // ─ Fake ───────────────────────────────────────────────

        private sealed class FakeShopService : IShopService
        {
            private readonly List<ShopItemInfo> _items;

            public IReadOnlyList<ShopItemInfo> ShopItems => _items;

            public int LastBuyIndex { get; private set; } = -1;

            public FakeShopService(int itemCount = 0)
            {
                _items = new List<ShopItemInfo>();
                for (int i = 0; i < itemCount; i++)
                    _items.Add(new ShopItemInfo($"item_{i}", $"아이템{i}", 100));
            }

            public int  GetSellPrice(ItemData item) => 0;

            public ShopResult TryBuy(int idx)
            {
                LastBuyIndex = idx;
                return ShopResult.Success;
            }

            public ShopResult TrySell(int idx) => ShopResult.Success;
        }

        private sealed class FakeCurrencyService : ICurrencyService
        {
            public event Action<CurrencyType, int> OnCurrencyChanged;

            private int _gold;

            public int  Get(CurrencyType type)         => type == CurrencyType.Gold ? _gold : 0;
            public void Add(CurrencyType type, int a)  { }
            public bool TrySpend(CurrencyType t, int a){ return false; }

            public void SetGold(int g) => _gold = g;

            public void FireCurrencyChanged(CurrencyType type, int amount)
                => OnCurrencyChanged?.Invoke(type, amount);
        }

        private sealed class FakeSpriteRepository : ISpriteRepository
        {
            public UnityEngine.Sprite GetSprite(string itemId) => null;
        }

        private sealed class FakeShopView : IShopView
        {
            public int                 RefreshBigCallCount   { get; private set; }
            public int                 RefreshSmallCallCount { get; private set; }
            public int                 RefreshGoldCallCount  { get; private set; }
            public ShopSlotViewModel[] LastBigSlots          { get; private set; }
            public ShopSlotViewModel[] LastSmallSlots        { get; private set; }
            public int                 LastGold              { get; private set; }

            public void RefreshBigSlots(ShopSlotViewModel[] slots)
            {
                RefreshBigCallCount++;
                LastBigSlots = slots;
            }

            public void RefreshSmallSlots(ShopSlotViewModel[] slots)
            {
                RefreshSmallCallCount++;
                LastSmallSlots = slots;
            }

            public void RefreshGold(int gold)
            {
                RefreshGoldCallCount++;
                LastGold = gold;
            }
        }

        // ─ SetUp ──────────────────────────────────────────────

        private FakeCurrencyService  _currency;
        private FakeSpriteRepository _sprites;
        private FakeShopView         _view;

        [SetUp]
        public void SetUp()
        {
            _currency = new FakeCurrencyService();
            _sprites  = new FakeSpriteRepository();
            _view     = new FakeShopView();
        }

        private ShopPresenter MakePresenter(int itemCount = 0)
            => new ShopPresenter(new FakeShopService(itemCount), _currency, _sprites);

        // ─ 생성자 ─────────────────────────────────────────────

        [Test]
        [Description("shopService 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_shopService_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ShopPresenter(null, _currency, _sprites));
        }

        [Test]
        [Description("currencyService 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_currencyService_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ShopPresenter(new FakeShopService(), null, _sprites));
        }

        [Test]
        [Description("spriteRepo 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_spriteRepo_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ShopPresenter(new FakeShopService(), _currency, null));
        }

        // ─ Bind ───────────────────────────────────────────────

        [Test]
        [Description("Bind 에 null 을 전달하면 ArgumentNullException 이 발생해야 한다.")]
        public void Bind_null_View_전달시_ArgumentNullException_발생()
        {
            var presenter = MakePresenter();

            Assert.Throws<ArgumentNullException>(() => presenter.Bind(null));
        }

        [Test]
        [Description("Bind 호출 즉시 View 가 갱신되어야 한다.")]
        public void Bind_호출시_View가_즉시_갱신된다()
        {
            var presenter = MakePresenter();

            presenter.Bind(_view);

            Assert.Greater(_view.RefreshBigCallCount,  0);
            Assert.Greater(_view.RefreshGoldCallCount, 0);
        }

        // ─ Unbind ─────────────────────────────────────────────

        [Test]
        [Description("Unbind 후 통화 이벤트 발행 시 View 가 갱신되지 않아야 한다.")]
        public void Unbind_후_View_갱신이_호출되지_않는다()
        {
            var presenter = MakePresenter();
            presenter.Bind(_view);
            presenter.Unbind();
            int before = _view.RefreshGoldCallCount;

            _currency.FireCurrencyChanged(CurrencyType.Gold, 100);

            Assert.AreEqual(before, _view.RefreshGoldCallCount);
        }

        // ─ OnBuyRequested ─────────────────────────────────────

        [Test]
        [Description("OnBuyRequested 호출 시 ShopService.TryBuy 에 인덱스가 위임되어야 한다.")]
        public void OnBuyRequested_호출시_ShopService_TryBuy가_위임된다()
        {
            var shopSvc   = new FakeShopService(3);
            var presenter = new ShopPresenter(shopSvc, _currency, _sprites);

            presenter.OnBuyRequested(1);

            Assert.AreEqual(1, shopSvc.LastBuyIndex);
        }

        [Test]
        [Description("OnBuyRequested 반환값이 ShopService.TryBuy 반환값과 같아야 한다.")]
        public void OnBuyRequested_반환값이_TryBuy_결과와_같다()
        {
            var presenter = MakePresenter(1);

            var result = presenter.OnBuyRequested(0);

            Assert.AreEqual(ShopResult.Success, result);
        }

        // ─ Big / Small 슬롯 분리 ─────────────────────────────

        [Test]
        [Description("상품 수가 3 이하이면 BigSlots 에만 할당되어야 한다.")]
        public void 상품수_3이하이면_BigSlots에만_할당된다()
        {
            var presenter = MakePresenter(itemCount: 2);
            presenter.Bind(_view);

            Assert.AreEqual(2, _view.LastBigSlots.Length);
            Assert.AreEqual(0, _view.LastSmallSlots.Length);
        }

        [Test]
        [Description("상품 수가 4 이상이면 BigSlots 3개, 나머지는 SmallSlots 에 할당되어야 한다.")]
        public void 상품수_4이상이면_SmallSlots도_채워진다()
        {
            var presenter = MakePresenter(itemCount: 5);
            presenter.Bind(_view);

            Assert.AreEqual(3, _view.LastBigSlots.Length);
            Assert.AreEqual(2, _view.LastSmallSlots.Length);
        }

        [Test]
        [Description("BigSlots 는 최대 3개를 초과하지 않아야 한다.")]
        public void BigSlots는_최대_3개를_초과하지_않는다()
        {
            var presenter = MakePresenter(itemCount: 10);
            presenter.Bind(_view);

            Assert.AreEqual(3, _view.LastBigSlots.Length);
        }

        [Test]
        [Description("SmallSlots 는 최대 6개를 초과하지 않아야 한다.")]
        public void SmallSlots는_최대_6개를_초과하지_않는다()
        {
            var presenter = MakePresenter(itemCount: 10);
            presenter.Bind(_view);

            Assert.AreEqual(6, _view.LastSmallSlots.Length);
        }

        [Test]
        [Description("BigSlots 의 ViewModel SlotIndex 는 0, 1, 2 여야 한다.")]
        public void BigSlots_SlotIndex가_0부터_순서대로_설정된다()
        {
            var presenter = MakePresenter(itemCount: 4);
            presenter.Bind(_view);

            Assert.AreEqual(0, _view.LastBigSlots[0].SlotIndex);
            Assert.AreEqual(1, _view.LastBigSlots[1].SlotIndex);
            Assert.AreEqual(2, _view.LastBigSlots[2].SlotIndex);
        }

        [Test]
        [Description("SmallSlots 의 ViewModel SlotIndex 는 3 부터 시작되어야 한다.")]
        public void SmallSlots_SlotIndex가_3부터_시작된다()
        {
            var presenter = MakePresenter(itemCount: 5);
            presenter.Bind(_view);

            Assert.AreEqual(3, _view.LastSmallSlots[0].SlotIndex);
            Assert.AreEqual(4, _view.LastSmallSlots[1].SlotIndex);
        }

        // ─ OnCurrencyChanged ──────────────────────────────────

        [Test]
        [Description("Gold 타입 통화 변경 이벤트 발행 시 RefreshGold 가 호출되어야 한다.")]
        public void OnCurrencyChanged_Gold타입이면_RefreshGold가_호출된다()
        {
            var presenter = MakePresenter();
            presenter.Bind(_view);
            int before = _view.RefreshGoldCallCount;

            _currency.FireCurrencyChanged(CurrencyType.Gold, 500);

            Assert.Greater(_view.RefreshGoldCallCount, before);
        }

        [Test]
        [Description("Gold 외 타입의 통화 변경 이벤트 발행 시 RefreshGold 가 추가 호출되지 않아야 한다.")]
        public void OnCurrencyChanged_Gold외_타입은_RefreshGold가_호출되지_않는다()
        {
            var presenter = MakePresenter();
            presenter.Bind(_view);
            int before = _view.RefreshGoldCallCount;

            _currency.FireCurrencyChanged(CurrencyType.SkillPoint, 10);

            Assert.AreEqual(before, _view.RefreshGoldCallCount);
        }

        [Test]
        [Description("RefreshGold 에 전달되는 값이 CurrencyService.Get(Gold) 결과와 같아야 한다.")]
        public void RefreshGold에_올바른_잔액이_전달된다()
        {
            _currency.SetGold(300);
            var presenter = MakePresenter();
            presenter.Bind(_view);

            Assert.AreEqual(300, _view.LastGold);
        }

        // ─ Dispose ────────────────────────────────────────────

        [Test]
        [Description("Dispose 후 통화 이벤트 발행 시 RefreshGold 가 추가 호출되지 않아야 한다.")]
        public void Dispose_후_통화_이벤트_구독이_해제된다()
        {
            var presenter = MakePresenter();
            presenter.Bind(_view);
            presenter.Dispose();
            int before = _view.RefreshGoldCallCount;

            _currency.FireCurrencyChanged(CurrencyType.Gold, 100);

            Assert.AreEqual(before, _view.RefreshGoldCallCount);
        }
    }
}
