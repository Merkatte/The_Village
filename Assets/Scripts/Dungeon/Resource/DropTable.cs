using System.Collections.Generic;
using Game.Core.Enums;
using UnityEngine;

namespace Game.Dungeon.Resource
{
    /// <summary>
    /// 드롭 테이블 항목 하나를 나타냅니다.
    /// "이 자원이 이 확률로 최소~최대 몇 개 드롭된다" 를 정의합니다.
    /// </summary>
    public sealed class DropEntry
    {
        /// <summary>드롭될 자원 종류</summary>
        public ResourceType ResourceType { get; }

        /// <summary>드롭 확률 (0.0 ~ 1.0)</summary>
        public float DropChance { get; }

        /// <summary>최소 드롭 수량</summary>
        public int MinAmount { get; }

        /// <summary>최대 드롭 수량</summary>
        public int MaxAmount { get; }

        /// <summary>
        /// DropEntry 생성자.
        /// </summary>
        /// <param name="resourceType">자원 종류</param>
        /// <param name="dropChance">드롭 확률 (0.0 ~ 1.0, 범위 밖이면 클램프)</param>
        /// <param name="minAmount">최소 수량 (1 이상)</param>
        /// <param name="maxAmount">최대 수량 (minAmount 이상)</param>
        public DropEntry(ResourceType resourceType, float dropChance, int minAmount, int maxAmount)
        {
            ResourceType = resourceType;
            DropChance   = UnityEngine.Mathf.Clamp01(dropChance);
            MinAmount    = minAmount    < 1           ? 1           : minAmount;
            MaxAmount    = maxAmount    < MinAmount   ? MinAmount   : maxAmount;
        }
    }

    /// <summary>
    /// 드롭 테이블의 인터페이스입니다.
    /// 외부에서는 구체 구현 없이 이 인터페이스만 의존합니다. (DIP)
    /// </summary>
    public interface IDropTable
    {
        /// <summary>
        /// 드롭 테이블을 굴려 실제로 드롭될 자원 목록을 반환합니다.
        /// 확률 판정에 실패한 항목은 결과에 포함되지 않습니다.
        /// </summary>
        /// <param name="random">난수 생성기 (테스트 시 Fake 주입)</param>
        /// <returns>드롭된 자원 목록 (없으면 빈 리스트)</returns>
        IReadOnlyList<ResourceDrop> Roll(IRandom random);
    }
}
