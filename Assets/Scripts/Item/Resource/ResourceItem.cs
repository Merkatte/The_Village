using System;
using Game.Core.Enums;
using Game.Core.Interfaces;
using Game.Item.Catalog;

namespace Game.Item.Resource
{
    /// <summary>
    /// 채취·드롭으로 얻는 자원 아이템입니다. (나무, 광석, 약초 등)
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - ItemCatalogEntry 를 통해 생성됩니다.
    ///     이름·등급·MaxStackSize·Value 는 모두 카탈로그에서 읽습니다.
    ///   - IStackable 을 구현하여 Inventory 의 자동 스택 합산 대상이 됩니다.
    /// </summary>
    public sealed class ResourceItem : ItemData, IStackable
    {
        /// <summary>자원 종류</summary>
        public ResourceType ResourceType { get; }

        /// <summary>현재 수량 (1 이상)</summary>
        public int Quantity { get; private set; }

        /// <inheritdoc/>
        public bool IsFull => Quantity >= MaxStackSize;

        /// <inheritdoc/>
        public override int MaxStackSize { get; }

        /// <inheritdoc/>
        public override int Value { get; }

        /// <param name="entry">ItemCatalog 에서 조회한 항목</param>
        /// <param name="quantity">초기 수량 (1 이상)</param>
        public ResourceItem(ItemCatalogEntry entry, int quantity)
            : base(entry.ItemId, entry.ItemName, entry.Grade)
        {
            if (!entry.ResourceType.HasValue)
                throw new ArgumentException(
                    $"[ResourceItem] '{entry.ItemId}' 카탈로그 항목에 ResourceType 이 없습니다.",
                    nameof(entry));

            ResourceType = entry.ResourceType.Value;
            MaxStackSize = entry.MaxStackSize;
            Value        = entry.Value;
            Quantity     = Math.Clamp(quantity, 1, MaxStackSize);
        }

        /// <inheritdoc/>
        public int AddQuantity(int amount)
        {
            if (amount <= 0) return 0;

            int space = MaxStackSize - Quantity;
            int toAdd = Math.Min(amount, space);
            Quantity += toAdd;
            return amount - toAdd;
        }

        /// <inheritdoc/>
        public void SetQuantity(int quantity)
        {
            Quantity = Math.Clamp(quantity, 1, MaxStackSize);
        }
    }
}
