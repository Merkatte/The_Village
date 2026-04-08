using System;
using UnityEngine;

namespace Game.UI.Shop
{
    /// <summary>
    /// 상점 슬롯 하나를 표시하기 위한 불변 데이터입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 순수 C# 불변 객체입니다.
    ///   - ShopPresenter 가 생성하여 IShopView 로 전달합니다.
    ///   - 빈 슬롯은 ShopSlotViewModel.Empty 싱글턴을 재사용합니다.
    ///   - SlotIndex : Buy 탭에서는 ShopItems 인덱스, Sell 탭에서는 인벤토리 슬롯 인덱스.
    /// </summary>
    public sealed class ShopSlotViewModel
    {
        /// <summary>슬롯이 비어 있으면 true.</summary>
        public bool   IsEmpty   { get; }

        /// <summary>아이템 아이콘. 스프라이트 누락 시 null.</summary>
        public Sprite Icon      { get; }

        /// <summary>아이템 이름.</summary>
        public string ItemName  { get; }

        /// <summary>가격 표시 문자열 (구입가 또는 판매가).</summary>
        public string PriceText { get; }

        /// <summary>이 슬롯이 나타내는 인덱스. Buy=ShopItems 인덱스, Sell=인벤토리 슬롯 인덱스.</summary>
        public int    SlotIndex { get; }

        /// <summary>빈 슬롯을 나타내는 공유 인스턴스입니다.</summary>
        public static readonly ShopSlotViewModel Empty = new ShopSlotViewModel();

        private ShopSlotViewModel()
        {
            IsEmpty = true;
        }

        /// <param name="icon">아이템 아이콘 (null 허용)</param>
        /// <param name="itemName">아이템 이름</param>
        /// <param name="price">표시할 가격</param>
        /// <param name="slotIndex">슬롯 인덱스</param>
        public ShopSlotViewModel(Sprite icon, string itemName, int price, int slotIndex)
        {
            IsEmpty   = false;
            Icon      = icon;
            ItemName  = itemName ?? throw new ArgumentNullException(nameof(itemName));
            PriceText = $"{price}G";
            SlotIndex = slotIndex;
        }
    }
}
