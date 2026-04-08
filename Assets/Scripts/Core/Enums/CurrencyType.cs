namespace Game.Core.Enums
{
    /// <summary>
    /// 게임 내 재화 종류를 정의합니다.
    /// CurrencyManager 에서 종류별로 독립적으로 관리합니다.
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>기본 거래 재화. 상점 구입/판매에 사용됩니다.</summary>
        Gold,

        /// <summary>캐릭터 성장에 사용되는 스킬 포인트입니다.</summary>
        SkillPoint,
    }
}
