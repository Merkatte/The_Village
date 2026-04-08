using NUnit.Framework;
using Game.Core.Enums;
using Game.Manager.State;

namespace Game.Tests.EditMode.Manager
{
    /// <summary>
    /// GameStateManager 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 초기 상태 None 검증
    ///   [v] 상태 변경 후 CurrentState 갱신
    ///   [v] 상태 변경 시 OnStateChanged 이벤트 발행
    ///   [v] 이전/새 상태 인자 정확성
    ///   [v] 동일 상태 재전환 시 이벤트 미발행
    ///   [v] 연속 상태 전환 시 순서 보장
    /// </summary>
    [TestFixture]
    public class GameStateManagerTests
    {
        private GameStateManager _manager;

        [SetUp]
        public void SetUp()
        {
            // 각 테스트마다 새 인스턴스를 생성하여 상태 격리를 보장합니다.
            _manager = new GameStateManager();
        }

        // ─ 초기 상태 ──────────────────────────────────────────

        [Test]
        [Description("GameStateManager 는 생성 직후 CurrentState 가 None 이어야 한다.")]
        public void 초기_CurrentState는_None이다()
        {
            Assert.AreEqual(GameState.None, _manager.CurrentState);
        }

        // ─ 상태 변경 ──────────────────────────────────────────

        [Test]
        [Description("ChangeState 호출 후 CurrentState 가 새 값으로 변경되어야 한다.")]
        public void ChangeState_호출후_CurrentState_변경됨()
        {
            _manager.ChangeState(GameState.Town);

            Assert.AreEqual(GameState.Town, _manager.CurrentState);
        }

        [Test]
        [Description("ChangeState 호출 시 OnStateChanged 이벤트가 발행되어야 한다.")]
        public void ChangeState_호출시_OnStateChanged_이벤트_발행()
        {
            var eventFired = false;
            _manager.OnStateChanged += (_, __) => eventFired = true;

            _manager.ChangeState(GameState.Dungeon);

            Assert.IsTrue(eventFired);
        }

        [Test]
        [Description("OnStateChanged 이벤트의 첫 번째 인자는 이전 상태여야 한다.")]
        public void OnStateChanged_첫번째_인자는_이전_상태()
        {
            _manager.ChangeState(GameState.Town);

            GameState capturedPrevious = GameState.None;
            _manager.OnStateChanged += (prev, _) => capturedPrevious = prev;

            _manager.ChangeState(GameState.Dungeon);

            Assert.AreEqual(GameState.Town, capturedPrevious);
        }

        [Test]
        [Description("OnStateChanged 이벤트의 두 번째 인자는 새 상태여야 한다.")]
        public void OnStateChanged_두번째_인자는_새_상태()
        {
            GameState capturedNew = GameState.None;
            _manager.OnStateChanged += (_, next) => capturedNew = next;

            _manager.ChangeState(GameState.Dungeon);

            Assert.AreEqual(GameState.Dungeon, capturedNew);
        }

        // ─ 중복 상태 방어 ─────────────────────────────────────

        [Test]
        [Description("현재와 동일한 상태로 ChangeState 를 호출하면 이벤트가 발행되지 않아야 한다.")]
        public void 동일_상태로_ChangeState_시_이벤트_미발행()
        {
            _manager.ChangeState(GameState.Town);

            var callCount = 0;
            _manager.OnStateChanged += (_, __) => callCount++;

            _manager.ChangeState(GameState.Town); // 동일 상태 재전환

            Assert.AreEqual(0, callCount);
        }

        // ─ 연속 전환 ──────────────────────────────────────────

        [Test]
        [Description("여러 번 상태를 전환해도 마지막 상태가 CurrentState 에 반영되어야 한다.")]
        public void 연속_상태_전환시_마지막_상태가_반영됨()
        {
            _manager.ChangeState(GameState.Town);
            _manager.ChangeState(GameState.Dungeon);
            _manager.ChangeState(GameState.Town);

            Assert.AreEqual(GameState.Town, _manager.CurrentState);
        }

        [Test]
        [Description("연속 전환 시 OnStateChanged 이벤트가 각 전환마다 발행되어야 한다.")]
        public void 연속_전환시_이벤트_발행_횟수가_전환_횟수와_같다()
        {
            var callCount = 0;
            _manager.OnStateChanged += (_, __) => callCount++;

            _manager.ChangeState(GameState.Town);
            _manager.ChangeState(GameState.Dungeon);
            _manager.ChangeState(GameState.Town);

            Assert.AreEqual(3, callCount);
        }

        [Test]
        [Description("None 으로 상태를 되돌리면 CurrentState 가 None 이 되어야 한다.")]
        public void None으로_복귀_전환시_CurrentState가_None이다()
        {
            _manager.ChangeState(GameState.Dungeon);
            _manager.ChangeState(GameState.None);

            Assert.AreEqual(GameState.None, _manager.CurrentState);
        }
    }
}
