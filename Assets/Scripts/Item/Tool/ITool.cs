namespace Game.Item.Tool
{
    /// <summary>
    /// 도구 아이템 인터페이스입니다.
    /// 채취도구(HarvestTool)와 농기구(FarmTool)가 이 인터페이스를 구현합니다.
    ///
    /// ─ 보정치 의미 ──────────────────────────────────────────────
    ///   HarvestDurationMultiplier : 1.0 = 기본, 0.5 = 채취 시간 절반
    ///   YieldMultiplier           : 1.0 = 기본, 1.5 = 수확량 50% 증가
    /// </summary>
    public interface ITool
    {
        /// <summary>도구 기반 데이터</summary>
        ItemData Data { get; }

        /// <summary>
        /// 채취/수확 홀드 시간 배율입니다.
        /// 1.0이 기본값이며, 값이 낮을수록 빠르게 완료됩니다.
        /// </summary>
        float HarvestDurationMultiplier { get; }

        /// <summary>
        /// 수확량 배율입니다.
        /// 1.0이 기본값이며, 값이 높을수록 더 많이 획득합니다.
        /// </summary>
        float YieldMultiplier { get; }
    }
}
