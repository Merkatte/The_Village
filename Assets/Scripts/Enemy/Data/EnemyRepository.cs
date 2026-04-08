using System;
using System.Collections.Generic;
using Game.Enemy.Core;

namespace Game.Enemy.Data
{
    /// <summary>
    /// IEnemyRepository 구현체입니다.
    /// 파싱된 EnemyData 목록을 받아 EnemyId 기반 조회를 제공합니다.
    /// </summary>
    public sealed class EnemyRepository : IEnemyRepository
    {
        private readonly Dictionary<string, EnemyData> _byId;
        private readonly List<EnemyData>               _all;

        public EnemyRepository(IReadOnlyList<EnemyData> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            _byId = new Dictionary<string, EnemyData>(StringComparer.OrdinalIgnoreCase);
            _all  = new List<EnemyData>(entries);

            foreach (var entry in entries)
                _byId[entry.EnemyId] = entry;
        }

        /// <inheritdoc/>
        public EnemyData GetById(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId)) return null;
            return _byId.TryGetValue(enemyId, out var data) ? data : null;
        }

        /// <inheritdoc/>
        public IReadOnlyList<EnemyData> GetAll() => _all;
    }
}
