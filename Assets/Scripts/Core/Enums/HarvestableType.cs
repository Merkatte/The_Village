namespace Game.Core.Enums
{
    /// <summary>
    /// 채취 가능한 오브젝트의 종류를 정의합니다.
    /// 장착 도구의 종류와 일치해야 채취할 수 있습니다.
    ///
    /// ─ 도구 호환 규칙 ──────────────────────────────────────────
    ///   Tree → 도끼(Axe) 필요
    ///   Ore  → 곡괭이(Pickaxe) 필요
    /// </summary>
    public enum HarvestableType
    {
        /// <summary>나무 오브젝트 — 도끼(Axe)가 필요합니다.</summary>
        Tree,

        /// <summary>광물 오브젝트 — 곡괭이(Pickaxe)가 필요합니다.</summary>
        Ore,
    }
}
