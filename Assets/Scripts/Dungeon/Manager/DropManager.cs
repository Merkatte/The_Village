using Game.Core.Infrastructure;
using Game.Dungeon.Resource;
using Game.Inventory;
using Game.Item.Catalog;
using Game.Item.Resource;
using UnityEngine;

namespace Game.Dungeon.Manager
{
    /// <summary>
    /// 채취 완료 시 드롭 테이블을 굴려 아이템을 인벤토리에 추가합니다.
    ///
    /// ─ 초기화 흐름 ────────────────────────────────────────────
    ///   1. DungeonSceneInitializer 가 Init() 호출
    ///      → IDropTableRepository, IInventory 를 ServiceLocator 에서 수령
    ///   2. HarvestableSpawner 가 오브젝트 스폰 후 Register(harvestable) 호출
    ///      → OnHarvested 이벤트 구독
    ///   3. 플레이어가 채취 완료 시 HandleHarvest 실행
    ///      → 드롭 판정 → ResourceItem 생성 → 인벤토리 추가
    ///
    /// ─ 인벤토리 가득 찼을 때 ─────────────────────────────────
    ///   TryAddItem 이 false 를 반환하면 경고 로그를 남깁니다.
    ///   (추후 UI 알림으로 교체 예정)
    /// </summary>
    public sealed class DropManager : MonoBehaviour
    {
        private IDropTableRepository  _dropTableRepo;
        private IItemCatalogRepository _catalogRepo;
        private IInventory            _inventory;
        private IRandom               _random;

        /// <summary>
        /// 서비스를 ServiceLocator 에서 수령합니다.
        /// DungeonSceneInitializer 가 HarvestableSpawner.Init() 전에 호출해야 합니다.
        /// </summary>
        public void Init()
        {
            _dropTableRepo = ServiceLocator.Get<IDropTableRepository>();
            _catalogRepo   = ServiceLocator.Get<IItemCatalogRepository>();
            _inventory     = ServiceLocator.Get<IInventory>();
            _random        = new UnityRandom();
        }

        /// <summary>
        /// Harvestable 을 등록합니다.
        /// 등록 후 OnHarvested 발행 시 드롭 판정이 자동으로 수행됩니다.
        /// HarvestableSpawner 가 오브젝트 스폰 직후 호출합니다.
        /// </summary>
        public void Register(Harvestable harvestable)
        {
            harvestable.OnHarvested += yieldMultiplier => HandleHarvest(harvestable, yieldMultiplier);
        }

        private void HandleHarvest(Harvestable harvestable, float yieldMultiplier)
        {
            Debug.Log("Something is harvested!!!");
            Debug.Log(harvestable.DropTableId);
            var dropTable = _dropTableRepo.GetTable(harvestable.DropTableId);
            if (dropTable == null)
            {
                Debug.LogWarning($"[DropManager] 드롭 테이블 없음: '{harvestable.DropTableId}'");
                return;
            }
            
            var drops = dropTable.Roll(_random);

            foreach (var drop in drops)
            {
                Debug.Log("Calculating");
                var entry = _catalogRepo.GetEntryByResourceType(drop.Type);
                Debug.Log("!");
                if (entry == null)
                {
                    Debug.LogWarning($"[DropManager] 카탈로그에 '{drop.Type}' 항목이 없습니다. ItemCatalog.csv 를 확인하세요.");
                    continue;
                }
                Debug.Log("!!");
                int quantity = Mathf.Max(1, Mathf.RoundToInt(drop.Amount * yieldMultiplier));
                Debug.Log("!!!");
                var item     = new ResourceItem(entry, quantity);
                Debug.Log("!!!!");
                if (!_inventory.TryAddItem(item))
                    Debug.LogWarning(
                        $"[DropManager] 인벤토리 가득 참 — {item.ItemName} x{quantity} 획득 실패.");
                else Debug.Log($"[DropManager] {item.ItemName} x{quantity} 획득.");
                Debug.Log("!!!!!");
            }
        }
    }
}
