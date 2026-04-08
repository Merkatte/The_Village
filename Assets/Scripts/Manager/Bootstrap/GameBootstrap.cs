using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using Game.Data.Config;
using Game.Inventory;
using Game.Item.Catalog;
using Game.Graphic.Sprite;
using Game.Manager.Currency;
using Game.Manager.Input;
using Game.Manager.Scene;
using Game.Manager.State;
using Game.UI.Core;
using UnityEngine;
using System;

namespace Game.Manager.Bootstrap
{
    /// <summary>
    /// 게임 시작 시 가장 먼저 실행되는 진입점 MonoBehaviour 입니다.
    ///
    /// ─ 초기화 순서 (중요) ────────────────────────────────────
    ///   1. 코어 서비스 생성 및 ServiceLocator 등록
    ///      (SceneTransitionManager, GameStateManager)
    ///   2. InputManager.Init() 호출
    ///      → 내부에서 IGameStateService 구독 → 반드시 1 이후여야 함
    ///   3. StartGame() → GameState / SceneType 전환
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - Singleton 패턴 사용 안 함: ServiceLocator 가 그 역할을 대체
    ///   - 새 서비스 추가 시 RegisterServices() 만 수정 (OCP)
    ///   - InputManager 는 [SerializeField] 로 주입 받음
    ///     → Bootstrap 이 명시적으로 초기화 순서를 제어합니다.
    ///
    /// ─ 씬 설정 ────────────────────────────────────────────────
    ///   이 MonoBehaviour 는 반드시 BootstrapScene 에만 배치하세요.
    ///   Build Settings 씬 순서: 0.BootstrapScene, 1.DungeonScene, 2.TownScene
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] GlobalCsvConfig   globalCsvConfig;
        [SerializeField] InputManager      inputManager;
        [SerializeField] UIManager         uiManager;
        [SerializeField] UIPopupConfig     commonPopupConfig;
        [SerializeField] UIShortcutConfig  shortcutConfig;

        private UIShortcutHandler _shortcutHandler;

        private void Awake()
        {
            // 씬 전환 후에도 Bootstrap 오브젝트와 서비스가 유지됩니다.
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(inputManager.gameObject);
            DontDestroyOnLoad(uiManager.gameObject);
            
            RegisterServices();
            StartGame();
        }

        /// <summary>
        /// 모든 서비스 구현체를 인터페이스 타입으로 ServiceLocator 에 등록합니다.
        ///
        /// ─ 주의: inputManager.Init() 은 코어 서비스 등록 이후에 호출해야 합니다.
        ///   Init() 내부에서 IGameStateService.OnStateChanged 구독이 발생하기 때문입니다.
        ///   순서를 바꾸면 InputManager 가 게임 상태 이벤트를 받지 못합니다.
        /// </summary>
        private void RegisterServices()
        {
            // ─ Step 1. 데이터 서비스 등록 ────────────────────────────────────
            var catalogRepo = ItemCatalogLoader.Load(globalCsvConfig != null ? globalCsvConfig.itemCatalogCsv : null);
            ServiceLocator.Register<IItemCatalogRepository>(catalogRepo);

            var spriteRepo = SpriteLoader.Load(catalogRepo);
            ServiceLocator.Register<ISpriteRepository>(spriteRepo);

            // ─ Step 2. 코어 서비스 생성 및 등록 ─────────────────────────────
            var sceneLoader            = new UnitySceneLoader();
            var sceneTransitionManager = new SceneTransitionManager(sceneLoader);
            var gameStateManager       = new GameStateManager();

            ServiceLocator.Register<ISceneTransitionService>(sceneTransitionManager);
            ServiceLocator.Register<IGameStateService>(gameStateManager);
            ServiceLocator.Register<IInventory>(new Inventory.Inventory());
            ServiceLocator.Register<ICurrencyService>(new CurrencyManager());

            // ─ Step 3. InputManager 초기화 ────────────────────────────────
            // IGameStateService 가 이미 등록된 상태이므로 구독이 정상 동작합니다.
            inputManager.Init();
            ServiceLocator.Register<InputManager>(inputManager);

            // ─ Step 4. UIManager 초기화 ───────────────────────────────────
            // IUIInputReader 가 등록된 이후에 Init() 을 호출합니다.
            ServiceLocator.Register<IUIManager>(uiManager);
            if (commonPopupConfig != null)
                uiManager.RegisterPopups(commonPopupConfig);
            uiManager.Init();

            // ─ Step 5. UIShortcutHandler 초기화 ──────────────────────────
            if (shortcutConfig != null)
                _shortcutHandler = new UIShortcutHandler(
                    shortcutConfig,
                    uiManager,
                    () => ServiceLocator.Get<InputManager>().CurrentContext);

            Debug.Log("[GameBootstrap] 모든 서비스 등록 완료.");
        }

        private void OnDestroy()
        {
            _shortcutHandler?.Dispose();
        }

        /// <summary>게임을 시작하여 마을 씬으로 전환합니다.</summary>
        private void StartGame()
        {
            ServiceLocator.Get<IGameStateService>().ChangeState(GameState.Dungeon);
            ServiceLocator.Get<ISceneTransitionService>().TransitionToWithLoading(SceneType.Town);

            Debug.Log($"[GameBootstrap] {ServiceLocator.Get<IGameStateService>().CurrentState.ToString()} 으로 전환 시작.");
        }
    }
}
