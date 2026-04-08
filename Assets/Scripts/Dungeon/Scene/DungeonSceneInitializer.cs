using Cysharp.Threading.Tasks;
using UnityEngine;
using Game.Data.Config;
using Game.Core.Infrastructure;
using Game.Dungeon.Data;
using Game.Dungeon.Manager;
using Game.Dungeon.Resource;
using Game.Dungeon.Spawn;
using Game.Enemy;
using Game.Enemy.Data;
using Game.UI.Core;

namespace Game.Dungeon.Scene
{
    /// <summary>
    /// 던전 씬 전용 초기화 MonoBehaviour입니다.
    /// 던전에서만 필요한 서비스를 씬 로드 시 등록하고, 씬 종료 시 해제합니다.
    ///
    /// ─ 배치 위치 ──────────────────────────────────────────────
    ///   DungeonScene 내 전용 GameObject 에 컴포넌트로 부착합니다.
    ///   (예: 씬 내 "DungeonInitializer" GameObject)
    ///
    /// ─ 등록 서비스 ────────────────────────────────────────────
    ///   - IDropTableRepository  : 드롭 테이블 CSV 데이터
    ///   - ISpawnPointRepository : 씬별 자원 스폰 위치 데이터
    ///   - DropManager           : 채취 완료 → 인벤토리 추가 처리
    ///   - HarvestableSpawner    : 스폰 지점 기반 채취 오브젝트 생성
    ///   - IEnemyRepository      : 적 수치 데이터 (EnemyData.csv)
    ///
    /// ─ 등록하지 않는 서비스 (Bootstrap 담당) ──────────────────
    ///   - ISceneTransitionService
    ///   - IGameStateService
    ///   - IInventory
    /// </summary>
    public sealed class DungeonSceneInitializer : MonoBehaviour
    {
        [SerializeField] DungeonCsvConfig   dungeonCsvConfig;
        [SerializeField] DropManager        dropManager;
        [SerializeField] HarvestableSpawner harvestSpawner;

        [Header("적 AI")]
        [SerializeField] EnemyCsvConfig  enemyCsvConfig;
        [SerializeField] EnemySpawner    enemySpawner;

        [Header("UI")]
        [SerializeField] UIPopupConfig uiPopupConfig;
        [SerializeField] Canvas        popupCanvas;

        private void Awake()     => RegisterDungeonServicesAsync().Forget();
        private void OnDestroy() => UnregisterDungeonServices();

        private async UniTaskVoid RegisterDungeonServicesAsync()
        {
            // ─ Step 1. TextAsset 텍스트 추출 (메인 스레드) ──────────────
            var dropTableText = dungeonCsvConfig != null ? dungeonCsvConfig.dropTablesCsv?.text   : null;
            var spawnText     = dungeonCsvConfig != null ? dungeonCsvConfig.spawnPointsCsv?.text  : null;
            var enemyText     = enemyCsvConfig   != null ? enemyCsvConfig.enemyCsv?.text           : null;

            // ─ Step 2. CSV 파싱 (백그라운드 스레드) ──────────────────────
            await UniTask.SwitchToThreadPool();

            var dropTableRepo = DropTableLoader.LoadFromText(dropTableText);
            var spawnRepo     = SpawnPointLoader.LoadFromText(spawnText);
            var enemyRepo     = EnemyLoader.LoadFromText(enemyText);

            // ─ Step 3. 서비스 등록 (메인 스레드 복귀) ───────────────────
            await UniTask.SwitchToMainThread();

            ServiceLocator.Register<IDropTableRepository>(dropTableRepo);
            ServiceLocator.Register<ISpawnPointRepository>(spawnRepo);
            ServiceLocator.Register<IEnemyRepository>(enemyRepo);

            // ─ Step 4. UI Canvas 주입 ────────────────────────────────────
            var uiManager = ServiceLocator.Get<IUIManager>();
            uiManager.SetCanvas(popupCanvas);
            if (uiPopupConfig != null)
                uiManager.RegisterPopups(uiPopupConfig);

            // ─ Step 5. 매니저 등록 ───────────────────────────────────────
            ServiceLocator.Register<DropManager>(dropManager);
            ServiceLocator.Register<HarvestableSpawner>(harvestSpawner);

            // ─ Step 6. 초기화 순서 보장 ──────────────────────────────────
            // dropManager.Init() 이 먼저 실행되어야 InitAsync() 에서
            // Register(harvestable) 호출 시 드롭 매니저가 준비된 상태입니다.
            dropManager.Init();
            await harvestSpawner.InitAsync();

            // ─ Step 7. 적 스폰 (IEnemyRepository 등록 후) ────────────────
            if (enemySpawner != null)
                await enemySpawner.InitAsync();

            Debug.Log("[DungeonSceneInitializer] 던전 서비스 등록 완료.");
        }

        private void UnregisterDungeonServices()
        {
            ServiceLocator.Unregister<IDropTableRepository>();
            ServiceLocator.Unregister<ISpawnPointRepository>();
            ServiceLocator.Unregister<IEnemyRepository>();
            ServiceLocator.Unregister<DropManager>();
            ServiceLocator.Unregister<HarvestableSpawner>();

            Debug.Log("[DungeonSceneInitializer] 던전 서비스 해제 완료.");
        }
    }
}