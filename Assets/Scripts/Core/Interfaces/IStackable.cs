namespace Game.Core.Interfaces
{
    /// <summary>
    /// 인벤토리 내에서 수량을 겹칠 수 있는 아이템 인터페이스입니다.
    ///
    /// ─ 동작 규칙 ──────────────────────────────────────────────
    ///   - MaxStackSize > 1 인 ItemData 가 이 인터페이스를 구현합니다.
    ///   - Inventory.TryAddItem 에서 IStackable 여부를 감지하여
    ///     기존 슬롯에 자동으로 수량을 합산합니다.
    /// </summary>
    public interface IStackable
    {
        /// <summary>현재 수량</summary>
        int Quantity { get; }

        /// <summary>슬롯이 가득 찼는지 여부 (Quantity >= MaxStackSize)</summary>
        bool IsFull { get; }

        /// <summary>
        /// 수량을 추가합니다. MaxStackSize 를 초과하는 분은 반환합니다.
        /// </summary>
        /// <param name="amount">추가할 수량</param>
        /// <returns>추가하지 못한 잔여 수량 (0이면 전부 추가됨)</returns>
        int AddQuantity(int amount);

        /// <summary>
        /// 수량을 직접 설정합니다.
        /// Inventory 가 빈 슬롯에 배치하기 전 잔여 수량으로 조정할 때 사용합니다.
        /// </summary>
        void SetQuantity(int quantity);
    }
}
