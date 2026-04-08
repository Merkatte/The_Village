using System;
using System.Collections.Generic;

namespace Game.Dungeon.Spawn
{
    /// <summary>
    /// ISpawnPointRepository 의 구체 구현입니다.
    /// 파싱된 딕셔너리를 받아 ID 기반 조회와 전체 조회를 제공합니다.
    /// </summary>
    public sealed class SpawnPointRepository : ISpawnPointRepository
    {
        private readonly Dictionary<string, SpawnPointData> _table;
        private readonly List<SpawnPointData>               _allList;

        /// <summary>
        /// SpawnPointRepository 생성자.
        /// </summary>
        /// <param name="table">파싱된 스폰 지점 딕셔너리</param>
        public SpawnPointRepository(Dictionary<string, SpawnPointData> table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            _table   = table;
            _allList = new List<SpawnPointData>(table.Values);
        }

        /// <inheritdoc/>
        public SpawnPointData GetById(string spawnPointId)
        {
            if (string.IsNullOrWhiteSpace(spawnPointId))
                return null;

            return _table.TryGetValue(spawnPointId, out var data) ? data : null;
        }

        /// <inheritdoc/>
        public IReadOnlyList<SpawnPointData> GetAll() => _allList;
    }
}
