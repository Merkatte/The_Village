using NUnit.Framework;
using System;
using Game.Core.Enums;
using Game.Core.Interfaces;
using Game.Manager.Scene;
using UnityEngine.SceneManagement;

namespace Game.Tests.EditMode.Manager
{
    /// <summary>
    /// SceneTransitionManager 의 단위 테스트입니다.
    ///
    /// Unity SceneManager 없이 테스트하기 위해 FakeSceneLoader(Test Double) 를 사용합니다.
    /// → LSP: FakeSceneLoader 는 ISceneLoader 를 완전히 대체 가능합니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 초기 CurrentScene = Bootstrap
    ///   [v] TransitionTo 후 CurrentScene 갱신
    ///   [v] TransitionTo 시 ISceneLoader.LoadScene 호출
    ///   [v] TransitionTo 시 OnSceneTransitioned 이벤트 발행
    ///   [v] 이벤트 인자 정확성
    ///   [v] 동일 씬 전환 시 중복 로딩 방지
    ///   [v] null sceneLoader 주입 시 예외
    /// </summary>
    [TestFixture]
    public class SceneTransitionManagerTests
    {
        // ─ 테스트용 Fake 구현체 ───────────────────────────────

        /// <summary>
        /// 테스트용 ISceneLoader Fake 입니다. (Test Double - Fake Object)
        /// 실제 씬 로딩 없이 호출 횟수와 마지막 전달된 씬 타입을 기록합니다.
        /// </summary>
        private class FakeSceneLoader : ISceneLoader
        {
            /// <summary>마지막으로 LoadScene 에 전달된 씬 타입. 미호출 시 null.</summary>
            public SceneType? LastLoadedScene { get; private set; }

            /// <summary>LoadScene 이 호출된 총 횟수.</summary>
            public int LoadCallCount { get; private set; }

            /// <inheritdoc/>
            public void LoadScene(SceneType sceneType)
            {
                LastLoadedScene = sceneType;
                LoadCallCount++;
            }

            public UnityEngine.AsyncOperation LoadSceneAsync(SceneType sceneType)
            {
                return SceneManager.LoadSceneAsync(sceneType.ToString());
            }
        }

        // ─ 픽스처 ────────────────────────────────────────────

        private FakeSceneLoader _fakeLoader;
        private SceneTransitionManager _manager;

        [SetUp]
        public void SetUp()
        {
            _fakeLoader = new FakeSceneLoader();
            _manager    = new SceneTransitionManager(_fakeLoader);
        }

        // ─ 초기 상태 ──────────────────────────────────────────

        [Test]
        [NUnit.Framework.Description("생성 직후 CurrentScene 은 Bootstrap 이어야 한다.")]
        public void 초기_CurrentScene은_Bootstrap이다()
        {
            Assert.AreEqual(SceneType.Bootstrap, _manager.CurrentScene);
        }

        // ─ 씬 전환 ────────────────────────────────────────────

        [Test]
        [NUnit.Framework.Description("TransitionTo 호출 후 CurrentScene 이 대상 씬으로 변경되어야 한다.")]
        public void TransitionTo_호출후_CurrentScene_변경됨()
        {
            _manager.TransitionTo(SceneType.Town);

            Assert.AreEqual(SceneType.Town, _manager.CurrentScene);
        }

        [Test]
        [NUnit.Framework.Description("TransitionTo 호출 시 ISceneLoader.LoadScene 이 호출되어야 한다.")]
        public void TransitionTo_호출시_ISceneLoader_LoadScene_호출됨()
        {
            _manager.TransitionTo(SceneType.Town);

            Assert.AreEqual(SceneType.Town, _fakeLoader.LastLoadedScene);
        }

        [Test]
        [NUnit.Framework.Description("TransitionTo 호출 시 OnSceneTransitioned 이벤트가 발행되어야 한다.")]
        public void TransitionTo_호출시_OnSceneTransitioned_이벤트_발행()
        {
            SceneType? received = null;
            _manager.OnSceneTransitioned += scene => received = scene;

            _manager.TransitionTo(SceneType.Dungeon);

            Assert.AreEqual(SceneType.Dungeon, received);
        }

        [Test]
        [NUnit.Framework.Description("OnSceneTransitioned 이벤트 인자는 전환된 씬 타입이어야 한다.")]
        public void OnSceneTransitioned_이벤트_인자는_전환된_씬_타입()
        {
            SceneType? received = null;
            _manager.OnSceneTransitioned += scene => received = scene;

            _manager.TransitionTo(SceneType.Town);

            Assert.AreEqual(SceneType.Town, received);
        }

        // ─ 중복 전환 방어 ─────────────────────────────────────

        [Test]
        [NUnit.Framework.Description("동일 씬으로 TransitionTo 를 반복해도 ISceneLoader 는 1회만 호출되어야 한다.")]
        public void 동일_씬으로_반복_TransitionTo시_ISceneLoader_1회_호출()
        {
            _manager.TransitionTo(SceneType.Town);
            _manager.TransitionTo(SceneType.Town); // 중복 호출

            Assert.AreEqual(1, _fakeLoader.LoadCallCount);
        }

        [Test]
        [NUnit.Framework.Description("동일 씬으로 전환 시 OnSceneTransitioned 이벤트가 발행되지 않아야 한다.")]
        public void 동일_씬으로_전환시_이벤트_미발행()
        {
            _manager.TransitionTo(SceneType.Town);

            var callCount = 0;
            _manager.OnSceneTransitioned += _ => callCount++;

            _manager.TransitionTo(SceneType.Town); // 중복 호출

            Assert.AreEqual(0, callCount);
        }

        // ─ 예외 처리 ──────────────────────────────────────────

        [Test]
        [NUnit.Framework.Description("null ISceneLoader 를 주입하면 ArgumentNullException 이 발생해야 한다.")]
        public void null_ISceneLoader_주입시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(
                () => new SceneTransitionManager(null));
        }

        // ─ 연속 전환 ──────────────────────────────────────────

        [Test]
        [NUnit.Framework.Description("여러 번 다른 씬으로 전환해도 ISceneLoader 는 전환 횟수만큼 호출되어야 한다.")]
        public void 연속_전환시_ISceneLoader_호출_횟수가_맞아야_한다()
        {
            _manager.TransitionTo(SceneType.Town);
            _manager.TransitionTo(SceneType.Dungeon);
            _manager.TransitionTo(SceneType.Town);

            Assert.AreEqual(3, _fakeLoader.LoadCallCount);
        }
    }
}
