using System.Collections.Generic;

namespace Game.Dungeon.Resource
{
    /// <summary>
    /// 드롭 테이블 저장소 인터페이스입니다.
    /// "어떤 소스(채취물/몬스터)에서 어떤 드롭 테이블을 가져오는가" 를 정의합니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 데이터 로딩 방식(CSV, ScriptableObject 등)을 외부에서 교체 가능 (DIP)
    ///   - 테스트에서는 FakeDropTableRepository 로 교체하여 파일 의존성을 제거
    /// </summary>
    public interface IDropTableRepository
    {
        /// <summary>
        /// sourceId 에 해당하는 IDropTable 을 반환합니다.
        /// </summary>
        /// <param name="sourceId">채취 오브젝트 또는 몬스터의 ID (예: "Tree", "GoblinSlime")</param>
        /// <returns>드롭 테이블. 없으면 null.</returns>
        IDropTable GetTable(string sourceId);

        /// <summary>
        /// 저장소에 등록된 모든 sourceId 목록을 반환합니다.
        /// </summary>
        IReadOnlyList<string> GetAllSourceIds();
    }
}
