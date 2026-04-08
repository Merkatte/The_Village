using System;
using System.Collections.Generic;

namespace Game.Dungeon.Resource
{
    /// <summary>
    /// IDropTableRepository 의 구체 구현입니다.
    /// 파싱된 딕셔너리를 받아 조회 기능을 제공합니다.
    ///
    /// ─ 생성 방법 ──────────────────────────────────────────────
    ///   var parser = new CsvDropTableParser();
    ///   var dict   = parser.Parse(csvText);
    ///   var repo   = new DropTableRepository(dict);
    /// </summary>
    public sealed class DropTableRepository : IDropTableRepository
    {
        private readonly Dictionary<string, IDropTable> _tables;

        /// <summary>
        /// DropTableRepository 생성자.
        /// 파싱된 딕셔너리를 받아 내부에서 IDropTable 인스턴스를 1회 생성합니다.
        /// </summary>
        /// <param name="table">파싱된 드롭 테이블 딕셔너리</param>
        public DropTableRepository(Dictionary<string, List<DropEntry>> table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            _tables = new Dictionary<string, IDropTable>(table.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var kv in table)
                _tables[kv.Key] = new DropTableImpl(kv.Value);
        }

        /// <inheritdoc/>
        public IDropTable GetTable(string sourceId)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
                return null;

            _tables.TryGetValue(sourceId, out var table);
            return table;
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetAllSourceIds()
            => new List<string>(_tables.Keys);
    }
}
