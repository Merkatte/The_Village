using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Game.Core.Enums;
using Game.Core.Interfaces;
using Game.Inventory;
using Game.Item;
using Game.Item.Catalog;
using Game.Item.Resource;
using Game.Shop;
using Game.UI.Inventory;
using Game.UI.Sell;

namespace Game.Tests.EditMode.UI
{
    /// <summary>
    /// SellPresenter 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 생성자 null 인자 예외 검증
    ///   [v] Bind — null View 예외, 즉시 갱신
    ///   [v] Unbind — 이후 View 갱신 미호출
    ///   [v] ToggleSlot — 빈 슬롯 무시, 선택·해제 토글, View 갱신
    ///   [v] OnSellConfirmed — 빈 선택 무시, TrySell 호출, 높은 인덱스 우선, 선택 초기화
    ///   [v] OnInventoryChanged — 판매된 슬롯 선택 해제
    ///   [v] OnCurrencyChanged — Gold 타입만 RefreshGold 호출
    ///   [v] Dispose — 이벤트 구독 해제
    /// </summary>
    [TestFixture]
    public class SellPresenterTests
    {
        // ─ Fake ───────────────────────────────────────────────

        private sealed class FakeInventory : IInventory
        {
            public event Action OnInventoryChanged;

            private readonly ItemData[] _slots;

            public int SlotCount => _slots.Length;
            public IReadOnlyList<ItemData> Slots => _slots;

            public FakeInventory(int slotCount = 5)
            {
                _slots = new ItemData[slotCount];
            }

            public bool TryAddItem(ItemData item)  { return false; }
            public ItemData GetSlot(int index)     => _slots[index];
            public void RemoveAt(int index)        { _slots[index] = null; FireChanged(); }
            public void MoveItem(int from, int to) { }

            public void SetSlot(int index, ItemData item) => _slots[index] = item;

            public void FireChanged() => OnInventoryChanged?.Invoke();
        }

        private sealed class FakeCurrencyService : ICurrencyService
        {
            public event Action<CurrencyType, int> OnCurrencyChanged;

            private int _gold;

            public int  Get(CurrencyType type)       => type == CurrencyType.Gold ? _gold : 0;
            public void Add(CurrencyType type, int a){ if (type == CurrencyType.Gold) _gold += a; }
            public bool TrySpend(CurrencyType t, int a){ return false; }

            public void SetGold(int g) => _gold = g;

            public void FireCurrencyChanged(CurrencyType type, int amount)
                => OnCurrencyChanged?.Invoke(type, amount);
        }

        private sealed class FakeShopService : IShopService
        {
            public IReadOnlyList<ShopItemInfo> ShopItems => new List<ShopItemInfo>();

            public List<int> SoldIndices { get; } = new List<int>();

            public int  GetSellPrice(ItemData item)    => 10;
            public ShopResult TryBuy(int idx)          => ShopResult.Success;
            public ShopResult TrySell(int idx)
            {
                SoldIndices.Add(idx);
                return ShopResult.Success;
            }
        }

        private sealed class FakeSpriteRepository : ISpriteRepository
        {
            public UnityEngine.Sprite GetSprite(string itemId) => null;
        }

        private sealed class FakeSellView : ISellView
        {
            public int                        RefreshCallCount     { get; private set; }
            public int                        RefreshGoldCallCount { get; private set; }
            public int                        RefreshTotalCallCount { get; private set; }
            public int                        LastGold             { get; private set; }
            public int                        LastTotal            { get; private set; }
            public IReadOnlyCollection<int>   LastSelected         { get; private set; }

            public void Refresh(SlotViewModel[] slots, IReadOnlyCollection<int> selectedIndices)
            {
                RefreshCallCount++;
                LastSelected = selectedIndices;
            }

            public void RefreshGold(int gold)
            {
                RefreshGoldCallCount++;
                LastGold = gold;
            }

            public void RefreshSellTotal(int totalGold)
            {
                RefreshTotalCallCount++;
                LastTotal = totalGold;
            }
        }

        // ─ SetUp ──────────────────────────────────────────────

        private FakeInventory       _inventory;
        private FakeShopService     _shopSvc;
        private FakeCurrencyService _currency;
        private FakeSpriteRepository _sprites;
        private FakeSellView        _view;

        [SetUp]
        public void SetUp()
        {
            _inventory = new FakeInventory(5);
            _shopSvc   = new FakeShopService();
            _currency  = new FakeCurrencyService();
            _sprites   = new FakeSpriteRepository();
            _view      = new FakeSellView();
        }

        private SellPresenter MakePresenter()
            => new SellPresenter(_inventory, _shopSvc, _currency, _sprites);

        private static ItemData MakeDummyItem(string id = "wood")
        {
            var entry = new ItemCatalogEntry(id, "아이템", ItemType.Resource, ItemGrade.Normal, 99, 10,
                resourceType: ResourceType.Wood);
            return new ResourceItem(entry, 1);
        }

        // ─ 생성자 ─────────────────────────────────────────────

        [Test]
        [Description("inventory 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_inventory_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SellPresenter(null, _shopSvc, _currency, _sprites));
        }

        [Test]
        [Description("shopService 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_shopService_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SellPresenter(_inventory, null, _currency, _sprites));
        }

        [Test]
        [Description("currencyService 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_currencyService_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SellPresenter(_inventory, _shopSvc, null, _sprites));
        }

        [Test]
        [Description("spriteRepo 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_spriteRepo_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new SellPresenter(_inventory, _shopSvc, _currency, null));
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

            Assert.Greater(_view.RefreshCallCount, 0);
        }

        // ─ Unbind ─────────────────────────────────────────────

        [Test]
        [Description("Unbind 후 인벤토리 이벤트 발행 시 View 가 갱신되지 않아야 한다.")]
        public void Unbind_후_View_갱신이_호출되지_않는다()
        {
            var presenter = MakePresenter();
            presenter.Bind(_view);
            presenter.Unbind();
            int countBefore = _view.RefreshCallCount;

            _inventory.FireChanged();

            Assert.AreEqual(countBefore, _view.RefreshCallCount);
        }

        // ─ ToggleSlot ─────────────────────────────────────────

        [Test]
        [Description("빈 슬롯을 ToggleSlot 해도 선택 상태에 변화가 없어야 한다.")]
        public void ToggleSlot_빈_슬롯은_선택되지_않는다()
        {
            var presenter = MakePresenter();
            presenter.Bind(_view);

            presenter.ToggleSlot(0);

            Assert.IsFalse(_view.LastSelected != null && _view.LastSelected.Contains(0));
        }

        [Test]
        [Description("아이템이 있는 슬롯을 ToggleSlot 하면 선택 상태가 추가되어야 한다.")]
        public void ToggleSlot_아이템_슬롯을_토글하면_선택된다()
        {
            _inventory.SetSlot(0, MakeDummyItem());
            var presenter = MakePresenter();
            presenter.Bind(_view);

            presenter.ToggleSlot(0);

            Assert.IsTrue(_view.LastSelected != null && _view.LastSelected.Contains(0));
        }

        [Test]
        [Description("이미 선택된 슬롯을 ToggleSlot 하면 선택이 해제되어야 한다.")]
        public void ToggleSlot_선택된_슬롯을_다시_토글하면_선택이_해제된다()
        {
            _inventory.SetSlot(0, MakeDummyItem());
            var presenter = MakePresenter();
            presenter.Bind(_view);

            presenter.ToggleSlot(0);
            presenter.ToggleSlot(0);

            Assert.IsFalse(_view.LastSelected != null && _view.LastSelected.Contains(0));
        }

        [Test]
        [Description("ToggleSlot 호출 시 View.Refresh 가 호출되어야 한다.")]
        public void ToggleSlot_호출시_View가_갱신된다()
        {
            _inventory.SetSlot(0, MakeDummyItem());
            var presenter = MakePresenter();
            presenter.Bind(_view);
            int before = _view.RefreshCallCount;

            presenter.ToggleSlot(0);

            Assert.Greater(_view.RefreshCallCount, before);
        }

        // ─ OnSellConfirmed ────────────────────────────────────

        [Test]
        [Description("선택된 슬롯이 없으면 OnSellConfirmed 호출 시 TrySell 이 호출되지 않아야 한다.")]
        public void OnSellConfirmed_선택된_슬롯이_없으면_판매가_호출되지_않는다()
        {
            var presenter = MakePresenter();
            presenter.Bind(_view);

            presenter.OnSellConfirmed();

            Assert.AreEqual(0, _shopSvc.SoldIndices.Count);
        }

        [Test]
        [Description("선택된 슬롯의 TrySell 이 호출되어야 한다.")]
        public void OnSellConfirmed_선택된_슬롯을_판매하면_TrySell이_호출된다()
        {
            _inventory.SetSlot(2, MakeDummyItem());
            var presenter = MakePresenter();
            presenter.Bind(_view);
            presenter.ToggleSlot(2);

            presenter.OnSellConfirmed();

            Assert.Contains(2, _shopSvc.SoldIndices);
        }

        [Test]
        [Description("여러 슬롯 선택 시 높은 인덱스부터 판매되어야 한다.")]
        public void OnSellConfirmed_높은_인덱스부터_판매한다()
        {
            _inventory.SetSlot(1, MakeDummyItem("item_a"));
            _inventory.SetSlot(3, MakeDummyItem("item_b"));
            var presenter = MakePresenter();
            presenter.Bind(_view);
            presenter.ToggleSlot(1);
            presenter.ToggleSlot(3);

            presenter.OnSellConfirmed();

            Assert.AreEqual(2, _shopSvc.SoldIndices.Count);
            Assert.AreEqual(3, _shopSvc.SoldIndices[0]); // 높은 인덱스가 먼저
            Assert.AreEqual(1, _shopSvc.SoldIndices[1]);
        }

        [Test]
        [Description("OnSellConfirmed 후 선택 상태가 초기화되어야 한다.")]
        public void OnSellConfirmed_후_선택_상태가_초기화된다()
        {
            _inventory.SetSlot(0, MakeDummyItem());
            var presenter = MakePresenter();
            presenter.Bind(_view);
            presenter.ToggleSlot(0);

            presenter.OnSellConfirmed();

            Assert.IsFalse(_view.LastSelected != null && _view.LastSelected.Contains(0));
        }

        // ─ OnInventoryChanged ─────────────────────────────────

        [Test]
        [Description("판매되어 비워진 슬롯의 선택 상태가 자동으로 해제되어야 한다.")]
        public void OnInventoryChanged_판매된_슬롯의_선택상태가_제거된다()
        {
            _inventory.SetSlot(0, MakeDummyItem());
            var presenter = MakePresenter();
            presenter.Bind(_view);
            presenter.ToggleSlot(0);

            // 외부에서 슬롯 제거 후 이벤트 발행 (판매처럼 동작)
            _inventory.SetSlot(0, null);
            _inventory.FireChanged();

            Assert.IsFalse(_view.LastSelected != null && _view.LastSelected.Contains(0));
        }

        // ─ OnCurrencyChanged ──────────────────────────────────

        [Test]
        [Description("Gold 타입의 통화 변경 이벤트 발행 시 RefreshGold 가 호출되어야 한다.")]
        public void OnCurrencyChanged_Gold타입이면_RefreshGold가_호출된다()
        {
            var presenter = MakePresenter();
            presenter.Bind(_view);
            int before = _view.RefreshGoldCallCount;

            _currency.FireCurrencyChanged(CurrencyType.Gold, 100);

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

        // ─ Dispose ────────────────────────────────────────────

        [Test]
        [Description("Dispose 후 인벤토리 이벤트 발행 시 View 가 갱신되지 않아야 한다.")]
        public void Dispose_후_인벤토리_이벤트_구독이_해제된다()
        {
            var presenter = MakePresenter();
            presenter.Bind(_view);
            presenter.Dispose();
            int before = _view.RefreshCallCount;

            _inventory.FireChanged();

            Assert.AreEqual(before, _view.RefreshCallCount);
        }

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
