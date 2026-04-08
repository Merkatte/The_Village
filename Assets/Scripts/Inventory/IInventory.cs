using System;
using System.Collections.Generic;
using Game.Item;

namespace Game.Inventory
{
    /// <summary>
    /// 인벤토리 서비스 인터페이스입니다.
    ///
    /// ─ 슬롯 기반 설계 ────────────────────────────────────────────
    ///   인벤토리는 고정 크기의 슬롯 배열로 구성됩니다.
    ///   각 슬롯은 인덱스(0 ~ SlotCount-1)로 식별되며,
    ///   null 은 빈 슬롯을 의미합니다.
    ///
    ///   이 구조 덕분에 인벤토리를 열고 닫아도 아이템 위치가 항상 유지됩니다.
    ///   UI 드래그 등으로 위치를 바꾸려면 MoveItem 을 호출하세요.
    /// </summary>
    public interface IInventory
    {
        /// <summary>
        /// 슬롯 내용이 변경될 때 발행됩니다.
        /// InventoryPresenter 등 UI 레이어가 구독하여 화면을 갱신합니다.
        /// </summary>
        event Action OnInventoryChanged;
        /// <summary>인벤토리 전체 슬롯 수.</summary>
        int SlotCount { get; }

        /// <summary>
        /// 전체 슬롯 읽기 전용 목록.
        /// 비어 있는 슬롯은 null 로 표시됩니다.
        /// </summary>
        IReadOnlyList<ItemData> Slots { get; }

        /// <summary>
        /// 첫 번째 빈 슬롯에 아이템을 추가합니다.
        /// </summary>
        /// <param name="item">추가할 아이템 (null 불가)</param>
        /// <returns>추가 성공이면 true, 인벤토리가 가득 찼으면 false</returns>
        bool TryAddItem(ItemData item);

        /// <summary>
        /// 특정 슬롯의 아이템을 반환합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스 (0 ~ SlotCount-1)</param>
        /// <returns>해당 슬롯의 아이템. 비어 있으면 null</returns>
        ItemData GetSlot(int slotIndex);

        /// <summary>
        /// 특정 슬롯의 아이템을 제거합니다 (슬롯을 null 로 비웁니다).
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스 (0 ~ SlotCount-1)</param>
        void RemoveAt(int slotIndex);

        /// <summary>
        /// 두 슬롯의 아이템을 교환합니다.
        /// UI 드래그로 아이템 위치를 변경할 때 사용합니다.
        /// </summary>
        /// <param name="fromIndex">원본 슬롯 인덱스</param>
        /// <param name="toIndex">대상 슬롯 인덱스</param>
        void MoveItem(int fromIndex, int toIndex);
    }
}
