namespace Game.Shop
{
    /// <summary>
    /// 상점 구입·판매 작업의 결과를 나타냅니다.
    /// </summary>
    public enum ShopResult
    {
        /// <summary>성공적으로 처리되었습니다.</summary>
        Success,

        /// <summary>골드가 부족합니다.</summary>
        NotEnoughGold,

        /// <summary>인벤토리가 가득 찼습니다.</summary>
        InventoryFull,

        /// <summary>유효하지 않은 아이템 인덱스입니다.</summary>
        InvalidItem,
    }
}
