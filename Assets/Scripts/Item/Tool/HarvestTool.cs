using Game.Core.Enums;

namespace Game.Item.Tool
{
    /// <summary>
    /// 채취 도구 아이템 데이터입니다. (도끼, 곡괭이 등)
    ///
    /// - 던전에서 나무, 바위 등 자원 채취 시 사용됩니다.
    /// - ToolType 으로 채취 가능한 Harvestable 종류를 결정합니다.
    /// - HarvestDurationMultiplier 로 채취 홀드 시간을 단축합니다.
    /// - YieldMultiplier 로 획득 자원량을 증가시킵니다.
    ///
    /// ─ 도구 호환 규칙 ──────────────────────────────────────────
    ///   Axe     → HarvestableType.Tree (나무) 채취 가능
    ///   Pickaxe → HarvestableType.Ore  (광물) 채취 가능
    /// </summary>
    public sealed class HarvestTool : ItemData, ITool
    {
        /// <inheritdoc/>
        public ItemData Data => this;

        /// <summary>도구 종류. 채취 가능한 Harvestable 타입을 결정합니다.</summary>
        public ToolType ToolType { get; }

        /// <inheritdoc/>
        public float HarvestDurationMultiplier { get; }

        /// <inheritdoc/>
        public float YieldMultiplier { get; }

        /// <summary>
        /// HarvestTool 생성자.
        /// </summary>
        /// <param name="itemId">고유 식별자 (예: "axe_iron")</param>
        /// <param name="itemName">표시 이름 (예: "철 도끼")</param>
        /// <param name="grade">아이템 등급</param>
        /// <param name="toolType">도구 종류 (Axe: 나무, Pickaxe: 광물)</param>
        /// <param name="harvestDurationMultiplier">채취 시간 배율 (0.1 ~ 1.0)</param>
        /// <param name="yieldMultiplier">수확량 배율 (1.0 이상)</param>
        public HarvestTool(
            string    itemId,
            string    itemName,
            ItemGrade grade,
            ToolType  toolType,
            float     harvestDurationMultiplier = 1.0f,
            float     yieldMultiplier           = 1.0f)
            : base(itemId, itemName, grade)
        {
            ToolType                  = toolType;
            HarvestDurationMultiplier = UnityEngine.Mathf.Clamp(harvestDurationMultiplier, 0.1f, 1.0f);
            YieldMultiplier           = yieldMultiplier < 1.0f ? 1.0f : yieldMultiplier;
        }
    }
}
