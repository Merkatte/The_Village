namespace Game.Core.Enums
{
    /// <summary>
    /// 적의 종류를 정의합니다.
    ///
    /// ─ 확장 지침 ──────────────────────────────────────────────
    ///   새 적 추가 시 이 열거형에 값을 추가하고
    ///   EnemyData.csv 에 해당 행을 추가하세요.
    /// </summary>
    public enum EnemyType
    {
        /// <summary>슬라임 계열.</summary>
        Slime,

        /// <summary>고블린 계열.</summary>
        Goblin,

        /// <summary>오크 계열.</summary>
        Orc,
    }
}
