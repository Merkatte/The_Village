using System;
using NUnit.Framework;
using Game.Core.Enums;
using Game.Item;
using Game.Item.Catalog;
using Game.Item.Equipment;
using Game.Item.Resource;
using Game.Item.Tool;

namespace Game.Tests.EditMode.Item
{
    /// <summary>
    /// ItemFactory 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] null entry 예외 검증
    ///   [v] Resource 타입 생성 및 수량 설정
    ///   [v] Weapon 타입 생성 및 공격력 설정
    ///   [v] Armor 타입 생성 및 방어력 설정
    ///   [v] HarvestTool 타입 생성 및 ToolType 누락 예외
    ///   [v] FarmTool 타입 생성
    ///   [v] 공통 필드(ItemId, ItemName) 전달 검증
    /// </summary>
    [TestFixture]
    public class ItemFactoryTests
    {
        // ─ 공통 ──────────────────────────────────────────────

        [Test]
        [Description("entry 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void Create_null_entry는_ArgumentNullException을_발생시킨다()
        {
            Assert.Throws<ArgumentNullException>(() => ItemFactory.Create(null));
        }

        [Test]
        [Description("생성된 ItemData 의 ItemId 와 ItemName 이 entry 와 일치해야 한다.")]
        public void Create_결과의_ItemId와_ItemName이_entry와_일치한다()
        {
            var entry = MakeResourceEntry("wood", "나무");

            var item = ItemFactory.Create(entry);

            Assert.AreEqual("wood", item.ItemId);
            Assert.AreEqual("나무",  item.ItemName);
        }

        // ─ Resource ───────────────────────────────────────────

        [Test]
        [Description("Resource 타입 entry 는 ResourceItem 을 반환해야 한다.")]
        public void Create_Resource_타입은_ResourceItem을_반환한다()
        {
            var entry = MakeResourceEntry("wood", "나무");

            var item = ItemFactory.Create(entry);

            Assert.IsInstanceOf<ResourceItem>(item);
        }

        [Test]
        [Description("quantity 를 지정하지 않으면 기본 수량이 1 이어야 한다.")]
        public void Create_Resource_기본수량은_1이다()
        {
            var entry = MakeResourceEntry("wood", "나무");

            var item = ItemFactory.Create(entry) as ResourceItem;

            Assert.AreEqual(1, item.Quantity);
        }

        [Test]
        [Description("quantity 를 지정하면 ResourceItem 의 수량에 반영되어야 한다.")]
        public void Create_Resource_수량을_지정하면_반영된다()
        {
            var entry = MakeResourceEntry("wood", "나무");

            var item = ItemFactory.Create(entry, 5) as ResourceItem;

            Assert.AreEqual(5, item.Quantity);
        }

        // ─ Weapon ─────────────────────────────────────────────

        [Test]
        [Description("Weapon 타입 entry 는 Weapon 을 반환해야 한다.")]
        public void Create_Weapon_타입은_Weapon을_반환한다()
        {
            var entry = new ItemCatalogEntry("sword", "검", ItemType.Weapon, ItemGrade.Normal, 1, 500,
                attackPower: 30);

            var item = ItemFactory.Create(entry);

            Assert.IsInstanceOf<Weapon>(item);
        }

        [Test]
        [Description("Weapon 의 공격력이 entry.AttackPower 와 일치해야 한다.")]
        public void Create_Weapon_공격력이_올바르게_설정된다()
        {
            var entry = new ItemCatalogEntry("sword", "검", ItemType.Weapon, ItemGrade.Normal, 1, 500,
                attackPower: 30);

            var item = ItemFactory.Create(entry) as Weapon;

            Assert.AreEqual(30, item.AttackPower);
        }

        // ─ Armor ──────────────────────────────────────────────

        [Test]
        [Description("Armor 타입 entry 는 Armor 를 반환해야 한다.")]
        public void Create_Armor_타입은_Armor를_반환한다()
        {
            var entry = new ItemCatalogEntry("shield", "방패", ItemType.Armor, ItemGrade.Normal, 1, 200,
                defensePower: 15);

            var item = ItemFactory.Create(entry);

            Assert.IsInstanceOf<Armor>(item);
        }

        [Test]
        [Description("Armor 의 방어력이 entry.DefensePower 와 일치해야 한다.")]
        public void Create_Armor_방어력이_올바르게_설정된다()
        {
            var entry = new ItemCatalogEntry("shield", "방패", ItemType.Armor, ItemGrade.Normal, 1, 200,
                defensePower: 15);

            var item = ItemFactory.Create(entry) as Armor;

            Assert.AreEqual(15, item.Defense);
        }

        // ─ HarvestTool ────────────────────────────────────────

        [Test]
        [Description("HarvestTool 타입 entry 는 HarvestTool 을 반환해야 한다.")]
        public void Create_HarvestTool_타입은_HarvestTool을_반환한다()
        {
            var entry = new ItemCatalogEntry("axe", "도끼", ItemType.HarvestTool, ItemGrade.Normal, 1, 100,
                toolType: ToolType.Axe);

            var item = ItemFactory.Create(entry);

            Assert.IsInstanceOf<HarvestTool>(item);
        }

        [Test]
        [Description("HarvestTool entry 에 ToolType 이 없으면 InvalidOperationException 이 발생해야 한다.")]
        public void Create_HarvestTool_ToolType_없으면_InvalidOperationException_발생한다()
        {
            var entry = new ItemCatalogEntry("axe", "도끼", ItemType.HarvestTool, ItemGrade.Normal, 1, 100);
            // toolType = null

            Assert.Throws<InvalidOperationException>(() => ItemFactory.Create(entry));
        }

        // ─ FarmTool ───────────────────────────────────────────

        [Test]
        [Description("FarmTool 타입 entry 는 FarmTool 을 반환해야 한다.")]
        public void Create_FarmTool_타입은_FarmTool을_반환한다()
        {
            var entry = new ItemCatalogEntry("scythe", "낫", ItemType.FarmTool, ItemGrade.Normal, 1, 80);

            var item = ItemFactory.Create(entry);

            Assert.IsInstanceOf<FarmTool>(item);
        }

        // ─ 헬퍼 ──────────────────────────────────────────────

        private static ItemCatalogEntry MakeResourceEntry(string itemId, string itemName)
            => new ItemCatalogEntry(itemId, itemName, ItemType.Resource, ItemGrade.Normal, 99, 10,
                resourceType: ResourceType.Wood);
    }
}
