namespace Game.Core.Enums
{
    /// <summary>
    /// 아이템 종류를 구분하는 열거형입니다.
    /// ItemCatalog CSV 의 ItemType 컬럼에서 사용됩니다.
    /// </summary>
    public enum ItemType
    {
        /// <summary>채취·드롭으로 얻는 소재 자원 (나무, 광석 등)</summary>
        Resource,

        /// <summary>채취 도구 (도끼, 곡괭이)</summary>
        HarvestTool,

        /// <summary>농기구 (낫, 물뿌리개)</summary>
        FarmTool,

        /// <summary>무기</summary>
        Weapon,

        /// <summary>방어구</summary>
        Armor,
    }
}
