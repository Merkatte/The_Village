using System.Collections.Generic;
using NUnit.Framework;
using Game.Core.Enums;
using Game.Dungeon.Resource;

namespace Game.Tests.EditMode.Dungeon
{
    /// <summary>
    /// 테스트용 고정 난수 생성기입니다.
    /// NextFloat 와 NextInt 를 원하는 값으로 고정할 수 있습니다.
    /// </summary>
    internal sealed class FakeRandom : IRandom
    {
        /// <summary>NextFloat() 가 반환할 값</summary>
        public float FixedFloat { get; set; }

        /// <summary>NextInt() 가 반환할 값</summary>
        public int FixedInt { get; set; }

        public float NextFloat() => FixedFloat;
        public int NextInt(int min, int max) => FixedInt;
    }

    /// <summary>
    /// DropTableImpl 단위 테스트입니다.
    ///
    /// ─ 검증 대상 ──────────────────────────────────────────────
    ///   - 확률 판정 성공/실패 여부
    ///   - 수량이 Min ~ Max 범위 내에서 결정되는가
    ///   - 여러 항목 중 일부만 드롭되는 경우
    ///   - null 인자 예외 처리
    /// </summary>
    [TestFixture]
    public class DropTableImplTests
    {
        // ─ 확률 판정 ───────────────────────────────────────────

        [Test]
        public void 확률_판정_성공시_해당_자원이_결과에_포함된다()
        {
            // DropChance 0.5, random 0.3 → 0.3 < 0.5 → 성공
            var fake    = new FakeRandom { FixedFloat = 0.3f, FixedInt = 2 };
            var entries = new List<DropEntry> { new DropEntry(ResourceType.Wood, 0.5f, 1, 3) };
            var table   = new DropTableImpl(entries);

            var result = table.Roll(fake);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Type, Is.EqualTo(ResourceType.Wood));
        }

        [Test]
        public void 확률_판정_실패시_결과가_비어있다()
        {
            // DropChance 0.5, random 0.7 → 0.7 >= 0.5 → 실패
            var fake    = new FakeRandom { FixedFloat = 0.7f, FixedInt = 1 };
            var entries = new List<DropEntry> { new DropEntry(ResourceType.Wood, 0.5f, 1, 3) };
            var table   = new DropTableImpl(entries);

            var result = table.Roll(fake);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void DropChance가_1이면_항상_드롭된다()
        {
            var fake    = new FakeRandom { FixedFloat = 0.9999f, FixedInt = 1 };
            var entries = new List<DropEntry> { new DropEntry(ResourceType.Stone, 1.0f, 1, 1) };
            var table   = new DropTableImpl(entries);

            var result = table.Roll(fake);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void DropChance가_0이면_절대_드롭되지_않는다()
        {
            var fake    = new FakeRandom { FixedFloat = 0.0f, FixedInt = 1 };
            var entries = new List<DropEntry> { new DropEntry(ResourceType.Stone, 0.0f, 1, 1) };
            var table   = new DropTableImpl(entries);

            var result = table.Roll(fake);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        // ─ 수량 결정 ───────────────────────────────────────────

        [Test]
        public void 드롭_성공시_수량이_FakeRandom_FixedInt값과_같다()
        {
            var fake    = new FakeRandom { FixedFloat = 0.0f, FixedInt = 3 };
            var entries = new List<DropEntry> { new DropEntry(ResourceType.IronOre, 1.0f, 1, 5) };
            var table   = new DropTableImpl(entries);

            var result = table.Roll(fake);

            Assert.That(result[0].Amount, Is.EqualTo(3));
        }

        // ─ 여러 항목 ───────────────────────────────────────────

        [Test]
        public void 여러_항목_중_성공한_항목만_결과에_포함된다()
        {
            // FixedFloat = 0.3 → DropChance 0.5 는 성공, DropChance 0.1 은 실패
            var fake = new FakeRandom { FixedFloat = 0.3f, FixedInt = 1 };
            var entries = new List<DropEntry>
            {
                new DropEntry(ResourceType.Wood,  0.5f, 1, 1),   // 성공 (0.3 < 0.5)
                new DropEntry(ResourceType.Stone, 0.1f, 1, 1),   // 실패 (0.3 >= 0.1)
            };
            var table = new DropTableImpl(entries);

            var result = table.Roll(fake);

            Assert.That(result.Count,      Is.EqualTo(1));
            Assert.That(result[0].Type,    Is.EqualTo(ResourceType.Wood));
        }

        [Test]
        public void 모든_항목_성공시_전부_결과에_포함된다()
        {
            var fake = new FakeRandom { FixedFloat = 0.0f, FixedInt = 1 };
            var entries = new List<DropEntry>
            {
                new DropEntry(ResourceType.Wood,    1.0f, 1, 1),
                new DropEntry(ResourceType.Stone,   1.0f, 1, 1),
                new DropEntry(ResourceType.IronOre, 1.0f, 1, 1),
            };
            var table = new DropTableImpl(entries);

            var result = table.Roll(fake);

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void 항목이_없으면_빈_결과를_반환한다()
        {
            var fake  = new FakeRandom { FixedFloat = 0.0f, FixedInt = 1 };
            var table = new DropTableImpl(new List<DropEntry>());

            var result = table.Roll(fake);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        // ─ 예외 처리 ───────────────────────────────────────────

        [Test]
        public void entries가_null이면_ArgumentNullException_발생()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new DropTableImpl(null));
        }

        [Test]
        public void random이_null이면_ArgumentNullException_발생()
        {
            var table = new DropTableImpl(new List<DropEntry>());
            Assert.Throws<System.ArgumentNullException>(
                () => table.Roll(null));
        }
    }
}
