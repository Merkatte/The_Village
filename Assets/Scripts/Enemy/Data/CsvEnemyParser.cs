using System;
using System.Collections.Generic;
using Game.Core.Csv;
using Game.Core.Enums;
using Game.Enemy.Core;

namespace Game.Enemy.Data
{
    /// <summary>
    /// CSV 텍스트를 파싱하여 EnemyData 목록을 만드는 순수 C# 파서입니다.
    ///
    /// ─ CSV 형식 (14열 필수) ───────────────────────────────────
    ///   EnemyId, EnemyType, MaxHp, MoveSpeed,
    ///   AlertRange, CombatRange, AttackRange,
    ///   AttackDamage, AttackCooldown, PreAttackDelay, PostAttackStun,
    ///   AttackType, WanderRadius, WanderInterval
    ///
    ///   예시:
    ///   slime_basic,Slime,30,2,10,6,2,5,2,0.2,0.4,Melee,3,3
    ///   goblin_warrior,Goblin,60,3,12,8,2.5,12,1.8,0.3,0.5,Melee,4,2
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 파일 I/O 없이 string 만 받아 처리 → EditMode 테스트 가능
    ///   - 빈 줄, #주석, 헤더 행은 자동 건너뜀
    ///   - 파싱 실패 행은 skip 후 경고 콜백 호출
    /// </summary>
    public sealed class CsvEnemyParser
    {
        /// <summary>파싱 경고 발생 시 호출되는 콜백 (행 번호, 메시지)</summary>
        public Action<int, string> OnWarning { get; set; }

        /// <summary>CSV 텍스트 전체를 파싱하여 EnemyData 목록을 반환합니다.</summary>
        public List<EnemyData> Parse(string csvText)
        {
            var result = new List<EnemyData>();
            var reader = new CsvReader();
            reader.OnWarning = OnWarning;

            reader.Read(csvText, (cols, lineNum) =>
            {
                if (!reader.RequireColumns(cols, lineNum, 14)) return;

                var enemyId = cols[0].Trim();
                if (string.IsNullOrWhiteSpace(enemyId))
                {
                    OnWarning?.Invoke(lineNum, "EnemyId 가 비어있습니다.");
                    return;
                }

                if (!reader.TryParseEnum<EnemyType>(cols[1],  lineNum, "EnemyType",      out var enemyType))      return;
                if (!reader.TryParseInt(cols[2],              lineNum, "MaxHp",           out var maxHp))          return;
                if (!reader.TryParseFloat(cols[3],            lineNum, "MoveSpeed",       out var moveSpeed))      return;
                if (!reader.TryParseFloat(cols[4],            lineNum, "AlertRange",      out var alertRange))     return;
                if (!reader.TryParseFloat(cols[5],            lineNum, "CombatRange",     out var combatRange))    return;
                if (!reader.TryParseFloat(cols[6],            lineNum, "AttackRange",     out var attackRange))    return;
                if (!reader.TryParseInt(cols[7],              lineNum, "AttackDamage",    out var attackDamage))   return;
                if (!reader.TryParseFloat(cols[8],            lineNum, "AttackCooldown",  out var attackCooldown)) return;
                if (!reader.TryParseFloat(cols[9],            lineNum, "PreAttackDelay",  out var preAttackDelay)) return;
                if (!reader.TryParseFloat(cols[10],           lineNum, "PostAttackStun",  out var postAttackStun)) return;
                if (!reader.TryParseEnum<AttackType>(cols[11], lineNum, "AttackType",      out var attackType))     return;
                if (!reader.TryParseFloat(cols[12],           lineNum, "WanderRadius",    out var wanderRadius))   return;
                if (!reader.TryParseFloat(cols[13],           lineNum, "WanderInterval",  out var wanderInterval)) return;

                if (maxHp <= 0)
                {
                    OnWarning?.Invoke(lineNum, $"MaxHp 은 1 이상이어야 합니다: '{maxHp}'");
                    return;
                }

                result.Add(new EnemyData(
                    enemyId, enemyType, maxHp, moveSpeed,
                    alertRange, combatRange, attackRange,
                    attackDamage, attackCooldown, preAttackDelay,
                    postAttackStun, attackType, wanderRadius, wanderInterval));
            });

            return result;
        }
    }
}
