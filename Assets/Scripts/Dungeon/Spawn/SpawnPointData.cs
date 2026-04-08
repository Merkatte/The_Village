using System.Collections.Generic;
using Game.Core.Enums;
using UnityEngine;

namespace Game.Dungeon.Spawn
{
    /// <summary>
    /// 하나의 스폰 지점 데이터를 나타내는 불변 구조체입니다.
    ///
    /// ─ 구성 ───────────────────────────────────────────────────
    ///   SpawnPointId : 고유 식별자 (예: "Spawn_Forest_001")
    ///   Position     : 씬 내 월드 좌표 (XY 평면)
    ///   Entries      : 이 지점에서 스폰 가능한 자원 목록 (ResourceType + 확률)
    /// </summary>
    public sealed class SpawnPointData
    {
        /// <summary>스폰 지점 고유 ID</summary>
        public string SpawnPointId { get; }

        /// <summary>씬 내 월드 좌표</summary>
        public Vector2 Position { get; }

        /// <summary>스폰 가능한 자원 항목 목록</summary>
        public IReadOnlyList<SpawnEntry> Entries { get; }

        public SpawnPointData(string spawnPointId, Vector2 position, IReadOnlyList<SpawnEntry> entries)
        {
            SpawnPointId = spawnPointId;
            Position     = position;
            Entries      = entries;
        }
    }

    /// <summary>
    /// 스폰 지점 하나에서 특정 자원이 스폰될 확률 항목입니다.
    /// </summary>
    public sealed class SpawnEntry
    {
        /// <summary>스폰될 자원 종류</summary>
        public ResourceType ResourceType { get; }

        /// <summary>스폰 확률 (0.0 ~ 1.0)</summary>
        public float SpawnChance { get; }

        /// <summary>채취 완료 시 참조할 드롭 테이블 ID (DropTable.csv 의 SourceId)</summary>
        public string DropTableId { get; }

        public SpawnEntry(ResourceType resourceType, float spawnChance, string dropTableId)
        {
            ResourceType = resourceType;
            SpawnChance  = Mathf.Clamp01(spawnChance);
            DropTableId  = dropTableId;
        }
    }
}
