using System;
using Game.Core.Enums;
using Game.Item.Catalog;
using Game.Item.Equipment;
using Game.Item.Resource;
using Game.Item.Tool;

namespace Game.Item
{
    /// <summary>
    /// ItemCatalogEntry 로부터 적절한 ItemData 구현체를 생성하는 정적 팩토리입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - ItemType 에 따라 구체 타입(ResourceItem, Weapon 등)을 결정합니다.
    ///   - 호출 측은 ItemData 인터페이스로만 결과를 수령합니다.
    ///   - quantity 는 Resource 아이템에만 유의미합니다. 나머지 타입은 무시됩니다.
    ///
    /// ─ 사용처 ─────────────────────────────────────────────────
    ///   ShopService.TryBuy() 에서 구입한 아이템을 인벤토리에 추가할 때 사용합니다.
    /// </summary>
    public static class ItemFactory
    {
        /// <summary>
        /// 카탈로그 항목으로부터 ItemData 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="entry">생성 기준이 되는 카탈로그 항목</param>
        /// <param name="quantity">Resource 타입의 초기 수량 (기본값 1)</param>
        /// <exception cref="ArgumentNullException">entry 가 null 인 경우</exception>
        /// <exception cref="InvalidOperationException">필수 선택 필드가 없는 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">알 수 없는 ItemType 인 경우</exception>
        public static ItemData Create(ItemCatalogEntry entry, int quantity = 1)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry),
                "[ItemFactory] entry 가 null 입니다.");

            return entry.ItemType switch
            {
                ItemType.Resource => new ResourceItem(entry, quantity),

                ItemType.HarvestTool => new HarvestTool(
                    entry.ItemId,
                    entry.ItemName,
                    entry.Grade,
                    entry.ToolType ?? throw new InvalidOperationException(
                        $"[ItemFactory] '{entry.ItemId}' 에 ToolType 이 없습니다."),
                    entry.HarvestDurationMultiplier,
                    entry.YieldMultiplier),

                ItemType.FarmTool => new FarmTool(
                    entry.ItemId,
                    entry.ItemName,
                    entry.Grade,
                    entry.HarvestDurationMultiplier,
                    entry.YieldMultiplier),

                ItemType.Weapon => new Weapon(
                    entry.ItemId,
                    entry.ItemName,
                    entry.Grade,
                    entry.AttackPower),

                ItemType.Armor => new Armor(
                    entry.ItemId,
                    entry.ItemName,
                    entry.Grade,
                    entry.DefensePower),

                _ => throw new ArgumentOutOfRangeException(nameof(entry.ItemType),
                    $"[ItemFactory] 알 수 없는 ItemType: {entry.ItemType}")
            };
        }
    }
}
