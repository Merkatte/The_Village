using System;
using System.Collections.Generic;
using Game.Core.Csv;
using Game.Core.Enums;
using UnityEngine;

namespace Game.Dungeon.Spawn
{
    /// <summary>
    /// CSV 텍스트를 파싱하여 SpawnPointData 목록을 만드는 순수 C# 파서입니다.
    ///
    /// ─ CSV 형식 ───────────────────────────────────────────────
    ///   헤더  : SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId
    ///   예시  : Spawn_Forest_001,-5.0,3.0,Wood,0.8,Tree
    ///
    /// ─ 규칙 ───────────────────────────────────────────────────
    ///   - 같은 SpawnPointId 가 여러 줄에 있으면 하나의 지점으로 합산합니다.
    ///   - 같은 SpawnPointId 의 X, Y 는 첫 번째 등장 행의 값을 사용합니다.
    ///   - 빈 줄, 공백 줄, # 으로 시작하는 줄은 무시합니다.
    ///   - 첫 번째 데이터 행(헤더)은 자동으로 건너뜁니다.
    ///   - 파싱 실패 행은 skip 하고 경고 콜백을 호출합니다.
    /// </summary>
    public sealed class CsvSpawnPointParser
    {
        /// <summary>파싱 경고 발생 시 호출되는 콜백 (행 번호, 메시지)</summary>
        public Action<int, string> OnWarning { get; set; }

        /// <summary>CSV 텍스트 전체를 파싱합니다.</summary>
        public Dictionary<string, SpawnPointData> Parse(string csvText)
        {
            var positionMap = new Dictionary<string, Vector2>(StringComparer.OrdinalIgnoreCase);
            var entriesMap  = new Dictionary<string, List<SpawnEntry>>(StringComparer.OrdinalIgnoreCase);

            var reader       = new CsvReader();
            reader.OnWarning = OnWarning;

            reader.Read(csvText, (cols, lineNum) =>
            {
                if (!reader.RequireColumns(cols, lineNum, 6)) return;

                var spawnPointId = cols[0].Trim();
                if (string.IsNullOrWhiteSpace(spawnPointId))
                {
                    OnWarning?.Invoke(lineNum, "SpawnPointId 가 비어있습니다.");
                    return;
                }

                if (!reader.TryParseFloat(cols[1],               lineNum, "X",            out var x))            return;
                if (!reader.TryParseFloat(cols[2],               lineNum, "Y",            out var y))            return;
                if (!reader.TryParseEnum<ResourceType>(cols[3],  lineNum, "ResourceType", out var resourceType)) return;
                if (!reader.TryParseFloat(cols[4],               lineNum, "SpawnChance",  out var spawnChance))  return;

                var dropTableId = cols[5].Trim();
                if (string.IsNullOrWhiteSpace(dropTableId))
                {
                    OnWarning?.Invoke(lineNum, "DropTableId 가 비어있습니다.");
                    return;
                }

                if (!positionMap.ContainsKey(spawnPointId))
                {
                    positionMap[spawnPointId] = new Vector2(x, y);
                    entriesMap[spawnPointId]  = new List<SpawnEntry>();
                }

                entriesMap[spawnPointId].Add(new SpawnEntry(resourceType, spawnChance, dropTableId));
            });

            var result = new Dictionary<string, SpawnPointData>(StringComparer.OrdinalIgnoreCase);
            foreach (var id in positionMap.Keys)
                result[id] = new SpawnPointData(id, positionMap[id], entriesMap[id]);

            return result;
        }
    }
}