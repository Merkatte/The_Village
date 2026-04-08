using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using Game.Manager.Interfaces.Input;
using UnityEngine;

namespace Game.Manager.Input
{
    /// <summary>
    /// 모든 입력 맥락(Context)을 총괄하는 매니저 MonoBehaviour 입니다.
    ///
    /// ─ 역할 ───────────────────────────────────────────────────
    ///   1. IPlayerInputReader, IUIInputReader 구현체를 생성하고 소유합니다.
    ///   2. ServiceLocator 에 인터페이스 타입으로 등록합니다.
    ///   3. IGameStateService.OnStateChanged 를 구독하여
    ///      게임 상태에 맞는 InputReader 를 자동으로 활성화합니다.
    ///   4. OnDestroy 에서 모든 InputAction 리소스를 정리합니다.
    ///
    /// ─ 컨텍스트 전환 규칙 ─────────────────────────────────────
    ///   GameState.Town / Dungeon → Gameplay 컨텍스트 (플레이어 이동 활성)
    ///   SwitchContext(UI)        → UI 컨텍스트 (인벤토리/메뉴 오픈 시)
    ///
    /// ─ 배치 위치 ──────────────────────────────────────────────
    ///   BootstrapScene 의 GameBootstrap 오브젝트에 추가하세요.
    ///   DontDestroyOnLoad 로 씬 전환 후에도 유지됩니다.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        /// <summary>현재 활성화된 입력 맥락</summary>
        private InputContext _currentContext = InputContext.None;

        /// <summary>현재 활성화된 입력 맥락을 반환합니다.</summary>
        public InputContext CurrentContext => _currentContext;

        /// <summary>플레이어 게임플레이 입력 처리기</summary>
        private PlayerInputReader _playerInputReader;

        /// <summary>UI 입력 처리기</summary>
        private UIInputReader _uiInputReader;

        public void Init()
        {
            DontDestroyOnLoad(gameObject);
            CreateAndRegisterReaders();
            SubscribeToGameState();
            SwitchContext(InputContext.Gameplay);

            Debug.Log("[InputManager] 초기화 완료 — Gameplay 컨텍스트 활성.");
        }

        /// <summary>
        /// IPlayerInputReader, IUIInputReader 구현체를 생성하고
        /// ServiceLocator 에 인터페이스 타입으로 등록합니다.
        /// </summary>
        private void CreateAndRegisterReaders()
        {
            _playerInputReader = new PlayerInputReader();
            _uiInputReader     = new UIInputReader();

            ServiceLocator.Register<IPlayerInputReader>(_playerInputReader);
            ServiceLocator.Register<IUIInputReader>(_uiInputReader);
        }

        /// <summary>
        /// IGameStateService 의 OnStateChanged 이벤트를 구독합니다.
        /// 게임 상태 변경 시 자동으로 컨텍스트를 전환합니다.
        /// </summary>
        private void SubscribeToGameState()
        {
            if (!ServiceLocator.IsRegistered<IGameStateService>()) return;
            ServiceLocator.Get<IGameStateService>().OnStateChanged += OnGameStateChanged;
        }

        /// <summary>
        /// 게임 상태 변경 시 호출됩니다.
        /// Town / Dungeon 모두 Gameplay 컨텍스트를 사용합니다.
        /// </summary>
        /// <param name="previous">이전 게임 상태</param>
        /// <param name="next">새 게임 상태</param>
        private void OnGameStateChanged(GameState previous, GameState next)
        {
            switch (next)
            {
                case GameState.Town:
                case GameState.Dungeon:
                    SwitchContext(InputContext.Gameplay);
                    break;
            }
        }

        /// <summary>
        /// 입력 맥락을 전환합니다.
        /// 이전 맥락의 Reader 를 비활성화하고 새 맥락의 Reader 를 활성화합니다.
        /// 동일한 컨텍스트로의 전환은 무시됩니다.
        /// </summary>
        /// <param name="context">전환할 입력 맥락</param>
        public void SwitchContext(InputContext context)
        {
            if (_currentContext == context) return;

            _currentContext = context;
            _playerInputReader.Disable();
            _uiInputReader.Disable();

            switch (context)
            {
                case InputContext.Gameplay:
                    _playerInputReader.Enable();
                    break;
                case InputContext.UI:
                    _uiInputReader.Enable();
                    break;
            }

            Debug.Log($"[InputManager] 컨텍스트 전환 → {context}");
        }

        /// <summary>
        /// 오브젝트 파괴 시 모든 InputAction 과 이벤트를 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            if (ServiceLocator.IsRegistered<IGameStateService>())
                ServiceLocator.Get<IGameStateService>().OnStateChanged -= OnGameStateChanged;

            _playerInputReader?.Dispose();
            _uiInputReader?.Dispose();
        }
    }
}
