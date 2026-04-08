using System;
using Game.Core.Enums;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 채취 가능한 오브젝트입니다. (나무, 광석 등)
    ///
    /// ─ 배치 ────────────────────────────────────────────────────
    ///   HarvestableSpawner 가 생성한 Prefab 에 부착합니다.
    ///
    /// ─ 채취 흐름 ────────────────────────────────────────────────
    ///   1. InteractionDetector 가 범위 진입 감지 → CanHarvest = true
    ///   2. HarvestController 가 홀드 완료 시 Harvest() 호출
    ///   3. OnHarvested 이벤트 → DropManager 등이 구독하여 드롭 처리
    ///
    /// ─ 도구 호환 ────────────────────────────────────────────────
    ///   HarvestController 가 HarvestableType 을 읽어 호환 여부 판단.
    ///   Tree → Axe 필요 / Ore → Pickaxe 필요
    /// </summary>
    public class Harvestable : MonoBehaviour
    {
        [SerializeField] private HarvestableType _harvestableType;

        /// <summary>
        /// 드롭 테이블 조회 키. DropTableRepository.GetEntries() 에 전달됩니다.
        /// Inspector 에서 CSV 의 SourceId 값과 일치하도록 설정하세요. (예: "tree_oak")
        /// </summary>
        [SerializeField] private string _dropTableId;

        /// <summary>이 오브젝트를 채취하는 데 필요한 도구 종류.</summary>
        public HarvestableType HarvestableType => _harvestableType;

        /// <summary>드롭 테이블 조회 키.</summary>
        public string DropTableId
        {
            get => _dropTableId;
            set => _dropTableId = value;
        }

        /// <summary>플레이어가 채취 범위 안에 있을 때 true.</summary>
        public bool CanHarvest { get; set; } = false;

        /// <summary>
        /// 채취 완료 시 발행됩니다.
        /// 매개변수: 수확량 배율 (장착 도구의 YieldMultiplier)
        /// </summary>
        public event Action<float> OnHarvested;

        /// <summary>
        /// 채취를 완료합니다. HarvestController 가 홀드 완료 후 호출합니다.
        /// </summary>
        /// <param name="yieldMultiplier">장착 도구의 수확량 배율</param>
        public void Harvest(float yieldMultiplier = 1f)
        {
            Debug.Log("This Thing is harvested!");
            OnHarvested?.Invoke(yieldMultiplier);
        }
    }
}
