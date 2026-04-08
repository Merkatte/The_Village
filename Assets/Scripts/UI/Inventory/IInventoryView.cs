namespace Game.UI.Inventory
{
    /// <summary>
    /// 인벤토리 UI 뷰 인터페이스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   InventoryPresenter 가 이 인터페이스만 알도록 하여
    ///   Presenter 를 Unity 에 비의존적인 순수 C# 로 유지합니다.
    ///   → InventoryPresenter 의 EditMode 단위 테스트 가능
    /// </summary>
    public interface IInventoryView
    {
        /// <summary>
        /// 전체 슬롯을 갱신합니다.
        /// IInventory.OnInventoryChanged 발행 시 Presenter 가 호출합니다.
        /// </summary>
        /// <param name="slots">슬롯 수만큼의 ViewModel 배열</param>
        void Refresh(SlotViewModel[] slots);
    }
}
