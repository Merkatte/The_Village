using NUnit.Framework;
using System;
using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Manager.Interfaces.Input;
using UnityEngine;

namespace Game.Tests.EditMode.Manager
{
    /// <summary>
    /// InputManager 의 핵심 로직을 담당하는 단위 테스트입니다.
    ///
    /// InputManager 자체는 MonoBehaviour 이므로 직접 테스트 불가합니다.
    /// 대신 InputManager 가 의존하는 IPlayerInputReader / IUIInputReader 인터페이스를
    /// Fake 로 교체하여 컨텍스트 전환 로직을 검증합니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] Fake 등록 후 ServiceLocator 로 올바른 인스턴스 반환
    ///   [v] IPlayerInputReader 이벤트 발행 → 구독자 수신
    ///   [v] IUIInputReader 이벤트 발행 → 구독자 수신
    ///   [v] Enable / Disable 호출 횟수 검증
    /// </summary>
    [TestFixture]
    public class InputManagerTests
    {
        // ─ Fake 구현체 ────────────────────────────────────────

        /// <summary>
        /// IPlayerInputReader 의 Fake 구현체입니다.
        /// 이벤트를 직접 발행하고 Enable / Disable 호출 횟수를 기록합니다.
        /// </summary>
        private class FakePlayerInputReader : IPlayerInputReader
        {
            public event Action<Vector2> OnMovePerformed;
            public event Action          OnMoveCancelled;
            public event Action          OnHarvestStarted;
            public event Action          OnHarvestCancelled;

            public int EnableCallCount  { get; private set; }
            public int DisableCallCount { get; private set; }

            public void Enable()  => EnableCallCount++;
            public void Disable() => DisableCallCount++;

            /// <summary>테스트에서 이동 입력을 강제 발행합니다.</summary>
            public void FireMove(Vector2 direction) => OnMovePerformed?.Invoke(direction);

            /// <summary>테스트에서 이동 취소를 강제 발행합니다.</summary>
            public void FireCancel() => OnMoveCancelled?.Invoke();
        }

        /// <summary>
        /// IUIInputReader 의 Fake 구현체입니다.
        /// </summary>
        private class FakeUIInputReader : IUIInputReader
        {
            public event Action OnSubmitPerformed;
            public event Action OnCancelPerformed;

            public int EnableCallCount  { get; private set; }
            public int DisableCallCount { get; private set; }

            public void Enable()  => EnableCallCount++;
            public void Disable() => DisableCallCount++;

            /// <summary>테스트에서 Submit 입력을 강제 발행합니다.</summary>
            public void FireSubmit() => OnSubmitPerformed?.Invoke();

            /// <summary>테스트에서 Cancel 입력을 강제 발행합니다.</summary>
            public void FireCancel() => OnCancelPerformed?.Invoke();
        }

        // ─ 픽스처 ────────────────────────────────────────────

        private FakePlayerInputReader _fakePlayer;
        private FakeUIInputReader     _fakeUI;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            _fakePlayer = new FakePlayerInputReader();
            _fakeUI     = new FakeUIInputReader();

            ServiceLocator.Register<IPlayerInputReader>(_fakePlayer);
            ServiceLocator.Register<IUIInputReader>(_fakeUI);
        }

        [TearDown]
        public void TearDown() => ServiceLocator.Clear();

        // ─ ServiceLocator 등록 검증 ───────────────────────────

        [Test]
        [Description("ServiceLocator 에 등록된 IPlayerInputReader 는 FakePlayerInputReader 여야 한다.")]
        public void ServiceLocator에서_IPlayerInputReader_조회시_Fake_반환()
        {
            var result = ServiceLocator.Get<IPlayerInputReader>();
            Assert.AreSame(_fakePlayer, result);
        }

        [Test]
        [Description("ServiceLocator 에 등록된 IUIInputReader 는 FakeUIInputReader 여야 한다.")]
        public void ServiceLocator에서_IUIInputReader_조회시_Fake_반환()
        {
            var result = ServiceLocator.Get<IUIInputReader>();
            Assert.AreSame(_fakeUI, result);
        }

        // ─ IPlayerInputReader 이벤트 검증 ────────────────────

        [Test]
        [Description("FakePlayerInputReader.FireMove 호출 시 OnMovePerformed 구독자가 Vector2 를 수신해야 한다.")]
        public void FakePlayerInputReader_FireMove시_OnMovePerformed_수신()
        {
            Vector2? received = null;
            _fakePlayer.OnMovePerformed += v => received = v;

            _fakePlayer.FireMove(Vector2.right);

            Assert.AreEqual(Vector2.right, received);
        }

        [Test]
        [Description("FakePlayerInputReader.FireCancel 호출 시 OnMoveCancelled 구독자가 호출되어야 한다.")]
        public void FakePlayerInputReader_FireCancel시_OnMoveCancelled_호출()
        {
            var called = false;
            _fakePlayer.OnMoveCancelled += () => called = true;

            _fakePlayer.FireCancel();

            Assert.IsTrue(called);
        }

        // ─ IUIInputReader 이벤트 검증 ─────────────────────────

        [Test]
        [Description("FakeUIInputReader.FireSubmit 호출 시 OnSubmitPerformed 구독자가 호출되어야 한다.")]
        public void FakeUIInputReader_FireSubmit시_OnSubmitPerformed_호출()
        {
            var called = false;
            _fakeUI.OnSubmitPerformed += () => called = true;

            _fakeUI.FireSubmit();

            Assert.IsTrue(called);
        }

        // ─ Enable / Disable 검증 ─────────────────────────────

        [Test]
        [Description("Enable 을 여러 번 호출하면 호출 횟수만큼 EnableCallCount 가 증가해야 한다.")]
        public void Enable_2회_호출시_EnableCallCount_2()
        {
            _fakePlayer.Enable();
            _fakePlayer.Enable();

            Assert.AreEqual(2, _fakePlayer.EnableCallCount);
        }

        [Test]
        [Description("Disable 을 호출하면 DisableCallCount 가 1 증가해야 한다.")]
        public void Disable_호출시_DisableCallCount_증가()
        {
            _fakePlayer.Disable();

            Assert.AreEqual(1, _fakePlayer.DisableCallCount);
        }
    }
}
