using System;
using System.Collections.Generic;
using Game.Core.Enums;
using Game.Core.Interfaces;
using Game.Data.Config;
using Game.Inventory;
using Game.Item;
using Game.Item.Catalog;
using UnityEngine;

namespace Game.Shop
{
    /// <summary>
    /// IShopService 구현체입니다.
    ///
    /// ─ 초기화 흐름 ────────────────────────────────────────────
    ///   TownSceneInitializer 가 생성자에 의존성을 주입한 뒤
    ///   ServiceLocator 에 IShopService 로 등록합니다.
    ///
    /// ─ 구입 흐름 ──────────────────────────────────────────────
    ///   골드 소비 → ItemFactory 로 아이템 생성 → 인벤토리 추가
    ///   인벤토리 가득 시 → 골드 환불 → InventoryFull 반환
    ///
    /// ─ 판매 흐름 ──────────────────────────────────────────────
    ///   인벤토리 슬롯 제거 → 판매가 골드 추가
    /// </summary>
    public sealed class ShopService : IShopService
    {
        private readonly IItemCatalogRepository _catalogRepo;
        private readonly ICurrencyService       _currencyService;
        private readonly IInventory             _inventory;
        private readonly ShopConfig             _config;
        private readonly List<ShopItemInfo>     _shopItems;

        /// <inheritdoc/>
        public IReadOnlyList<ShopItemInfo> ShopItems => _shopItems;

        public ShopService(
            IItemCatalogRepository catalogRepo,
            ICurrencyService       currencyService,
            IInventory             inventory,
            ShopConfig             config)
        {
            _catalogRepo     = catalogRepo     ?? throw new ArgumentNullException(nameof(catalogRepo));
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            _inventory       = inventory       ?? throw new ArgumentNullException(nameof(inventory));
            _config          = config          ?? throw new ArgumentNullException(nameof(config));

            _shopItems = BuildShopItems();
        }

        /// <inheritdoc/>
        public int GetSellPrice(ItemData item)
        {
            if (item == null) return 0;
            var entry = _catalogRepo.GetEntry(item.ItemId);
            if (entry == null) return 0;
            return Mathf.Max(1, Mathf.RoundToInt(entry.Value * _config.SellPriceRatio));
        }

        /// <inheritdoc/>
        public ShopResult TryBuy(int shopItemIndex)
        {
            if (shopItemIndex < 0 || shopItemIndex >= _shopItems.Count)
                return ShopResult.InvalidItem;

            var shopItem = _shopItems[shopItemIndex];
            var entry    = _catalogRepo.GetEntry(shopItem.ItemId);
            if (entry == null) return ShopResult.InvalidItem;

            if (!_currencyService.TrySpend(CurrencyType.Gold, shopItem.BuyPrice))
                return ShopResult.NotEnoughGold;

            var item = ItemFactory.Create(entry);
            if (_inventory.TryAddItem(item)) return ShopResult.Success;

            // 인벤토리 가득 — 골드 환불
            _currencyService.Add(CurrencyType.Gold, shopItem.BuyPrice);
            return ShopResult.InventoryFull;
        }

        /// <inheritdoc/>
        public ShopResult TrySell(int inventorySlotIndex)
        {
            var item = _inventory.GetSlot(inventorySlotIndex);
            if (item == null) return ShopResult.InvalidItem;

            int sellPrice = GetSellPrice(item);
            _inventory.RemoveAt(inventorySlotIndex);
            _currencyService.Add(CurrencyType.Gold, sellPrice);
            return ShopResult.Success;
        }

        // ─ 내부 ──────────────────────────────────────────────────

        private List<ShopItemInfo> BuildShopItems()
        {
            var list = new List<ShopItemInfo>();
            foreach (var itemId in _config.ItemIds)
            {
                var entry = _catalogRepo.GetEntry(itemId);
                if (entry == null)
                {
                    Debug.LogWarning($"[ShopService] '{itemId}' 를 ItemCatalog 에서 찾을 수 없습니다. 해당 항목을 건너뜁니다.");
                    continue;
                }
                int buyPrice = Mathf.Max(1, Mathf.RoundToInt(entry.Value * _config.BuyPriceRatio));
                list.Add(new ShopItemInfo(entry.ItemId, entry.ItemName, buyPrice));
            }
            return list;
        }
    }
}
