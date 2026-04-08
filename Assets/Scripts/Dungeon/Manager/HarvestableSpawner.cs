using Cysharp.Threading.Tasks;
using Game.Core.Infrastructure;
using Game.Data.Config;
using Game.Dungeon.Manager;
using Game.Dungeon.Spawn;
using UnityEngine;

namespace Game
{
    public class HarvestableSpawner : MonoBehaviour
    {
        [SerializeField] private ResourcePrefabConfig resourcePrefabConfig;

        /// <summary>
        /// 스폰 지점 데이터를 읽어 채취 오브젝트를 생성합니다.
        /// Instantiate 를 스폰 지점 하나마다 한 프레임씩 분산하여 프리징을 방지합니다.
        /// </summary>
        public async UniTask InitAsync()
        {
            var repo        = ServiceLocator.Get<ISpawnPointRepository>();
            var dropManager = ServiceLocator.Get<DropManager>();
            var allPoints   = repo.GetAll();

            foreach (var point in allPoints)
            {
                foreach (var entry in point.Entries)
                {
                    var prefab = resourcePrefabConfig.GetPrefab(entry.ResourceType);
                    if (prefab == null) continue;

                    if (Random.value < entry.SpawnChance)
                    {
                        var obj = Instantiate(prefab);
                        obj.transform.position = point.Position;
                        obj.SetActive(true);

                        if (obj.TryGetComponent<Harvestable>(out var harvestable))
                        {
                            harvestable.DropTableId = entry.DropTableId;
                            dropManager.Register(harvestable);
                        }

                        break;
                    }
                }

                await UniTask.Yield();
            }
        }
    }
}
