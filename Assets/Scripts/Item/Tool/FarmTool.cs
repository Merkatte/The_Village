namespace Game.Item.Tool
{
    /// <summary>
    /// 농기구 아이템 데이터입니다. (낫, 물뿌리개 등)
    ///
    /// - 마을에서 농작물 수확 시 사용됩니다.
    /// - HarvestDurationMultiplier 로 수확 홀드 시간을 단축합니다.
    /// - YieldMultiplier 로 수확량을 증가시킵니다.
    ///
    /// ─ HarvestTool 과의 차이 ────────────────────────────────────
    ///   HarvestTool : 던전 자원(나무/바위) 채취 전용
    ///   FarmTool    : 마을 농작물 수확 전용
    ///   두 도구는 적용되는 씬과 대상이 다릅니다.
    /// </summary>
    public sealed class FarmTool : ItemData, ITool
    {
        /// <inheritdoc/>
        public ItemData Data => this;

        /// <inheritdoc/>
        public float HarvestDurationMultiplier { get; }

        /// <inheritdoc/>
        public float YieldMultiplier { get; }

        /// <summary>
        /// FarmTool 생성자.
        /// </summary>
        /// <param name="itemId">고유 식별자 (예: "sickle_basic")</param>
        /// <param name="itemName">표시 이름 (예: "기본 낫")</param>
        /// <param name="grade">아이템 등급</param>
        /// <param name="harvestDurationMultiplier">수확 시간 배율 (0.1 ~ 1.0)</param>
        /// <param name="yieldMultiplier">수확량 배율 (1.0 이상)</param>
        public FarmTool(
            string    itemId,
            string    itemName,
            ItemGrade grade,
            float     harvestDurationMultiplier = 1.0f,
            float     yieldMultiplier           = 1.0f)
            : base(itemId, itemName, grade)
        {
            HarvestDurationMultiplier = UnityEngine.Mathf.Clamp(harvestDurationMultiplier, 0.1f, 1.0f);
            YieldMultiplier           = yieldMultiplier < 1.0f ? 1.0f : yieldMultiplier;
        }
    }
}
