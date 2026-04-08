using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy.Data
{
    /// <summary>
    /// TextAsset 으로부터 IEnemyRepository 를 생성합니다.
    ///
    /// ─ 사용 방법 ──────────────────────────────────────────────
    ///   1. Assets/Resources/Data/ 에 EnemyData.csv 파일을 배치합니다.
    ///   2. EnemyCsvConfig 의 enemyCsv 필드에 연결합니다.
    ///   3. DungeonSceneInitializer 에서:
    ///
    ///      var repo = EnemyLoader.Load(enemyCsvConfig.enemyCsv);
    ///      ServiceLocator.Register&lt;IEnemyRepository&gt;(repo);
    /// </summary>
    public static class EnemyLoader
    {
        /// <summary>
        /// TextAsset 으로부터 IEnemyRepository 를 생성합니다. (메인 스레드 전용)
        /// </summary>
        public static IEnemyRepository Load(TextAsset csvAsset)
        {
            if (csvAsset == null)
            {
                Debug.LogWarning(
                    "[EnemyLoader] csvAsset 이 null 입니다. " +
                    "EnemyCsvConfig 의 enemyCsv 필드에 CSV 를 연결했는지 확인하세요.");

                return new EnemyRepository(new List<Core.EnemyData>());
            }

            return LoadFromText(csvAsset.text);
        }

        /// <summary>
        /// CSV 텍스트로부터 IEnemyRepository 를 생성합니다.
        /// TextAsset.text 를 메인 스레드에서 미리 추출한 뒤 백그라운드 스레드에서 호출 가능합니다.
        /// </summary>
        public static IEnemyRepository LoadFromText(string csvText)
        {
            if (string.IsNullOrWhiteSpace(csvText))
                return new EnemyRepository(new List<Core.EnemyData>());

            var parser = new CsvEnemyParser();
            // 백그라운드 스레드에서 호출될 수 있으므로 Debug.Log 는 메인 스레드 복귀 후 처리됩니다.
            var warnings = new System.Collections.Generic.List<(int line, string msg)>();
            parser.OnWarning = (line, msg) => warnings.Add((line, msg));

            var entries = parser.Parse(csvText);

            // 경고 로그는 호출자(DungeonSceneInitializer)가 메인 스레드 복귀 후 출력합니다.
            foreach (var (line, msg) in warnings)
                Debug.LogWarning($"[EnemyData CSV] 행 {line}: {msg}");

            Debug.Log($"[EnemyLoader] 로드 완료 — {entries.Count}개 적 등록.");

            return new EnemyRepository(entries);
        }
    }
}
