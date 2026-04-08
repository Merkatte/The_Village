using System.Collections.Generic;
using Game.Item;

namespace Game.Shop
{
    /// <summary>
    /// 상점 구입·판매 서비스 인터페이스입니다.
    ///
    /// ─ 동작 규칙 ──────────────────────────────────────────────
    ///   - TryBuy  : 골드 소비 → 아이템 인벤토리 추가. 인벤토리 가득 시 골드 환불.
    ///   - TrySell : 인벤토리 슬롯 제거 → 골드 추가.
    ///   - GetSellPrice : Value × SellPriceRatio (ShopConfig 기준)
    /// </summary>
    public interface IShopService
    {
        /// <summary>상점이 판매하는 아이템 목록</summary>
        IReadOnlyList<ShopItemInfo> ShopItems { get; }

        /// <summary>해당 아이템의 판매가를 반환합니다. item 이 null 이면 0을 반환합니다.</summary>
        int GetSellPrice(ItemData item);

        /// <summary>
        /// shopItemIndex 번 아이템을 구입합니다.
        /// </summary>
        ShopResult TryBuy(int shopItemIndex);

        /// <summary>
        /// inventorySlotIndex 슬롯의 아이템을 판매합니다.
        /// </summary>
        ShopResult TrySell(int inventorySlotIndex);
    }
}
