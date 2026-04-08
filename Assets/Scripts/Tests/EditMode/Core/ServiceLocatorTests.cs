using NUnit.Framework;
using System;
using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;

namespace Game.Tests.EditMode.Core
{
    /// <summary>
    /// ServiceLocator 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 등록 후 동일 인스턴스 반환
    ///   [v] 미등록 서비스 조회 시 예외
    ///   [v] IsRegistered 정확성
    ///   [v] Clear 후 서비스 소멸
    ///   [v] null 등록 시 예외
    /// </summary>
    [TestFixture]
    public class ServiceLocatorTests
    {
        // ─ 테스트용 Fake 구현체 ───────────────────────────────

        /// <summary>
        /// IGameStateService 의 최소 구현 Fake 입니다.
        /// 실제 동작 없이 인터페이스 규약만 만족합니다.
        /// </summary>
        private class FakeGameStateService : IGameStateService
        {
            public GameState CurrentState => GameState.None;
            public event Action<GameState, GameState> OnStateChanged;
            public void ChangeState(GameState newState) { }
        }

        // ─ 픽스처 ────────────────────────────────────────────

        [SetUp]
        public void SetUp()
        {
            // 각 테스트가 독립된 상태에서 시작하도록 초기화합니다.
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            // 다른 테스트에 영향을 주지 않도록 정리합니다.
            ServiceLocator.Clear();
        }

        // ─ 테스트 케이스 ──────────────────────────────────────

        [Test]
        [Description("Register 후 Get 을 호출하면 동일한 인스턴스를 반환해야 한다.")]
        public void Register_후_Get은_동일한_인스턴스를_반환한다()
        {
            var fake = new FakeGameStateService();
            ServiceLocator.Register<IGameStateService>(fake);

            var result = ServiceLocator.Get<IGameStateService>();

            Assert.AreSame(fake, result);
        }

        [Test]
        [Description("등록되지 않은 서비스를 Get 하면 InvalidOperationException 이 발생해야 한다.")]
        public void 미등록_서비스_Get_호출시_InvalidOperationException_발생()
        {
            Assert.Throws<InvalidOperationException>(
                () => ServiceLocator.Get<IGameStateService>());
        }

        [Test]
        [Description("등록 전 IsRegistered 는 false 를 반환해야 한다.")]
        public void IsRegistered_등록전_false_반환()
        {
            Assert.IsFalse(ServiceLocator.IsRegistered<IGameStateService>());
        }

        [Test]
        [Description("등록 후 IsRegistered 는 true 를 반환해야 한다.")]
        public void IsRegistered_등록후_true_반환()
        {
            ServiceLocator.Register<IGameStateService>(new FakeGameStateService());

            Assert.IsTrue(ServiceLocator.IsRegistered<IGameStateService>());
        }

        [Test]
        [Description("Clear 호출 후에는 이전에 등록된 서비스를 조회할 수 없어야 한다.")]
        public void Clear_호출후_서비스_조회_불가()
        {
            ServiceLocator.Register<IGameStateService>(new FakeGameStateService());
            ServiceLocator.Clear();

            Assert.Throws<InvalidOperationException>(
                () => ServiceLocator.Get<IGameStateService>());
        }

        [Test]
        [Description("null 구현체를 Register 하면 ArgumentNullException 이 발생해야 한다.")]
        public void null_구현체_Register_시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(
                () => ServiceLocator.Register<IGameStateService>(null));
        }

        [Test]
        [Description("동일 타입을 재등록하면 이전 구현체를 덮어써야 한다.")]
        public void 재등록시_이전_구현체를_덮어쓴다()
        {
            var first  = new FakeGameStateService();
            var second = new FakeGameStateService();

            ServiceLocator.Register<IGameStateService>(first);
            ServiceLocator.Register<IGameStateService>(second);

            Assert.AreSame(second, ServiceLocator.Get<IGameStateService>());
        }

        [Test]
        public void Unregister_등록된_서비스를_정상_해제한다()
        {
            ServiceLocator.Register<IGameStateService>(new FakeGameStateService());
            ServiceLocator.Unregister<IGameStateService>();

            Assert.That(ServiceLocator.IsRegistered<IGameStateService>(), Is.False);
        }

        [Test]
        public void Unregister_등록되지_않은_타입_해제시_예외가_발생한다()
        {
            Assert.Throws<System.InvalidOperationException>(
                () => ServiceLocator.Unregister<IGameStateService>());
        }
    }
}
