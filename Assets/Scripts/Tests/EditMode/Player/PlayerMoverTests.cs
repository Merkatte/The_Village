using NUnit.Framework;
using Game.Player.Movement;
using UnityEngine;

namespace Game.Tests.EditMode.Player
{
    /// <summary>
    /// PlayerMover 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 초기 Position 검증
    ///   [v] 속도 0 이면 위치 불변
    ///   [v] 정방향 이동 후 위치 변화
    ///   [v] direction.zero 이면 이동 없음
    ///   [v] normalized 정규화 적용 확인
    ///   [v] deltaTime 비례 이동 확인
    ///   [v] 음수 speed 입력 시 0으로 처리
    ///   [v] SetPosition 즉시 반영
    /// </summary>
    [TestFixture]
    public class PlayerMoverTests
    {
        // ─ 초기 상태 ──────────────────────────────────────────

        [Test]
        [Description("초기 Position 은 생성자에 전달한 startPosition 이어야 한다.")]
        public void 초기_Position은_startPosition이다()
        {
            var start = new Vector2(3f, 4f);
            var mover = new PlayerMover(5f, start);

            Assert.AreEqual(start, mover.Position);
        }

        [Test]
        [Description("startPosition 을 생략하면 초기 Position 은 zero 이어야 한다.")]
        public void startPosition_생략시_초기_Position은_zero()
        {
            var mover = new PlayerMover(5f);

            Assert.AreEqual(Vector2.zero, mover.Position);
        }

        // ─ 속도 방어 ──────────────────────────────────────────

        [Test]
        [Description("speed 가 0 이면 Move 를 호출해도 Position 이 변하지 않아야 한다.")]
        public void 속도가_0이면_Move_후_Position_불변()
        {
            var mover = new PlayerMover(speed: 0f);
            mover.Move(Vector2.right, deltaTime: 1f);

            Assert.AreEqual(Vector2.zero, mover.Position);
        }

        [Test]
        [Description("음수 speed 를 전달하면 Speed 가 0 으로 처리되어야 한다.")]
        public void 음수_speed_전달시_Speed는_0()
        {
            var mover = new PlayerMover(speed: -10f);

            Assert.AreEqual(0f, mover.Speed);
        }

        // ─ 이동 계산 ──────────────────────────────────────────

        [Test]
        [Description("speed=5, direction=right, deltaTime=1 이면 Position.x 가 5 이어야 한다.")]
        public void 우측_이동시_X좌표가_speed만큼_증가한다()
        {
            var mover = new PlayerMover(speed: 5f);
            mover.Move(Vector2.right, deltaTime: 1f);

            Assert.AreEqual(5f, mover.Position.x, delta: 0.001f);
        }

        [Test]
        [Description("direction 이 zero 이면 Move 를 호출해도 Position 이 변하지 않아야 한다.")]
        public void direction이_zero이면_Move_후_Position_불변()
        {
            var mover = new PlayerMover(speed: 5f);
            mover.Move(Vector2.zero, deltaTime: 1f);

            Assert.AreEqual(Vector2.zero, mover.Position);
        }

        [Test]
        [Description("대각선 방향은 normalized 후 speed 를 곱하므로 이동 거리가 speed * deltaTime 과 같아야 한다.")]
        public void 대각선_이동은_normalized_속도를_유지한다()
        {
            var mover     = new PlayerMover(speed: 5f);
            var diagonal  = new Vector2(1f, 1f);
            mover.Move(diagonal, deltaTime: 1f);

            var distance = mover.Position.magnitude;
            Assert.AreEqual(5f, distance, delta: 0.001f);
        }

        [Test]
        [Description("deltaTime 이 2배이면 이동 거리도 2배여야 한다.")]
        public void deltaTime_2배시_이동거리_2배()
        {
            var mover1 = new PlayerMover(speed: 5f);
            var mover2 = new PlayerMover(speed: 5f);

            mover1.Move(Vector2.up, deltaTime: 1f);
            mover2.Move(Vector2.up, deltaTime: 2f);

            Assert.AreEqual(mover1.Position.y * 2f, mover2.Position.y, delta: 0.001f);
        }

        // ─ SetPosition ────────────────────────────────────────

        [Test]
        [Description("SetPosition 을 호출하면 Position 이 즉시 해당 좌표로 변경되어야 한다.")]
        public void SetPosition_호출시_Position_즉시_변경()
        {
            var mover  = new PlayerMover(speed: 5f);
            var target = new Vector2(10f, 20f);
            mover.SetPosition(target);

            Assert.AreEqual(target, mover.Position);
        }

        [Test]
        [Description("SetPosition 후 Move 를 호출하면 새 위치 기준으로 이동해야 한다.")]
        public void SetPosition_후_Move는_새_위치_기준으로_이동한다()
        {
            var mover = new PlayerMover(speed: 5f);
            mover.SetPosition(new Vector2(10f, 0f));
            mover.Move(Vector2.right, deltaTime: 1f);

            Assert.AreEqual(15f, mover.Position.x, delta: 0.001f);
        }
    }
}
