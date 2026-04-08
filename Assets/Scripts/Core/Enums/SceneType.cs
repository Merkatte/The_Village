namespace Game.Core.Enums
{
    /// <summary>
    /// 게임 내 씬의 종류를 정의합니다.
    /// Bootstrap : 게임 진입점 씬 (서비스 초기화)
    /// Town      : 마을 씬 (건설, 동료 모집)
    /// Dungeon   : 던전 씬 (전투, 자원 수집)
    /// </summary>
    public enum SceneType
    {
        Bootstrap,
        Town,
        Dungeon,

        /// <summary>씬 전환 중간에 표시되는 로딩 씬</summary>
        Loading
    }
}
