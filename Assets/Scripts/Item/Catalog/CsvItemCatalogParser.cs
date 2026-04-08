using System;
using System.Collections.Generic;
using Game.Core.Csv;
using Game.Core.Enums;

namespace Game.Item.Catalog
{
    /// <summary>
    /// CSV 텍스트를 파싱하여 ItemCatalogEntry 목록을 만드는 순수 C# 파서입니다.
    ///
    /// ─ CSV 형식 ───────────────────────────────────────────────
    ///   헤더(12열):
    ///   ItemId, ItemName, ItemType, Grade, MaxStackSize, Value,
    ///   ResourceType, ToolType, HarvestDurationMultiplier, YieldMultiplier,
    ///   AttackPower, DefensePower
    ///
    ///   필수열: 앞 6개 (ItemId ~ Value)
    ///   선택열: 뒤 6개 — 해당 타입에 맞는 열만 채우고 나머지는 비워둡니다.
    ///
    ///   예시:
    ///   wood,나무,Resource,Normal,99,5,Wood,,,,,
    ///   axe_iron,철 도끼,HarvestTool,Normal,1,200,,Axe,0.8,1.2,,
    ///   sword_basic,기본 검,Weapon,Normal,1,300,,,,,30,
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 파일 I/O 없이 string 만 받아 처리 → EditMode 테스트 가능
    ///   - 빈 줄, #주석, 헤더 행은 자동 건너뜀
    ///   - 파싱 실패 행은 skip 후 경고 콜백 호출
    /// </summary>
    public sealed class CsvItemCatalogParser
    {
        /// <summary>파싱 경고 발생 시 호출되는 콜백 (행 번호, 메시지)</summary>
        public Action<int, string> OnWarning { get; set; }

        /// <summary>CSV 텍스트 전체를 파싱하여 ItemCatalogEntry 목록을 반환합니다.</summary>
        public List<ItemCatalogEntry> Parse(string csvText)
        {
            var result = new List<ItemCatalogEntry>();
            var reader = new CsvReader();
            reader.OnWarning = OnWarning;

            reader.Read(csvText, (cols, lineNum) =>
            {
                // ─ 필수 6열 ──────────────────────────────────────
                if (!reader.RequireColumns(cols, lineNum, 6)) return;

                var itemId = cols[0].Trim();
                if (string.IsNullOrWhiteSpace(itemId))
                {
                    OnWarning?.Invoke(lineNum, "ItemId 가 비어있습니다.");
                    return;
                }

                var itemName = cols[1].Trim();

                if (!reader.TryParseEnum<ItemType>(cols[2],  lineNum, "ItemType",     out var itemType))     return;
                if (!reader.TryParseEnum<ItemGrade>(cols[3], lineNum, "Grade",        out var grade))        return;
                if (!reader.TryParseInt(cols[4],             lineNum, "MaxStackSize", out var maxStackSize)) return;
                if (!reader.TryParseInt(cols[5],             lineNum, "Value",        out var value))        return;

                // ─ 선택 6열 ──────────────────────────────────────
                ResourceType? resourceType        = null;
                ToolType?     toolType            = null;
                float         harvestDurationMult = 1f;
                float         yieldMultiplier     = 1f;
                int           attackPower         = 0;
                int           defensePower        = 0;

                if (cols.Length > 6  && !string.IsNullOrWhiteSpace(cols[6]))
                    if (reader.TryParseEnum<ResourceType>(cols[6], lineNum, "ResourceType", out var rt))
                        resourceType = rt;

                if (cols.Length > 7  && !string.IsNullOrWhiteSpace(cols[7]))
                    if (reader.TryParseEnum<ToolType>(cols[7], lineNum, "ToolType", out var tt))
                        toolType = tt;

                if (cols.Length > 8  && !string.IsNullOrWhiteSpace(cols[8]))
                    reader.TryParseFloat(cols[8],  lineNum, "HarvestDurationMultiplier", out harvestDurationMult);

                if (cols.Length > 9  && !string.IsNullOrWhiteSpace(cols[9]))
                    reader.TryParseFloat(cols[9],  lineNum, "YieldMultiplier", out yieldMultiplier);

                if (cols.Length > 10 && !string.IsNullOrWhiteSpace(cols[10]))
                    reader.TryParseInt(cols[10], lineNum, "AttackPower", out attackPower);

                if (cols.Length > 11 && !string.IsNullOrWhiteSpace(cols[11]))
                    reader.TryParseInt(cols[11], lineNum, "DefensePower", out defensePower);

                result.Add(new ItemCatalogEntry(
                    itemId, itemName, itemType, grade, maxStackSize, value,
                    resourceType, toolType, harvestDurationMult, yieldMultiplier,
                    attackPower, defensePower));
            });

            return result;
        }
    }
}
