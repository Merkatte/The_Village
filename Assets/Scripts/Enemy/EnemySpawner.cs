using Cysharp.Threading.Tasks;
using Game.Core.Infrastructure;
using Game.Data.Config;
using Game.Enemy.Data;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// EnemySpawnConfig 를 읽어 적 프리팹을 씬에 생성하는 MonoBehaviour 입니다.
    ///
    /// ─ 역할 (SRP) ─────────────────────────────────────────────
    ///   - EnemySpawnConfig 의 항목을 순회하며 확률 판정 후 Instantiate
    ///   - Instantiate 를 항목마다 한 프레임씩 분산하여 프리징 방지 (UniTask.Yield)
    ///   - 프리팹 매핑은 EnemyPrefabConfig 에 위임
    ///   - EnemyData 유효성은 IEnemyRepository 확인 (없으면 skip + 경고)
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   enemyPrefabConfig : EnemyType → 프리팹 매핑 에셋
    ///   enemySpawnConfig  : 스폰 위치·EnemyId·확률 목록 에셋
    ///
    /// ─ 사용 방법 ──────────────────────────────────────────────
    ///   DungeonSceneInitializer 에서 IEnemyRepository 등록 후
    ///   await enemySpawner.InitAsync() 호출
    /// </summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyPrefabConfig enemyPrefabConfig;
        [SerializeField] private EnemySpawnConfig  enemySpawnConfig;

        /// <summary>
        /// 스폰 설정을 읽어 적을 씬에 배치합니다.
        /// Instantiate 를 한 항목당 한 프레임 분산합니다.
        /// </summary>
        public async UniTask InitAsync()
        {
            if (enemyPrefabConfig == null)
            {
                Debug.LogWarning("[EnemySpawner] enemyPrefabConfig 가 연결되지 않았습니다.");
                return;
            }

            if (enemySpawnConfig == null)
            {
                Debug.LogWarning("[EnemySpawner] enemySpawnConfig 가 연결되지 않았습니다.");
                return;
            }

            var repo = ServiceLocator.Get<IEnemyRepository>();

            foreach (var entry in enemySpawnConfig.Entries)
            {
                if (Random.value > entry.SpawnChance)
                {
                    await UniTask.Yield();
                    continue;
                }

                var data = repo?.GetById(entry.EnemyId);
                if (data == null)
                {
                    Debug.LogWarning($"[EnemySpawner] EnemyId '{entry.EnemyId}' 를 IEnemyRepository 에서 찾을 수 없습니다. 해당 항목을 건너뜁니다.");
                    await UniTask.Yield();
                    continue;
                }

                var prefab = enemyPrefabConfig.GetPrefab(data.EnemyType);
                if (prefab == null)
                {
                    await UniTask.Yield();
                    continue;
                }

                var obj = Instantiate(prefab);
                obj.transform.position = new Vector3(entry.Position.x, entry.Position.y, 0f);
                obj.SetActive(true);

                await UniTask.Yield();
            }

            Debug.Log($"[EnemySpawner] 스폰 완료.");
        }
    }
}
