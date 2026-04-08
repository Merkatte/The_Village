using System;
using System.Collections.Generic;
using Game.Core.Interfaces;
using Game.Item;
using UnityEngine;

namespace Game.Inventory
{
    /// <summary>
    /// IInventory 구현체입니다.
    ///
    /// ─ 내부 구조 ─────────────────────────────────────────────────
    ///   ItemData 배열(_slots)로 슬롯을 관리합니다.
    ///   배열 인덱스가 곧 슬롯 위치이므로, 인벤토리를 열고 닫아도
    ///   별도 정렬 없이 아이템 위치가 항상 유지됩니다.
    ///
    /// ─ 스택 합산 ─────────────────────────────────────────────────
    ///   IStackable 을 구현한 아이템(ResourceItem 등)이 추가되면
    ///   동일 ItemId 의 기존 슬롯에 수량을 먼저 채우고,
    ///   남는 수량은 새 슬롯에 배치합니다.
    /// </summary>
    public sealed class Inventory : IInventory
    {
        /// <inheritdoc/>
        public event Action OnInventoryChanged;

        private readonly ItemData[] _slots;

        public int SlotCount => _slots.Length;

        public IReadOnlyList<ItemData> Slots => _slots;

        /// <param name="slotCount">슬롯 수 (기본값 20)</param>
        public Inventory(int slotCount = 20)
        {
            if (slotCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(slotCount),
                    "[Inventory] 슬롯 수는 1 이상이어야 합니다.");

            _slots = new ItemData[slotCount];
        }

        /// <inheritdoc/>
        public bool TryAddItem(ItemData item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item),
                    "[Inventory] null 아이템은 추가할 수 없습니다.");

            bool added = item is IStackable stackable && item.MaxStackSize > 1
                ? TryAddStackable(item, stackable)
                : TryAddToEmptySlot(item);
            
            if (added) OnInventoryChanged?.Invoke();
            return added;
        }

        public ItemData GetSlot(int slotIndex)
        {
            ValidateIndex(slotIndex);
            return _slots[slotIndex];
        }

        public void RemoveAt(int slotIndex)
        {
            ValidateIndex(slotIndex);
            _slots[slotIndex] = null;
            OnInventoryChanged?.Invoke();
        }

        public void MoveItem(int fromIndex, int toIndex)
        {
            ValidateIndex(fromIndex);
            ValidateIndex(toIndex);
            (_slots[toIndex], _slots[fromIndex]) = (_slots[fromIndex], _slots[toIndex]);
            OnInventoryChanged?.Invoke();
        }

        // ─ 내부 ──────────────────────────────────────────────────

        /// <summary>
        /// 스택 가능 아이템 추가 흐름:
        ///   1. 동일 ItemId 의 비어있지 않은 슬롯에 수량을 채웁니다.
        ///   2. 잔여 수량이 남으면 빈 슬롯에 배치합니다.
        ///   3. 빈 슬롯조차 없으면 false 를 반환합니다.
        /// </summary>
        private bool TryAddStackable(ItemData item, IStackable stackable)
        {
            int remaining = stackable.Quantity;

            // 1단계: 동일 타입의 기존 슬롯에 수량 합산
            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (_slots[i] == null) continue;
                if (_slots[i].ItemId != item.ItemId) continue;
                if (_slots[i] is not IStackable existing || existing.IsFull) continue;

                remaining = existing.AddQuantity(remaining);
            }

            if (remaining <= 0) return true;

            // 2단계: 잔여분을 빈 슬롯에 배치
            stackable.SetQuantity(remaining);
            return TryAddToEmptySlot(item);
        }

        private bool TryAddToEmptySlot(ItemData item)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null)
                {
                    _slots[i] = item;
                    return true;
                }
            }

            return false;
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= _slots.Length)
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"[Inventory] 슬롯 인덱스 {index}는 유효 범위(0~{_slots.Length - 1})를 벗어났습니다.");
        }
    }
}
