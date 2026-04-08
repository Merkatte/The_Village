using NUnit.Framework;
using Game.Core.Enums;
using Game.Dungeon.Resource;

namespace Game.Tests.EditMode.Dungeon
{
    /// <summary>
    /// ResourceDrop 구조체 단위 테스트입니다.
    /// </summary>
    [TestFixture]
    public class ResourceDropTests
    {
        [Test]
        public void 정상_생성시_Type과_Amount가_올바르게_설정된다()
        {
            var drop = new ResourceDrop(ResourceType.Wood, 3);

            Assert.That(drop.Type,   Is.EqualTo(ResourceType.Wood));
            Assert.That(drop.Amount, Is.EqualTo(3));
        }

        [Test]
        public void Amount가_0이하이면_1로_보정된다()
        {
            var drop = new ResourceDrop(ResourceType.Stone, 0);
            Assert.That(drop.Amount, Is.EqualTo(1));
        }

        [Test]
        public void Amount가_음수이면_1로_보정된다()
        {
            var drop = new ResourceDrop(ResourceType.IronOre, -5);
            Assert.That(drop.Amount, Is.EqualTo(1));
        }
    }

    /// <summary>
    /// DropEntry 단위 테스트입니다.
    /// </summary>
    [TestFixture]
    public class DropEntryTests
    {
        [Test]
        public void 정상_생성시_모든_값이_올바르게_설정된다()
        {
            var entry = new DropEntry(ResourceType.Wood, 0.8f, 1, 3);

            Assert.That(entry.ResourceType, Is.EqualTo(ResourceType.Wood));
            Assert.That(entry.DropChance,   Is.EqualTo(0.8f).Within(0.001f));
            Assert.That(entry.MinAmount,    Is.EqualTo(1));
            Assert.That(entry.MaxAmount,    Is.EqualTo(3));
        }

        [Test]
        public void DropChance가_1초과이면_1로_클램프된다()
        {
            var entry = new DropEntry(ResourceType.Stone, 2.0f, 1, 1);
            Assert.That(entry.DropChance, Is.EqualTo(1.0f).Within(0.001f));
        }

        [Test]
        public void DropChance가_음수이면_0으로_클램프된다()
        {
            var entry = new DropEntry(ResourceType.Stone, -0.5f, 1, 1);
            Assert.That(entry.DropChance, Is.EqualTo(0.0f).Within(0.001f));
        }

        [Test]
        public void MinAmount가_0이하이면_1로_보정된다()
        {
            var entry = new DropEntry(ResourceType.IronOre, 1.0f, 0, 5);
            Assert.That(entry.MinAmount, Is.EqualTo(1));
        }

        [Test]
        public void MaxAmount가_MinAmount보다_작으면_MinAmount로_보정된다()
        {
            var entry = new DropEntry(ResourceType.GoldOre, 1.0f, 5, 2);
            Assert.That(entry.MaxAmount, Is.EqualTo(5));
        }
    }
}
