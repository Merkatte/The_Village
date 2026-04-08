using System.Collections.Generic;
using UnityEngine;
using Game.Dungeon.Spawn;

namespace Game.Dungeon.Data
{
    /// <summary>
    /// CSV 텍스트 에셋을 로드하여 ISpawnPointRepository 를 제공합니다.
    ///
    /// ─ 사용 방법 ──────────────────────────────────────────────
    ///   동기(메인 스레드):   SpawnPointLoader.Load(textAsset)
    ///   비동기(백그라운드):  SpawnPointLoader.LoadFromText(textAsset.text)
    ///                        → Task.Run / UniTask.SwitchToThreadPool 컨텍스트에서 호출
    /// </summary>
    public static class SpawnPointLoader
    {
        /// <summary>
        /// TextAsset 으로부터 ISpawnPointRepository 를 생성합니다.
        /// </summary>
        /// <param name="csvAsset">Inspector 에서 연결된 TextAsset. null 이면 빈 저장소를 반환합니다.</param>
        public static ISpawnPointRepository Load(TextAsset csvAsset)
        {
            if (csvAsset == null)
            {
                Debug.LogWarning(
                    "[SpawnPointLoader] csvAsset 이 null 입니다. " +
                    "DungeonCsvConfig 의 spawnPointsCsv 필드에 CSV 를 연결했는지 확인하세요.");

                return new SpawnPointRepository(new Dictionary<string, SpawnPointData>());
            }

            return LoadFromText(csvAsset.text);
        }

        /// <summary>
        /// CSV 문자열로부터 ISpawnPointRepository 를 생성합니다.
        /// TextAsset.text 는 메인 스레드에서 미리 추출한 뒤 이 메서드를 백그라운드 스레드에서 호출하세요.
        /// </summary>
        /// <param name="csvText">파싱할 CSV 문자열. null 또는 빈 값이면 빈 저장소를 반환합니다.</param>
        public static ISpawnPointRepository LoadFromText(string csvText)
        {
            if (string.IsNullOrEmpty(csvText))
            {
                Debug.LogWarning("[SpawnPointLoader] csvText 가 비어 있습니다.");
                return new SpawnPointRepository(new Dictionary<string, SpawnPointData>());
            }

            var parser = new CsvSpawnPointParser();
            parser.OnWarning = (line, msg)
                => Debug.LogWarning($"[SpawnPoints CSV] 행 {line}: {msg}");

            var dict = parser.Parse(csvText);
            Debug.Log($"[SpawnPointLoader] 로드 완료 — {dict.Count}개 스폰 지점 등록.");

            return new SpawnPointRepository(dict);
        }
    }
}