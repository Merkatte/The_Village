using System;

namespace Game.Shop
{
    /// <summary>
    /// 상점이 판매하는 아이템 한 항목의 불변 데이터입니다.
    /// ShopService 가 생성하여 UI 에 전달합니다.
    /// </summary>
    public sealed class ShopItemInfo
    {
        /// <summary>아이템 고유 식별자</summary>
        public string ItemId   { get; }

        /// <summary>표시 이름</summary>
        public string ItemName { get; }

        /// <summary>구입가 (골드)</summary>
        public int    BuyPrice { get; }

        public ShopItemInfo(string itemId, string itemName, int buyPrice)
        {
            ItemId   = itemId   ?? throw new ArgumentNullException(nameof(itemId));
            ItemName = itemName ?? throw new ArgumentNullException(nameof(itemName));
            BuyPrice = buyPrice < 0 ? 0 : buyPrice;
        }
    }
}
