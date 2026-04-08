using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Game.Core.Enums;
using Game.Core.Interfaces;
using Game.Data.Config;
using Game.Inventory;
using Game.Item;
using Game.Item.Catalog;
using Game.Shop;

namespace Game.Tests.EditMode.Shop
{
    /// <summary>
    /// ShopService 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 생성자 null 인자 예외 검증
    ///   [v] ShopItems 목록 구성 (BuyPrice 계산, 카탈로그 누락 항목 제외)
    ///   [v] GetSellPrice — null·카탈로그 누락·정상·최솟값 1
    ///   [v] TryBuy — 범위 초과, 골드 부족, 성공, 인벤토리 가득(환불)
    ///   [v] TrySell — 빈 슬롯, 성공(아이템 제거·골드 획득)
    /// </summary>
    [TestFixture]
    public class ShopServiceTests
    {
        // ─ Fake ───────────────────────────────────────────────

        private sealed class FakeCurrencyService : ICurrencyService
        {
            public event Action<CurrencyType, int> OnCurrencyChanged;

            private readonly Dictionary<CurrencyType, int> _balances = new();

            public int  Get(CurrencyType type) => _balances.TryGetValue(type, out var v) ? v : 0;

            public void Add(CurrencyType type, int amount)
            {
                _balances[type] = Get(type) + amount;
                OnCurrencyChanged?.Invoke(type, _balances[type]);
            }

            public bool TrySpend(CurrencyType type, int amount)
            {
                if (Get(type) < amount) return false;
                _balances[type] = Get(type) - amount;
                OnCurrencyChanged?.Invoke(type, _balances[type]);
                return true;
            }

            /// <summary>테스트 헬퍼: 골드 잔액 강제 설정</summary>
            public void SetGold(int amount) => _balances[CurrencyType.Gold] = amount;
        }

        private sealed class FakeInventory : IInventory
        {
            public event Action OnInventoryChanged;

            private readonly ItemData[] _slots;

            public int SlotCount => _slots.Length;

            public IReadOnlyList<ItemData> Slots => _slots;

            public bool AddFails { get; set; } = false;

            public FakeInventory(int slotCount = 5)
            {
                _slots = new ItemData[slotCount];
            }

            public bool TryAddItem(ItemData item)
            {
                if (AddFails) return false;
                for (int i = 0; i < _slots.Length; i++)
                {
                    if (_slots[i] == null) { _slots[i] = item; OnInventoryChanged?.Invoke(); return true; }
                }
                return false;
            }

            public ItemData GetSlot(int index)   => _slots[index];
            public void RemoveAt(int index)      { _slots[index] = null; OnInventoryChanged?.Invoke(); }
            public void MoveItem(int from, int to) { (_slots[from], _slots[to]) = (_slots[to], _slots[from]); }

            /// <summary>테스트 헬퍼: 특정 슬롯에 아이템 배치</summary>
            public void SetSlot(int index, ItemData item) => _slots[index] = item;
        }

        // ─ 헬퍼 ──────────────────────────────────────────────

        private static ShopConfig CreateShopConfig(
            List<string> itemIds,
            float        buyRatio  = 1.0f,
            float        sellRatio = 0.5f)
        {
            var config = ScriptableObject.CreateInstance<ShopConfig>();
            typeof(ShopConfig)
                .GetField("_itemIds",        BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(config, itemIds);
            typeof(ShopConfig)
                .GetField("_buyPriceRatio",  BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(config, buyRatio);
            typeof(ShopConfig)
                .GetField("_sellPriceRatio", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(config, sellRatio);
            return config;
        }

        private static ItemCatalogEntry MakeWeaponEntry(string id = "sword", int value = 100)
            => new ItemCatalogEntry(id, "검", ItemType.Weapon, ItemGrade.Normal, 1, value, attackPower: 10);

        private static ItemCatalogEntry MakeResourceEntry(string id = "wood", int value = 20)
            => new ItemCatalogEntry(id, "나무", ItemType.Resource, ItemGrade.Normal, 99, value,
                resourceType: ResourceType.Wood);

        // ─ 생성자 ─────────────────────────────────────────────

        [Test]
        [Description("catalogRepo 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_catalogRepo_전달시_ArgumentNullException_발생()
        {
            var config = CreateShopConfig(new List<string>());
            Assert.Throws<ArgumentNullException>(() =>
                new ShopService(null, new FakeCurrencyService(), new FakeInventory(), config));
        }

        [Test]
        [Description("currencyService 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_currencyService_전달시_ArgumentNullException_발생()
        {
            var repo   = new ItemCatalogRepository(new List<ItemCatalogEntry>());
            var config = CreateShopConfig(new List<string>());
            Assert.Throws<ArgumentNullException>(() =>
                new ShopService(repo, null, new FakeInventory(), config));
        }

        [Test]
        [Description("inventory 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_inventory_전달시_ArgumentNullException_발생()
        {
            var repo   = new ItemCatalogRepository(new List<ItemCatalogEntry>());
            var config = CreateShopConfig(new List<string>());
            Assert.Throws<ArgumentNullException>(() =>
                new ShopService(repo, new FakeCurrencyService(), null, config));
        }

        [Test]
        [Description("config 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_config_전달시_ArgumentNullException_발생()
        {
            var repo = new ItemCatalogRepository(new List<ItemCatalogEntry>());
            Assert.Throws<ArgumentNullException>(() =>
                new ShopService(repo, new FakeCurrencyService(), new FakeInventory(), null));
        }

        // ─ ShopItems ──────────────────────────────────────────

        [Test]
        [Description("ShopItems 는 카탈로그에 존재하는 ItemId 만 포함되어야 한다.")]
        public void ShopItems_카탈로그에_없는_ItemId는_목록에서_제외된다()
        {
            var repo   = new ItemCatalogRepository(new List<ItemCatalogEntry> { MakeWeaponEntry("sword") });
            var config = CreateShopConfig(new List<string> { "sword", "nonexistent" });
            var svc    = new ShopService(repo, new FakeCurrencyService(), new FakeInventory(), config);

            Assert.AreEqual(1, svc.ShopItems.Count);
            Assert.AreEqual("sword", svc.ShopItems[0].ItemId);
        }

        [Test]
        [Description("ShopItems 의 BuyPrice 는 entry.Value × BuyPriceRatio 를 반올림한 값이어야 한다.")]
        public void ShopItems_BuyPrice는_Value_곱하기_BuyPriceRatio다()
        {
            var entry  = MakeWeaponEntry("sword", value: 100);
            var repo   = new ItemCatalogRepository(new List<ItemCatalogEntry> { entry });
            var config = CreateShopConfig(new List<string> { "sword" }, buyRatio: 2.0f);
            var svc    = new ShopService(repo, new FakeCurrencyService(), new FakeInventory(), config);

            Assert.AreEqual(200, svc.ShopItems[0].BuyPrice);
        }

        [Test]
        [Description("BuyPrice 는 최솟값이 1 이어야 한다.")]
        public void ShopItems_BuyPrice_최솟값은_1이다()
        {
            var entry  = MakeWeaponEntry("sword", value: 0);
            var repo   = new ItemCatalogRepository(new List<ItemCatalogEntry> { entry });
            var config = CreateShopConfig(new List<string> { "sword" }, buyRatio: 0f);
            var svc    = new ShopService(repo, new FakeCurrencyService(), new FakeInventory(), config);

            Assert.AreEqual(1, svc.ShopItems[0].BuyPrice);
        }

        // ─ GetSellPrice ───────────────────────────────────────

        [Test]
        [Description("null 아이템의 판매가는 0 이어야 한다.")]
        public void GetSellPrice_null_아이템은_0을_반환한다()
        {
            var svc = MakeSimpleService();

            Assert.AreEqual(0, svc.GetSellPrice(null));
        }

        [Test]
        [Description("카탈로그에 없는 아이템의 판매가는 0 이어야 한다.")]
        public void GetSellPrice_카탈로그에_없는_아이템은_0을_반환한다()
        {
            var repo   = new ItemCatalogRepository(new List<ItemCatalogEntry>());
            var config = CreateShopConfig(new List<string>());
            var svc    = new ShopService(repo, new FakeCurrencyService(), new FakeInventory(), config);

            var unknownItem = new FakeItem("unknown");

            Assert.AreEqual(0, svc.GetSellPrice(unknownItem));
        }

        [Test]
        [Description("판매가는 entry.Value × SellPriceRatio 를 반올림한 값이어야 한다.")]
        public void GetSellPrice_SellPriceRatio_적용된_가격을_반환한다()
        {
            var entry  = MakeWeaponEntry("sword", value: 100);
            var repo   = new ItemCatalogRepository(new List<ItemCatalogEntry> { entry });
            var config = CreateShopConfig(new List<string> { "sword" }, sellRatio: 0.5f);
            var svc    = new ShopService(repo, new FakeCurrencyService(), new FakeInventory(), config);

            var item = new FakeItem("sword");

            Assert.AreEqual(50, svc.GetSellPrice(item));
        }

        [Test]
        [Description("판매가 최솟값은 1 이어야 한다.")]
        public void GetSellPrice_최솟값은_1이다()
        {
            var entry  = MakeWeaponEntry("sword", value: 1);
            var repo   = new ItemCatalogRepository(new List<ItemCatalogEntry> { entry });
            var config = CreateShopConfig(new List<string> { "sword" }, sellRatio: 0f);
            var svc    = new ShopService(repo, new FakeCurrencyService(), new FakeInventory(), config);

            var item = new FakeItem("sword");

            Assert.AreEqual(1, svc.GetSellPrice(item));
        }

        // ─ TryBuy ─────────────────────────────────────────────

        [Test]
        [Description("범위를 벗어난 인덱스로 TryBuy 호출 시 InvalidItem 을 반환해야 한다.")]
        public void TryBuy_범위초과_인덱스는_InvalidItem을_반환한다()
        {
            var svc = MakeSimpleService();

            Assert.AreEqual(ShopResult.InvalidItem, svc.TryBuy(-1));
            Assert.AreEqual(ShopResult.InvalidItem, svc.TryBuy(999));
        }

        [Test]
        [Description("골드 부족 시 TryBuy 는 NotEnoughGold 를 반환하고 골드를 소비하지 않아야 한다.")]
        public void TryBuy_골드_부족시_NotEnoughGold_반환한다()
        {
            var entry    = MakeWeaponEntry("sword", value: 100);
            var repo     = new ItemCatalogRepository(new List<ItemCatalogEntry> { entry });
            var currency = new FakeCurrencyService();          // 잔액 0
            var config   = CreateShopConfig(new List<string> { "sword" }, buyRatio: 1.0f);
            var svc      = new ShopService(repo, currency, new FakeInventory(), config);

            var result = svc.TryBuy(0);

            Assert.AreEqual(ShopResult.NotEnoughGold, result);
            Assert.AreEqual(0, currency.Get(CurrencyType.Gold));
        }

        [Test]
        [Description("TryBuy 성공 시 Success 를 반환해야 한다.")]
        public void TryBuy_성공시_Success를_반환한다()
        {
            var (svc, currency, inv) = MakeServiceWithSword(gold: 200);

            var result = svc.TryBuy(0);

            Assert.AreEqual(ShopResult.Success, result);
        }

        [Test]
        [Description("TryBuy 성공 시 골드가 BuyPrice 만큼 소비되어야 한다.")]
        public void TryBuy_성공시_골드가_BuyPrice만큼_소비된다()
        {
            var (svc, currency, inv) = MakeServiceWithSword(gold: 200, buyRatio: 1.0f);

            svc.TryBuy(0);

            Assert.AreEqual(100, currency.Get(CurrencyType.Gold));
        }

        [Test]
        [Description("TryBuy 성공 시 아이템이 인벤토리에 추가되어야 한다.")]
        public void TryBuy_성공시_아이템이_인벤토리에_추가된다()
        {
            var (svc, _, inv) = MakeServiceWithSword(gold: 200);

            svc.TryBuy(0);

            Assert.IsNotNull(inv.GetSlot(0));
        }

        [Test]
        [Description("인벤토리 가득 시 TryBuy 는 InventoryFull 을 반환하고 골드를 환불해야 한다.")]
        public void TryBuy_인벤토리_가득시_골드_환불후_InventoryFull_반환한다()
        {
            var entry    = MakeWeaponEntry("sword", value: 100);
            var repo     = new ItemCatalogRepository(new List<ItemCatalogEntry> { entry });
            var currency = new FakeCurrencyService();
            currency.SetGold(200);
            var inv    = new FakeInventory();
            inv.AddFails = true;
            var config = CreateShopConfig(new List<string> { "sword" }, buyRatio: 1.0f);
            var svc    = new ShopService(repo, currency, inv, config);

            var result = svc.TryBuy(0);

            Assert.AreEqual(ShopResult.InventoryFull, result);
            Assert.AreEqual(200, currency.Get(CurrencyType.Gold)); // 환불됨
        }

        // ─ TrySell ────────────────────────────────────────────

        [Test]
        [Description("빈 슬롯을 판매하면 InvalidItem 을 반환해야 한다.")]
        public void TrySell_빈_슬롯은_InvalidItem을_반환한다()
        {
            var svc = MakeSimpleService();

            Assert.AreEqual(ShopResult.InvalidItem, svc.TrySell(0));
        }

        [Test]
        [Description("TrySell 성공 시 Success 를 반환해야 한다.")]
        public void TrySell_성공시_Success를_반환한다()
        {
            var entry    = MakeWeaponEntry("sword", value: 100);
            var repo     = new ItemCatalogRepository(new List<ItemCatalogEntry> { entry });
            var currency = new FakeCurrencyService();
            var inv      = new FakeInventory();
            inv.SetSlot(0, new FakeItem("sword"));
            var config = CreateShopConfig(new List<string>(), sellRatio: 0.5f);
            var svc    = new ShopService(repo, currency, inv, config);

            var result = svc.TrySell(0);

            Assert.AreEqual(ShopResult.Success, result);
        }

        [Test]
        [Description("TrySell 성공 시 인벤토리 슬롯이 비워져야 한다.")]
        public void TrySell_성공시_인벤토리_슬롯이_비워진다()
        {
            var entry    = MakeWeaponEntry("sword", value: 100);
            var repo     = new ItemCatalogRepository(new List<ItemCatalogEntry> { entry });
            var currency = new FakeCurrencyService();
            var inv      = new FakeInventory();
            inv.SetSlot(0, new FakeItem("sword"));
            var config = CreateShopConfig(new List<string>(), sellRatio: 0.5f);
            var svc    = new ShopService(repo, currency, inv, config);

            svc.TrySell(0);

            Assert.IsNull(inv.GetSlot(0));
        }

        [Test]
        [Description("TrySell 성공 시 판매가만큼 골드가 추가되어야 한다.")]
        public void TrySell_성공시_판매가만큼_골드가_추가된다()
        {
            var entry    = MakeWeaponEntry("sword", value: 100);
            var repo     = new ItemCatalogRepository(new List<ItemCatalogEntry> { entry });
            var currency = new FakeCurrencyService();
            var inv      = new FakeInventory();
            inv.SetSlot(0, new FakeItem("sword"));
            var config = CreateShopConfig(new List<string>(), sellRatio: 0.5f);
            var svc    = new ShopService(repo, currency, inv, config);

            svc.TrySell(0);

            Assert.AreEqual(50, currency.Get(CurrencyType.Gold));
        }

        // ─ 헬퍼 ──────────────────────────────────────────────

        private ShopService MakeSimpleService()
        {
            var repo   = new ItemCatalogRepository(new List<ItemCatalogEntry>());
            var config = CreateShopConfig(new List<string>());
            return new ShopService(repo, new FakeCurrencyService(), new FakeInventory(), config);
        }

        private (ShopService svc, FakeCurrencyService currency, FakeInventory inv)
            MakeServiceWithSword(int gold, float buyRatio = 1.0f)
        {
            var entry    = MakeWeaponEntry("sword", value: 100);
            var repo     = new ItemCatalogRepository(new List<ItemCatalogEntry> { entry });
            var currency = new FakeCurrencyService();
            currency.SetGold(gold);
            var inv    = new FakeInventory();
            var config = CreateShopConfig(new List<string> { "sword" }, buyRatio: buyRatio);
            var svc    = new ShopService(repo, currency, inv, config);
            return (svc, currency, inv);
        }

        // ─ 아이템 스텁 ────────────────────────────────────────

        private sealed class FakeItem : ItemData
        {
            public FakeItem(string itemId)
                : base(itemId, "FakeItem", ItemGrade.Normal) { }
        }
    }
}
