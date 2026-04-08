namespace Game.Core.Enums
{
    /// <summary>
    /// 게임 전체 흐름 상태를 정의합니다.
    /// None    : 초기화 전 혹은 미정 상태
    /// Town    : 마을 씬이 활성화된 상태
    /// Dungeon : 던전 씬이 활성화된 상태
    /// </summary>
    public enum GameState
    {
        None,
        Town,
        Dungeon
    }
}
