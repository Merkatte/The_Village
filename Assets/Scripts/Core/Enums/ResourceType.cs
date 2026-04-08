namespace Game.Core.Enums
{
    /// <summary>
    /// 게임 내 존재하는 자원의 종류를 정의합니다.
    ///
    /// ─ 분류 ───────────────────────────────────────────────────
    ///   자연 채취 : Wood, Stone, Herb
    ///   광물 채취 : IronOre, GoldOre, Crystal
    ///   몬스터 드롭 : Bone, Leather, MagicEssence
    /// </summary>
    public enum ResourceType
    {
        // ─ 자연 채취 ──────────────────────────────────
        /// <summary>나무</summary>
        Wood,

        /// <summary>돌</summary>
        Stone,

        /// <summary>약초</summary>
        Herb,

        // ─ 광물 채취 ──────────────────────────────────
        /// <summary>철광석</summary>
        IronOre,

        /// <summary>금광석</summary>
        GoldOre,

        /// <summary>수정</summary>
        Crystal,

        // ─ 몬스터 드롭 ────────────────────────────────
        /// <summary>뼈</summary>
        Bone,

        /// <summary>가죽</summary>
        Leather,

        /// <summary>마력 정수</summary>
        MagicEssence,
    }
}
