using Game.Core.Enums;

namespace Game.Item.Catalog
{
    /// <summary>
    /// ItemCatalog CSV 한 행에 해당하는 불변 데이터 클래스입니다.
    ///
    /// ─ 공통 필드 ──────────────────────────────────────────────
    ///   ItemId, ItemName, ItemType, Grade, MaxStackSize
    ///
    /// ─ 타입별 선택 필드 ────────────────────────────────────────
    ///   Resource   : ResourceType
    ///   HarvestTool: ToolType, HarvestDurationMultiplier, YieldMultiplier
    ///   FarmTool   : ToolType
    ///   Weapon     : AttackPower
    ///   Armor      : DefensePower
    ///
    ///   해당 타입에서 사용하지 않는 필드는 null(참조형) 또는 0/1(기본값)입니다.
    /// </summary>
    public sealed class ItemCatalogEntry
    {
        // ─ 공통 ───────────────────────────────────────────────

        /// <summary>아이템 고유 식별자 (예: "wood", "axe_iron")</summary>
        public string    ItemId       { get; }

        /// <summary>표시 이름 (예: "나무", "철 도끼")</summary>
        public string    ItemName     { get; }

        /// <summary>아이템 종류</summary>
        public ItemType  ItemType     { get; }

        /// <summary>아이템 등급</summary>
        public ItemGrade Grade        { get; }

        /// <summary>최대 스택 수. 1 = 겹치지 않음</summary>
        public int       MaxStackSize { get; }

        /// <summary>아이템 기본 가치 (골드). 상점 매입/판매가 계산의 기준값입니다.</summary>
        public int       Value        { get; }

        // ─ 타입별 선택 필드 ────────────────────────────────────

        /// <summary>자원 종류. ItemType.Resource 일 때만 유효합니다.</summary>
        public ResourceType? ResourceType               { get; }

        /// <summary>도구 종류. HarvestTool / FarmTool 일 때만 유효합니다.</summary>
        public ToolType?     ToolType                   { get; }

        /// <summary>채취 시간 배율. HarvestTool 일 때만 유효합니다. (기본 1.0)</summary>
        public float         HarvestDurationMultiplier  { get; }

        /// <summary>수확량 배율. HarvestTool 일 때만 유효합니다. (기본 1.0)</summary>
        public float         YieldMultiplier            { get; }

        /// <summary>공격력. Weapon 일 때만 유효합니다. (기본 0)</summary>
        public int           AttackPower                { get; }

        /// <summary>방어력. Armor 일 때만 유효합니다. (기본 0)</summary>
        public int           DefensePower               { get; }

        public ItemCatalogEntry(
            string        itemId,
            string        itemName,
            ItemType      itemType,
            ItemGrade     grade,
            int           maxStackSize,
            int           value,
            ResourceType? resourceType              = null,
            ToolType?     toolType                  = null,
            float         harvestDurationMultiplier = 1f,
            float         yieldMultiplier           = 1f,
            int           attackPower               = 0,
            int           defensePower              = 0)
        {
            ItemId                    = itemId;
            ItemName                  = itemName;
            ItemType                  = itemType;
            Grade                     = grade;
            MaxStackSize              = maxStackSize < 1 ? 1 : maxStackSize;
            Value                     = value < 0 ? 0 : value;
            ResourceType              = resourceType;
            ToolType                  = toolType;
            HarvestDurationMultiplier = harvestDurationMultiplier;
            YieldMultiplier           = yieldMultiplier;
            AttackPower               = attackPower;
            DefensePower              = defensePower;
        }
    }
}
