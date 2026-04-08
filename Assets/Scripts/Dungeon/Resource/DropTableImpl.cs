using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dungeon.Resource
{
    /// <summary>
    /// IDropTable 의 구체 구현입니다.
    ///
    /// ─ 동작 규칙 ──────────────────────────────────────────────
    ///   Roll() 호출 시 각 DropEntry 에 대해:
    ///   1. IRandom.NextFloat() 값이 DropChance 보다 작으면 드롭 성공
    ///   2. 드롭 성공 시 MinAmount ~ MaxAmount 범위에서 수량 결정
    ///   3. 결과를 ResourceDrop 으로 변환하여 반환
    /// </summary>
    public sealed class DropTableImpl : IDropTable
    {
        private readonly IReadOnlyList<DropEntry> _entries;

        /// <summary>
        /// DropTableImpl 생성자.
        /// </summary>
        /// <param name="entries">드롭 항목 목록</param>
        public DropTableImpl(IReadOnlyList<DropEntry> entries)
        {
            _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        /// <inheritdoc/>
        public IReadOnlyList<ResourceDrop> Roll(IRandom random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));

            var result = new List<ResourceDrop>();
            foreach (var entry in _entries)
            {
                // 확률 판정: random 값이 DropChance 미만이면 드롭
                if (random.NextFloat() < entry.DropChance)
                {
                    var amount = random.NextInt(entry.MinAmount, entry.MaxAmount);
                    Debug.Log("amount = " + amount);
                    result.Add(new ResourceDrop(entry.ResourceType, amount));
                }
            }
            Debug.Log(result.Count);
            return result;
        }
    }
}
