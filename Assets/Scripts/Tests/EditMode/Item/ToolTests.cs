using NUnit.Framework;
using Game.Core.Enums;
using Game.Item;
using Game.Item.Tool;

namespace Game.Tests.EditMode.Item
{
    /// <summary>
    /// HarvestTool 클래스 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 정상 생성 시 ItemId / ItemName / Grade 반영
    ///   [v] 기본값으로 생성 시 배율이 1.0
    ///   [v] HarvestDurationMultiplier 정상 범위 (0.1 ~ 1.0) 내 값 유지
    ///   [v] HarvestDurationMultiplier 0.1 미만 입력 시 0.1로 클램프
    ///   [v] HarvestDurationMultiplier 1.0 초과 입력 시 1.0으로 클램프
    ///   [v] YieldMultiplier 1.0 이상이면 그대로 유지
    ///   [v] YieldMultiplier 1.0 미만 입력 시 1.0으로 보정
    ///   [v] Data 프로퍼티가 자기 자신을 반환
    ///   [v] ITool 인터페이스 구현 여부
    /// </summary>
    [TestFixture]
    public class HarvestToolTests
    {
        // ─ 정상 생성 ──────────────────────────────────────────

        [Test]
        [Description("정상 생성 시 ItemId 가 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_ItemId가_올바르게_설정된다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe);

            Assert.That(tool.ItemId, Is.EqualTo("axe_iron"));
        }

        [Test]
        [Description("정상 생성 시 ItemName 이 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_ItemName이_올바르게_설정된다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe);

            Assert.That(tool.ItemName, Is.EqualTo("철 도끼"));
        }

        [Test]
        [Description("정상 생성 시 Grade 가 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_Grade가_올바르게_설정된다()
        {
            var tool = new HarvestTool("axe_epic", "영웅 도끼", ItemGrade.Epic, ToolType.Axe);

            Assert.That(tool.Grade, Is.EqualTo(ItemGrade.Epic));
        }

        [Test]
        [Description("배율을 생략하면 HarvestDurationMultiplier 기본값은 1.0 이어야 한다.")]
        public void 기본값_생성시_HarvestDurationMultiplier가_1이다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(1.0f).Within(0.001f));
        }

        [Test]
        [Description("배율을 생략하면 YieldMultiplier 기본값은 1.0 이어야 한다.")]
        public void 기본값_생성시_YieldMultiplier가_1이다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe);

            Assert.That(tool.YieldMultiplier, Is.EqualTo(1.0f).Within(0.001f));
        }

        // ─ HarvestDurationMultiplier 클램프 ───────────────────

        [Test]
        [Description("HarvestDurationMultiplier 가 0.5 이면 그대로 유지되어야 한다.")]
        public void HarvestDurationMultiplier_정상값_그대로_유지된다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe,
                harvestDurationMultiplier: 0.5f);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        [Description("HarvestDurationMultiplier 가 0.1 이면 경계값으로 그대로 유지되어야 한다.")]
        public void HarvestDurationMultiplier_최소경계값_그대로_유지된다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe,
                harvestDurationMultiplier: 0.1f);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(0.1f).Within(0.001f));
        }

        [Test]
        [Description("HarvestDurationMultiplier 가 0.1 미만이면 0.1 로 클램프되어야 한다.")]
        public void HarvestDurationMultiplier_0_1_미만_입력시_0_1로_클램프된다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe,
                harvestDurationMultiplier: 0.0f);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(0.1f).Within(0.001f));
        }

        [Test]
        [Description("HarvestDurationMultiplier 가 음수이면 0.1 로 클램프되어야 한다.")]
        public void HarvestDurationMultiplier_음수_입력시_0_1로_클램프된다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe,
                harvestDurationMultiplier: -1.0f);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(0.1f).Within(0.001f));
        }

        [Test]
        [Description("HarvestDurationMultiplier 가 1.0 초과이면 1.0 으로 클램프되어야 한다.")]
        public void HarvestDurationMultiplier_1_0_초과_입력시_1_0으로_클램프된다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe,
                harvestDurationMultiplier: 2.0f);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(1.0f).Within(0.001f));
        }

        // ─ YieldMultiplier 보정 ───────────────────────────────

        [Test]
        [Description("YieldMultiplier 가 1.5 이면 그대로 유지되어야 한다.")]
        public void YieldMultiplier_1_이상_그대로_유지된다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe,
                yieldMultiplier: 1.5f);

            Assert.That(tool.YieldMultiplier, Is.EqualTo(1.5f).Within(0.001f));
        }

        [Test]
        [Description("YieldMultiplier 가 1.0 미만이면 1.0 으로 보정되어야 한다.")]
        public void YieldMultiplier_1_미만_입력시_1로_보정된다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe,
                yieldMultiplier: 0.5f);

            Assert.That(tool.YieldMultiplier, Is.EqualTo(1.0f).Within(0.001f));
        }

        [Test]
        [Description("YieldMultiplier 가 음수이면 1.0 으로 보정되어야 한다.")]
        public void YieldMultiplier_음수_입력시_1로_보정된다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe,
                yieldMultiplier: -2.0f);

            Assert.That(tool.YieldMultiplier, Is.EqualTo(1.0f).Within(0.001f));
        }

        // ─ Data / 인터페이스 ──────────────────────────────────

        [Test]
        [Description("Data 프로퍼티는 HarvestTool 자기 자신을 반환해야 한다.")]
        public void Data_프로퍼티가_자기_자신을_반환한다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe);

            Assert.That(tool.Data, Is.SameAs(tool));
        }

        [Test]
        [Description("HarvestTool 은 ITool 을 구현해야 한다.")]
        public void HarvestTool은_ITool을_구현한다()
        {
            var tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe);

            Assert.That(tool, Is.InstanceOf<ITool>());
        }

        [Test]
        [Description("ITool 로 캐스팅해도 배율이 올바르게 반환되어야 한다.")]
        public void ITool로_캐스팅시_배율이_올바르게_반환된다()
        {
            ITool tool = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe,
                harvestDurationMultiplier: 0.7f,
                yieldMultiplier: 1.3f);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(0.7f).Within(0.001f));
            Assert.That(tool.YieldMultiplier,           Is.EqualTo(1.3f).Within(0.001f));
        }
    }

    /// <summary>
    /// FarmTool 클래스 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 정상 생성 시 ItemId / ItemName / Grade 반영
    ///   [v] 기본값으로 생성 시 배율이 1.0
    ///   [v] HarvestDurationMultiplier 0.1 미만 입력 시 0.1로 클램프
    ///   [v] HarvestDurationMultiplier 1.0 초과 입력 시 1.0으로 클램프
    ///   [v] YieldMultiplier 1.0 미만 입력 시 1.0으로 보정
    ///   [v] Data 프로퍼티가 자기 자신을 반환
    ///   [v] ITool 인터페이스 구현 여부
    ///   [v] HarvestTool 과 FarmTool 이 동일 ITool 인터페이스를 구현함
    /// </summary>
    [TestFixture]
    public class FarmToolTests
    {
        // ─ 정상 생성 ──────────────────────────────────────────

        [Test]
        [Description("정상 생성 시 ItemId 가 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_ItemId가_올바르게_설정된다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal);

            Assert.That(tool.ItemId, Is.EqualTo("sickle_basic"));
        }

        [Test]
        [Description("정상 생성 시 ItemName 이 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_ItemName이_올바르게_설정된다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal);

            Assert.That(tool.ItemName, Is.EqualTo("기본 낫"));
        }

        [Test]
        [Description("정상 생성 시 Grade 가 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_Grade가_올바르게_설정된다()
        {
            var tool = new FarmTool("sickle_rare", "희귀 낫", ItemGrade.Rare);

            Assert.That(tool.Grade, Is.EqualTo(ItemGrade.Rare));
        }

        [Test]
        [Description("배율을 생략하면 HarvestDurationMultiplier 기본값은 1.0 이어야 한다.")]
        public void 기본값_생성시_HarvestDurationMultiplier가_1이다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(1.0f).Within(0.001f));
        }

        [Test]
        [Description("배율을 생략하면 YieldMultiplier 기본값은 1.0 이어야 한다.")]
        public void 기본값_생성시_YieldMultiplier가_1이다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal);

            Assert.That(tool.YieldMultiplier, Is.EqualTo(1.0f).Within(0.001f));
        }

        // ─ HarvestDurationMultiplier 클램프 ───────────────────

        [Test]
        [Description("HarvestDurationMultiplier 가 0.1 미만이면 0.1 로 클램프되어야 한다.")]
        public void HarvestDurationMultiplier_0_1_미만_입력시_0_1로_클램프된다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal,
                harvestDurationMultiplier: 0.0f);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(0.1f).Within(0.001f));
        }

        [Test]
        [Description("HarvestDurationMultiplier 가 1.0 초과이면 1.0 으로 클램프되어야 한다.")]
        public void HarvestDurationMultiplier_1_0_초과_입력시_1_0으로_클램프된다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal,
                harvestDurationMultiplier: 3.0f);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(1.0f).Within(0.001f));
        }

        [Test]
        [Description("HarvestDurationMultiplier 가 0.3 이면 그대로 유지되어야 한다.")]
        public void HarvestDurationMultiplier_정상_범위값_그대로_유지된다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal,
                harvestDurationMultiplier: 0.3f);

            Assert.That(tool.HarvestDurationMultiplier, Is.EqualTo(0.3f).Within(0.001f));
        }

        // ─ YieldMultiplier 보정 ───────────────────────────────

        [Test]
        [Description("YieldMultiplier 가 1.0 미만이면 1.0 으로 보정되어야 한다.")]
        public void YieldMultiplier_1_미만_입력시_1로_보정된다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal,
                yieldMultiplier: 0.8f);

            Assert.That(tool.YieldMultiplier, Is.EqualTo(1.0f).Within(0.001f));
        }

        [Test]
        [Description("YieldMultiplier 가 2.0 이면 그대로 유지되어야 한다.")]
        public void YieldMultiplier_2_0_이상_그대로_유지된다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal,
                yieldMultiplier: 2.0f);

            Assert.That(tool.YieldMultiplier, Is.EqualTo(2.0f).Within(0.001f));
        }

        // ─ Data / 인터페이스 ──────────────────────────────────

        [Test]
        [Description("Data 프로퍼티는 FarmTool 자기 자신을 반환해야 한다.")]
        public void Data_프로퍼티가_자기_자신을_반환한다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal);

            Assert.That(tool.Data, Is.SameAs(tool));
        }

        [Test]
        [Description("FarmTool 은 ITool 을 구현해야 한다.")]
        public void FarmTool은_ITool을_구현한다()
        {
            var tool = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal);

            Assert.That(tool, Is.InstanceOf<ITool>());
        }

        [Test]
        [Description("HarvestTool 과 FarmTool 은 모두 동일한 ITool 인터페이스를 구현해야 한다.")]
        public void HarvestTool과_FarmTool_모두_ITool을_구현한다()
        {
            ITool harvest = new HarvestTool("axe_iron", "철 도끼", ItemGrade.Normal, ToolType.Axe);
            ITool farm    = new FarmTool("sickle_basic", "기본 낫", ItemGrade.Normal);

            Assert.That(harvest, Is.InstanceOf<ITool>());
            Assert.That(farm,    Is.InstanceOf<ITool>());
        }
    }
}
