using System;
using System.Collections.Generic;
using Game.Core.Csv;
using Game.Core.Enums;

namespace Game.Dungeon.Resource
{
    /// <summary>
    /// CSV 텍스트를 파싱하여 DropEntry 목록을 만드는 순수 C# 파서입니다.
    ///
    /// ─ CSV 형식 ───────────────────────────────────────────────
    ///   헤더: SourceId,ResourceType,DropChance,MinAmount,MaxAmount
    ///   예시: Tree,Wood,0.9,1,3
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 파일 I/O 없이 string 만 받아서 처리 → EditMode 테스트 가능
    ///   - 첫 행(헤더)은 자동으로 건너뜁니다.
    ///   - 빈 줄, 공백만 있는 줄, #으로 시작하는 주석 줄은 무시합니다.
    ///   - 파싱 실패 행은 예외 대신 skip 하고 경고 콜백을 호출합니다.
    /// </summary>
    public sealed class CsvDropTableParser
    {
        /// <summary>파싱 경고 발생 시 호출되는 콜백 (행 번호, 메시지)</summary>
        public Action<int, string> OnWarning { get; set; }

        /// <summary>CSV 텍스트 전체를 파싱합니다.</summary>
        public Dictionary<string, List<DropEntry>> Parse(string csvText)
        {
            var result = new Dictionary<string, List<DropEntry>>(
                StringComparer.OrdinalIgnoreCase);

            var reader       = new CsvReader();
            reader.OnWarning = OnWarning;

            reader.Read(csvText, (cols, lineNum) =>
            {
                if (!reader.RequireColumns(cols, lineNum, 5)) return;

                var sourceId = cols[0].Trim();
                if (string.IsNullOrWhiteSpace(sourceId))
                {
                    OnWarning?.Invoke(lineNum, "SourceId 가 비어있습니다.");
                    return;
                }

                if (!reader.TryParseEnum<ResourceType>(cols[1], lineNum, "ResourceType", out var resourceType)) return;
                if (!reader.TryParseFloat(cols[2],              lineNum, "DropChance",   out var dropChance))   return;
                if (!reader.TryParseInt(cols[3],                lineNum, "MinAmount",    out var minAmount))    return;
                if (!reader.TryParseInt(cols[4],                lineNum, "MaxAmount",    out var maxAmount))    return;

                if (!result.ContainsKey(sourceId))
                    result[sourceId] = new List<DropEntry>();

                result[sourceId].Add(new DropEntry(resourceType, dropChance, minAmount, maxAmount));
            });

            return result;
        }
    }
}