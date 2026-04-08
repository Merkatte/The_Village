using System.Collections.Generic;
using UnityEngine;

namespace Game.Dungeon.Interact
{
    /// <summary>
    /// 던전 및 마을 상호작용을 위한 MonoBehaviour입니다.
    /// 플레이어 근처 상호작용한 오브젝트를 감지합니다.
    ///
    /// ─ 배치 위치 ───────────────────────────────────────────────────────
    ///   DungeonScene 그리고 TownScene 내 Player 하위 컴포넌트로 부착합니다.
    ///   별도의 CircleCollider2D (isTrigger = true) 가 필요합니다.
    /// </summary>
    public sealed class InteractionDetector : MonoBehaviour
    {
        private readonly List<Harvestable> _candidates = new List<Harvestable>();
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Something Entered!!!" + other.name);
            if (other.TryGetComponent<Harvestable>(out var harvestable))
            {
                harvestable.CanHarvest = true;
                _candidates.Add(harvestable);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<Harvestable>(out var harvestable))
            {
                harvestable.CanHarvest = false;
                _candidates.Remove(harvestable);
            }
        }

        /// <summary>
        /// 현재 채취 범위 안에서 가장 가까운 Harvestable 을 반환합니다.
        /// 범위 안에 없으면 null 을 반환합니다.
        /// HarvestController 가 매 프레임 호출합니다.
        /// </summary>
        public Harvestable GetNearestHarvestable()
        {
            Harvestable nearest = null;
            float minDist = float.MaxValue;

            foreach (var candidate in _candidates)
            {
                float dist = Vector2.Distance(candidate.transform.position, transform.position);
                if (dist < minDist)
                {
                    minDist  = dist;
                    nearest  = candidate;
                }
            }

            return nearest;
        }
    }
}
