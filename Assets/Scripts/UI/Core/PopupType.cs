namespace Game.UI.Core
{
    /// <summary>
    /// 팝업 UI 종류를 식별하는 열거형입니다.
    /// UIManager.Open / Close 호출 시 사용합니다.
    /// </summary>
    public enum PopupType
    {
        Inventory,
        Shop,
        Sell,
        DungeonEntry,
        PlayerDeath,
    }
}
