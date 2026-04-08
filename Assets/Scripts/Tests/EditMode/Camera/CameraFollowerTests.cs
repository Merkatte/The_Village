using NUnit.Framework;
using UnityEngine;
using Game.Camera;

namespace Game.Tests.EditMode.Camera
{
    /// <summary>
    /// CameraFollower 단위 테스트입니다.
    ///
    /// ─ 검증 대상 ──────────────────────────────────────────────
    ///   - 타겟이 범위 내에 있을 때 카메라가 타겟을 따라가는가
    ///   - 타겟이 범위를 벗어났을 때 카메라가 경계에서 멈추는가
    ///   - 정확히 경계선에 있을 때 처리가 올바른가
    /// </summary>
    [TestFixture]
    public class CameraFollowerTests
    {
        // 테스트 공통 범위: X(-10~10), Y(-8~8)
        private static readonly Vector2 BoundsMin = new Vector2(-10f, -8f);
        private static readonly Vector2 BoundsMax = new Vector2(10f,  8f);

        private CameraFollower CreateFollower()
            => new CameraFollower(BoundsMin, BoundsMax);

        // ─ 범위 내 팔로우 ──────────────────────────────────────

        [Test]
        public void 타겟이_범위_내에_있으면_타겟_위치를_반환한다()
        {
            var follower = CreateFollower();
            var target   = new Vector2(3f, 2f);

            var result = follower.CalculatePosition(target);

            Assert.That(result, Is.EqualTo(target));
        }

        [Test]
        public void 타겟이_원점이면_원점을_반환한다()
        {
            var follower = CreateFollower();

            var result = follower.CalculatePosition(Vector2.zero);

            Assert.That(result, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void 타겟이_범위_최소값과_같으면_최소값을_반환한다()
        {
            var follower = CreateFollower();

            var result = follower.CalculatePosition(BoundsMin);

            Assert.That(result, Is.EqualTo(BoundsMin));
        }

        [Test]
        public void 타겟이_범위_최대값과_같으면_최대값을_반환한다()
        {
            var follower = CreateFollower();

            var result = follower.CalculatePosition(BoundsMax);

            Assert.That(result, Is.EqualTo(BoundsMax));
        }

        // ─ X축 경계 초과 ──────────────────────────────────────

        [Test]
        public void 타겟_X가_최대값을_초과하면_X는_최대값으로_고정된다()
        {
            var follower = CreateFollower();
            var target   = new Vector2(20f, 0f);   // X 초과

            var result = follower.CalculatePosition(target);

            Assert.That(result.x, Is.EqualTo(BoundsMax.x));
            Assert.That(result.y, Is.EqualTo(0f));
        }

        [Test]
        public void 타겟_X가_최소값_미만이면_X는_최소값으로_고정된다()
        {
            var follower = CreateFollower();
            var target   = new Vector2(-20f, 0f);  // X 미달

            var result = follower.CalculatePosition(target);

            Assert.That(result.x, Is.EqualTo(BoundsMin.x));
            Assert.That(result.y, Is.EqualTo(0f));
        }

        // ─ Y축 경계 초과 ──────────────────────────────────────

        [Test]
        public void 타겟_Y가_최대값을_초과하면_Y는_최대값으로_고정된다()
        {
            var follower = CreateFollower();
            var target   = new Vector2(0f, 50f);   // Y 초과

            var result = follower.CalculatePosition(target);

            Assert.That(result.x, Is.EqualTo(0f));
            Assert.That(result.y, Is.EqualTo(BoundsMax.y));
        }

        [Test]
        public void 타겟_Y가_최소값_미만이면_Y는_최소값으로_고정된다()
        {
            var follower = CreateFollower();
            var target   = new Vector2(0f, -50f);  // Y 미달

            var result = follower.CalculatePosition(target);

            Assert.That(result.x, Is.EqualTo(0f));
            Assert.That(result.y, Is.EqualTo(BoundsMin.y));
        }

        // ─ XY 동시 초과 ───────────────────────────────────────

        [Test]
        public void 타겟이_XY_모두_초과하면_XY_모두_경계로_고정된다()
        {
            var follower = CreateFollower();
            var target   = new Vector2(100f, 100f);

            var result = follower.CalculatePosition(target);

            Assert.That(result.x, Is.EqualTo(BoundsMax.x));
            Assert.That(result.y, Is.EqualTo(BoundsMax.y));
        }

        // ─ Bounds 프로퍼티 ────────────────────────────────────

        [Test]
        public void BoundsMin_프로퍼티가_생성자_값과_일치한다()
        {
            var follower = CreateFollower();
            Assert.That(follower.BoundsMin, Is.EqualTo(BoundsMin));
        }

        [Test]
        public void BoundsMax_프로퍼티가_생성자_값과_일치한다()
        {
            var follower = CreateFollower();
            Assert.That(follower.BoundsMax, Is.EqualTo(BoundsMax));
        }

        // ─ 퇴화 범위 (BoundsMin == BoundsMax) ────────────────

        [Test]
        public void BoundsMin과_BoundsMax가_같으면_타겟_무관하게_해당_좌표를_반환한다()
        {
            var point    = new Vector2(3f, 5f);
            var follower = new CameraFollower(point, point);

            var result = follower.CalculatePosition(new Vector2(100f, -200f));

            Assert.That(result, Is.EqualTo(point));
        }

        // ─ 인터페이스 구현 ────────────────────────────────────

        [Test]
        public void CameraFollower는_ICameraFollower를_구현한다()
        {
            var follower = CreateFollower();
            Assert.That(follower, Is.InstanceOf<ICameraFollower>());
        }
    }
}
