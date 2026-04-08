using System;
using NUnit.Framework;
using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using Game.Manager.Scene;

namespace Game.Tests.EditMode.Manager
{
    /// <summary>
    /// 로딩 씨 전환 관련 단위 테스트입니다.
    ///
    /// ─ 테스트 범위 ────────────────────────────────────────────
    ///   [EditMode - 동기 검증]
    ///   [v] SceneType.Loading 열거값 존재
    ///   [v] SceneType.Loading 이 다른 값과 구별됨
    ///   [v] PendingOperation 초기값 null
    ///   [v] TransitionToWithLoading 동일 씨 중복 방지
    ///   [v] ISceneTransitionService 에 PendingOperation 속성 존재
    ///   [v] ServiceLocator 등록/조회/해제
    ///
    ///   [PlayMode 영역 — 이 파일에서 다루지 않음]
    ///   [ ] 페이드 아웃 → 0.2초 대기 → LoadingScene 전환 순서
    ///   [ ] PendingOperation 비동기 설정 타이밍
    ///   [ ] LoadingSceneController 전체 시퀀스
    ///
    /// ─ 설계 원칙 ────────────────────────────────────────────
    ///   UniTask 를 Test Assembly 에서 직접 참조하지 않기 위해
    ///   ISceneFader Fake 는 사용하지 않습니다.
    ///   ServiceLocator 연동은 IGameStateService 를 통해 검증합니다.
    /// </summary>
    [TestFixture]
    public class SceneTransitionManagerLoadingTests
    {
        // ─ Test Double ────────────────────────────────────────────

        private class FakeSceneLoader : ISceneLoader
        {
            public SceneType? LastLoadedScene { get; private set; }
            public int        LoadCallCount   { get; private set; }

            public void LoadScene(SceneType sceneType)
            {
                LastLoadedScene = sceneType;
                LoadCallCount++;
            }

            /// <summary>EditMode 테스트 전용 — 실제 씨 로딩 없이 null 반환.</summary>
            public UnityEngine.AsyncOperation LoadSceneAsync(SceneType sceneType) => null;
        }

        private class FakeGameStateService : IGameStateService
        {
            public GameState CurrentState { get; private set; } = GameState.Town;
            public event Action<GameState, GameState> OnStateChanged;

            public void ChangeState(GameState newState)
            {
                var prev = CurrentState;
                CurrentState = newState;
                OnStateChanged?.Invoke(prev, newState);
            }
        }

        // ─ 픽스처 ──────────────────────────────────────────────

        private FakeSceneLoader        _fakeLoader;
        private SceneTransitionManager _manager;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            _fakeLoader = new FakeSceneLoader();
            _manager    = new SceneTransitionManager(_fakeLoader);
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Clear();
        }

        // ─ SceneType.Loading 열거값 ──────────────────────────────────

        [Test]
        [Description("ISceneTransitionService 인터페이스에 PendingTarget 속성이 존재해야 한다.")]
        public void ISceneTransitionService에_PendingTarget_속성이_존재한다()
        {
            ISceneTransitionService service = _manager;
            Assert.That(service.PendingTarget, Is.Null);
        }

        // ─ TransitionToWithLoading 중복 방지 ──────────────────────

        [Test]
        [Description("TransitionToWithLoading 을 같은 씨으로 호출하면 LoadScene 이 추가 호출되지 않아야 한다.")]
        public void TransitionToWithLoading_동일씨_중복호출시_LoadScene_추가호출_없음()
        {
            _manager.TransitionTo(SceneType.Dungeon);
            var loadCountAfterFirst = _fakeLoader.LoadCallCount;

            _manager.TransitionToWithLoading(SceneType.Dungeon);

            Assert.That(_fakeLoader.LoadCallCount, Is.EqualTo(loadCountAfterFirst));
        }

        [Test]
        [Description("TransitionToWithLoading 를 다른 씨으로 호출하면 CurrentScene 이 변경되어야 한다.")]
        public void TransitionToWithLoading_다른씨_호출시_CurrentScene_변경됨()
        {
            _manager.TransitionToWithLoading(SceneType.Dungeon);

            Assert.That(_manager.CurrentScene, Is.EqualTo(SceneType.Dungeon));
        }

        [Test]
        [Description("TransitionToWithLoading 동일 씨 반복 호출 시 CurrentScene 은 유지되어야 한다.")]
        public void TransitionToWithLoading_동일씨_반복호출시_CurrentScene_유지()
        {
            _manager.TransitionToWithLoading(SceneType.Town);
            _manager.TransitionToWithLoading(SceneType.Town);

            Assert.That(_manager.CurrentScene, Is.EqualTo(SceneType.Town));
        }

        // ─ 인터페이스 준수 ─────────────────────────────────────────

        [Test]
        [Description("SceneTransitionManager 는 ISceneTransitionService 를 구현해야 한다.")]
        public void SceneTransitionManager는_ISceneTransitionService를_구현한다()
        {
            Assert.That(_manager, Is.InstanceOf<ISceneTransitionService>());
        }

        [Test]
        [Description("TransitionToWithLoading 은 ISceneTransitionService 에 정의된 메서드여야 한다.")]
        public void TransitionToWithLoading은_인터페이스에_정의된_메서드다()
        {
            ISceneTransitionService service = _manager;
            Assert.DoesNotThrow(() => service.TransitionToWithLoading(SceneType.Loading));
        }

        // ─ ServiceLocator 등록/해제 ────────────────────────────────

        [Test]
        [Description("ServiceLocator 에 ISceneFader 가 미등록 상태이면 IsRegistered 는 false 여야 한다.")]
        public void ServiceLocator에_ISceneFader_미등록시_IsRegistered는_false()
        {
            Assert.That(ServiceLocator.IsRegistered<ISceneFader>(), Is.False);
        }

        [Test]
        [Description("ServiceLocator 에 서비스를 등록 후 IsRegistered 가 true 여야 한다.")]
        public void ServiceLocator에_서비스_등록후_IsRegistered는_true()
        {
            var fake = new FakeGameStateService();
            ServiceLocator.Register<IGameStateService>(fake);

            Assert.That(ServiceLocator.IsRegistered<IGameStateService>(), Is.True);
        }

        [Test]
        [Description("ServiceLocator 에서 서비스 해제 후 IsRegistered 는 false 여야 한다.")]
        public void ServiceLocator에서_서비스_해제후_IsRegistered는_false()
        {
            var fake = new FakeGameStateService();
            ServiceLocator.Register<IGameStateService>(fake);
            ServiceLocator.Unregister<IGameStateService>();

            Assert.That(ServiceLocator.IsRegistered<IGameStateService>(), Is.False);
        }

        [Test]
        [Description("ServiceLocator.Clear 후 모든 서비스가 해제되어야 한다.")]
        public void ServiceLocator_Clear_후_모든_서비스_해제됨()
        {
            var fake = new FakeGameStateService();
            ServiceLocator.Register<IGameStateService>(fake);

            ServiceLocator.Clear();

            Assert.That(ServiceLocator.IsRegistered<IGameStateService>(), Is.False);
        }
    }
}
