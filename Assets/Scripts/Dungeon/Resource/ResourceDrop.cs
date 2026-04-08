namespace Game.Dungeon.Resource
{
    /// <summary>
    /// 자원 획득 결과를 나타내는 불변 데이터 구조체입니다.
    /// "어떤 자원이 몇 개 드롭됐는가" 를 표현합니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 순수 데이터 구조체 (Unity / MonoBehaviour 의존 없음)
    ///   - 불변(readonly) → 드롭 결과가 생성 후 변경되지 않음을 보장
    /// </summary>
    public readonly struct ResourceDrop
    {
        /// <summary>드롭된 자원 종류</summary>
        public Core.Enums.ResourceType Type { get; }

        /// <summary>드롭된 수량 (1 이상)</summary>
        public int Amount { get; }

        /// <summary>
        /// ResourceDrop 생성자.
        /// </summary>
        /// <param name="type">자원 종류</param>
        /// <param name="amount">수량 (1 이상이어야 합니다)</param>
        public ResourceDrop(Core.Enums.ResourceType type, int amount)
        {
            Type   = type;
            Amount = amount < 1 ? 1 : amount;  // 최소 1개 보장
        }
    }
}
