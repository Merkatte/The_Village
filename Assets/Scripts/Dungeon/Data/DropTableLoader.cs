using System.Collections.Generic;
using UnityEngine;
using Game.Dungeon.Resource;

namespace Game.Dungeon.Data
{
    /// <summary>
    /// CSV 텍스트 에셋을 로드하여 IDropTableRepository 를 제공합니다.
    ///
    /// ─ 사용 방법 ──────────────────────────────────────────────
    ///   동기(메인 스레드):   DropTableLoader.Load(textAsset)
    ///   비동기(백그라운드):  DropTableLoader.LoadFromText(textAsset.text)
    ///                        → Task.Run / UniTask.SwitchToThreadPool 컨텍스트에서 호출
    /// </summary>
    public static class DropTableLoader
    {
        /// <summary>
        /// TextAsset 으로부터 IDropTableRepository 를 생성합니다.
        /// </summary>
        /// <param name="csvAsset">Inspector 에서 연결된 TextAsset. null 이면 빈 저장소를 반환합니다.</param>
        public static IDropTableRepository Load(TextAsset csvAsset)
        {
            if (csvAsset == null)
            {
                Debug.LogWarning(
                    "[DropTableLoader] csvAsset 이 null 입니다. " +
                    "DungeonCsvConfig 의 dropTablesCsv 필드에 CSV 를 연결했는지 확인하세요.");

                return new DropTableRepository(new Dictionary<string, List<DropEntry>>());
            }

            return LoadFromText(csvAsset.text);
        }

        /// <summary>
        /// CSV 문자열로부터 IDropTableRepository 를 생성합니다.
        /// TextAsset.text 는 메인 스레드에서 미리 추출한 뒤 이 메서드를 백그라운드 스레드에서 호출하세요.
        /// </summary>
        /// <param name="csvText">파싱할 CSV 문자열. null 또는 빈 값이면 빈 저장소를 반환합니다.</param>
        public static IDropTableRepository LoadFromText(string csvText)
        {
            if (string.IsNullOrEmpty(csvText))
            {
                Debug.LogWarning("[DropTableLoader] csvText 가 비어 있습니다.");
                return new DropTableRepository(new Dictionary<string, List<DropEntry>>());
            }

            var parser = new CsvDropTableParser();
            parser.OnWarning = (line, msg)
                => Debug.LogWarning($"[DropTable CSV] 행 {line}: {msg}");

            var dict = parser.Parse(csvText);
            Debug.Log($"[DropTableLoader] 로드 완료 — {dict.Count}개 소스 등록.");

            return new DropTableRepository(dict);
        }
    }
}