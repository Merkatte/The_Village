using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>
    /// 던전 씬 전용 CSV 파일 참조를 보관하는 ScriptableObject 입니다.
    ///
    /// ─ 배치 방법 ──────────────────────────────────────────────
    ///   Assets/Data/Config/ 에 에셋을 생성한 뒤,
    ///   DungeonSceneInitializer 의 [SerializeField] dungeonCsvConfig 에 연결합니다.
    ///   (메뉴: Assets > Create > Game/Config/Dungeon CSV Config)
    ///
    /// ─ 포함 데이터 ────────────────────────────────────────────
    ///   - spawnPointsCsv : 던전 내 자원 스폰 위치 / 확률 테이블
    /// </summary>
    [CreateAssetMenu(
        menuName = "Game/Config/Dungeon CSV Config",
        fileName = "DungeonCsvConfig")]
    public sealed class DungeonCsvConfig : ScriptableObject
    {
        [Header("스폰 지점")]
        [Tooltip("SpawnPoints.csv 파일을 여기에 연결합니다.")]
        public TextAsset spawnPointsCsv;
        
        [Header("드롭 정보")]
        [Tooltip("DropTables.csv 파일을 여기에 연결합니다.")]
        public TextAsset dropTablesCsv;
    }
}
