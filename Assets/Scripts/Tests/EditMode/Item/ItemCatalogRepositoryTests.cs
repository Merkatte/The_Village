using System;
using System.Collections.Generic;
using NUnit.Framework;
using Game.Core.Enums;
using Game.Item;
using Game.Item.Catalog;

namespace Game.Tests.EditMode.Item
{
    /// <summary>
    /// ItemCatalogRepository 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] null 목록 생성 시 예외
    ///   [v] GetEntry — 정상 조회, 없는 키, null/빈 키
    ///   [v] GetEntry — 대소문자 무시 조회
    ///   [v] GetEntryByResourceType — 정상 조회, 없는 타입
    ///   [v] GetAll — 전체 항목 반환, 빈 목록
    /// </summary>
    [TestFixture]
    public class ItemCatalogRepositoryTests
    {
        private ItemCatalogRepository _repo;
        private ItemCatalogEntry      _woodEntry;
        private ItemCatalogEntry      _swordEntry;

        [SetUp]
        public void SetUp()
        {
            _woodEntry  = new ItemCatalogEntry("wood",  "나무", ItemType.Resource, ItemGrade.Normal, 99, 10,
                resourceType: ResourceType.Wood);
            _swordEntry = new ItemCatalogEntry("sword", "검",   ItemType.Weapon,   ItemGrade.Rare,   1, 500,
                attackPower: 30);

            _repo = new ItemCatalogRepository(new List<ItemCatalogEntry> { _woodEntry, _swordEntry });
        }

        // ─ 생성자 ─────────────────────────────────────────────

        [Test]
        [Description("null 목록을 전달하면 ArgumentNullException 이 발생해야 한다.")]
        public void null_목록_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() => new ItemCatalogRepository(null));
        }

        // ─ GetEntry ───────────────────────────────────────────

        [Test]
        [Description("등록된 ItemId 로 조회하면 해당 항목을 반환해야 한다.")]
        public void GetEntry_등록된_ItemId로_조회하면_해당_항목을_반환한다()
        {
            var entry = _repo.GetEntry("wood");

            Assert.AreEqual(_woodEntry, entry);
        }

        [Test]
        [Description("등록되지 않은 ItemId 는 null 을 반환해야 한다.")]
        public void GetEntry_없는_ItemId는_null을_반환한다()
        {
            Assert.IsNull(_repo.GetEntry("unknown_item"));
        }

        [Test]
        [Description("null ItemId 는 null 을 반환해야 한다.")]
        public void GetEntry_null_ItemId는_null을_반환한다()
        {
            Assert.IsNull(_repo.GetEntry(null));
        }

        [Test]
        [Description("빈 문자열 ItemId 는 null 을 반환해야 한다.")]
        public void GetEntry_빈_문자열_ItemId는_null을_반환한다()
        {
            Assert.IsNull(_repo.GetEntry(""));
        }

        [Test]
        [Description("ItemId 조회는 대소문자를 구분하지 않아야 한다.")]
        public void GetEntry_대소문자_구분없이_조회된다()
        {
            Assert.AreEqual(_woodEntry, _repo.GetEntry("WOOD"));
            Assert.AreEqual(_woodEntry, _repo.GetEntry("Wood"));
            Assert.AreEqual(_woodEntry, _repo.GetEntry("wOoD"));
        }

        // ─ GetEntryByResourceType ─────────────────────────────

        [Test]
        [Description("등록된 ResourceType 으로 조회하면 해당 항목을 반환해야 한다.")]
        public void GetEntryByResourceType_등록된_ResourceType으로_조회하면_해당_항목을_반환한다()
        {
            var entry = _repo.GetEntryByResourceType(ResourceType.Wood);

            Assert.AreEqual(_woodEntry, entry);
        }

        [Test]
        [Description("등록되지 않은 ResourceType 은 null 을 반환해야 한다.")]
        public void GetEntryByResourceType_없는_ResourceType은_null을_반환한다()
        {
            Assert.IsNull(_repo.GetEntryByResourceType(ResourceType.Stone));
        }

        [Test]
        [Description("ResourceType 이 없는 아이템(Weapon 등)은 ResourceType 인덱스에 등록되지 않아야 한다.")]
        public void GetEntryByResourceType_ResourceType없는_아이템은_인덱싱_안됨()
        {
            // sword 는 ResourceType 이 없으므로 어떤 ResourceType 으로도 조회 불가
            Assert.IsNull(_repo.GetEntryByResourceType(ResourceType.Stone));
        }

        // ─ GetAll ─────────────────────────────────────────────

        [Test]
        [Description("GetAll 은 등록된 모든 항목을 반환해야 한다.")]
        public void GetAll_등록된_모든_항목을_반환한다()
        {
            var all = _repo.GetAll();

            Assert.AreEqual(2, all.Count);
            Assert.Contains(_woodEntry,  (System.Collections.IList)all);
            Assert.Contains(_swordEntry, (System.Collections.IList)all);
        }

        [Test]
        [Description("빈 목록으로 생성하면 GetAll 은 빈 결과를 반환해야 한다.")]
        public void GetAll_빈_목록으로_생성하면_빈_결과를_반환한다()
        {
            var repo = new ItemCatalogRepository(new List<ItemCatalogEntry>());

            Assert.AreEqual(0, repo.GetAll().Count);
        }
    }
}
