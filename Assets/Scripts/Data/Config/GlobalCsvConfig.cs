using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>
    /// 게임 전역에서 사용하는 CSV 파일 참조를 보관하는 ScriptableObject 입니다.
    ///
    /// ─ 배치 방법 ──────────────────────────────────────────────
    ///   Assets/Data/Config/ 에 에셋을 생성한 뒤,
    ///   GameBootstrap 의 [SerializeField] globalCsvConfig 에 연결합니다.
    ///   (메뉴: Assets > Create > Game/Config/Global CSV Config)
    ///
    /// ─ 포함 데이터 ────────────────────────────────────────────
    ///   - itemCatalogCsv : 전체 아이템 정의 (이름·등급·스택·타입별 수치)
    ///   - dropTableCsv   : 채취 오브젝트 / 몬스터 드롭 테이블
    /// </summary>
    [CreateAssetMenu(
        menuName = "Game/Config/Global CSV Config",
        fileName = "GlobalCsvConfig")]
    public sealed class GlobalCsvConfig : ScriptableObject
    {
        [Header("아이템 카탈로그")]
        [Tooltip("ItemCatalog.csv 파일을 여기에 연결합니다.")]
        public TextAsset itemCatalogCsv;

        [Header("드롭 테이블")]
        [Tooltip("DropTable.csv 파일을 여기에 연결합니다.")]
        public TextAsset dropTableCsv;
    }
}
