namespace Game.UI.Shop
{
    /// <summary>
    /// 상점 UI View 인터페이스입니다.
    ///
    /// ─ 구조 ───────────────────────────────────────────────────
    ///   Big Sale Zone  : 대형 슬롯 (추천/특가 상품, 최대 3개)
    ///   Small Sale Zone: 소형 슬롯 (일반 상품, 최대 6개)
    ///   하단 정보      : 보유 골드 표시
    /// </summary>
    public interface IShopView
    {
        /// <summary>Big Sale Zone 슬롯 목록을 갱신합니다. (최대 3개)</summary>
        void RefreshBigSlots(ShopSlotViewModel[] slots);

        /// <summary>Small Sale Zone 슬롯 목록을 갱신합니다. (최대 6개)</summary>
        void RefreshSmallSlots(ShopSlotViewModel[] slots);

        /// <summary>보유 골드 표시를 갱신합니다.</summary>
        void RefreshGold(int gold);
    }
}
