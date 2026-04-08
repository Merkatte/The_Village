using System;
using System.Collections.Generic;
using NUnit.Framework;
using Game.Core.Enums;
using Game.Core.Interfaces;
using Game.Inventory;
using Game.Item;
using Game.Item.Catalog;
using Game.Item.Resource;
using Game.UI.Inventory;

namespace Game.Tests.EditMode.UI
{
    /// <summary>
    /// InventoryPresenter 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 생성자 null 인자 예외 검증
    ///   [v] Bind — null View 예외, 즉시 갱신
    ///   [v] Unbind — 이후 View 갱신 미호출
    ///   [v] OnInventoryChanged — View 자동 갱신
    ///   [v] MoveItem — 인벤토리 위임 검증
    ///   [v] Dispose — 이벤트 구독 해제
    ///   [v] Refresh — 빈 슬롯 Empty, 아이템 슬롯 이름·수량 전달
    /// </summary>
    [TestFixture]
    public class InventoryPresenterTests
    {
        // ─ Fake ───────────────────────────────────────────────

        private sealed class FakeInventory : IInventory
        {
            public event Action OnInventoryChanged;

            private readonly ItemData[] _slots;

            public int SlotCount => _slots.Length;
            public IReadOnlyList<ItemData> Slots => _slots;

            public int  MoveFromIndex { get; private set; } = -1;
            public int  MoveToIndex   { get; private set; } = -1;

            public FakeInventory(int slotCount = 3)
            {
                _slots = new ItemData[slotCount];
            }

            public bool TryAddItem(ItemData item)  { return false; }
            public ItemData GetSlot(int index)     => _slots[index];
            public void RemoveAt(int index)        { _slots[index] = null; }
            public void MoveItem(int from, int to) { MoveFromIndex = from; MoveToIndex = to; }

            public void SetSlot(int index, ItemData item) => _slots[index] = item;

            public void FireInventoryChanged() => OnInventoryChanged?.Invoke();
        }

        private sealed class FakeSpriteRepository : ISpriteRepository
        {
            // EditMode 에서 Sprite 생성이 불가하므로 null 반환
            public UnityEngine.Sprite GetSprite(string itemId) => null;
        }

        private sealed class FakeInventoryView : IInventoryView
        {
            public int            RefreshCallCount { get; private set; }
            public SlotViewModel[] LastSlots       { get; private set; }

            public void Refresh(SlotViewModel[] slots)
            {
                RefreshCallCount++;
                LastSlots = slots;
            }
        }

        // ─ SetUp ──────────────────────────────────────────────

        private FakeInventory       _inventory;
        private FakeSpriteRepository _sprites;
        private FakeInventoryView   _view;

        [SetUp]
        public void SetUp()
        {
            _inventory = new FakeInventory(3);
            _sprites   = new FakeSpriteRepository();
            _view      = new FakeInventoryView();
        }

        // ─ 생성자 ─────────────────────────────────────────────

        [Test]
        [Description("inventory 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_inventory_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new InventoryPresenter(null, _sprites));
        }

        [Test]
        [Description("spriteRepository 가 null 이면 ArgumentNullException 이 발생해야 한다.")]
        public void null_spriteRepository_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new InventoryPresenter(_inventory, null));
        }

        // ─ Bind ───────────────────────────────────────────────

        [Test]
        [Description("Bind 에 null 을 전달하면 ArgumentNullException 이 발생해야 한다.")]
        public void Bind_null_View_전달시_ArgumentNullException_발생()
        {
            var presenter = new InventoryPresenter(_inventory, _sprites);

            Assert.Throws<ArgumentNullException>(() => presenter.Bind(null));
        }

        [Test]
        [Description("Bind 호출 즉시 View.Refresh 가 한 번 호출되어야 한다.")]
        public void Bind_호출시_View가_즉시_갱신된다()
        {
            var presenter = new InventoryPresenter(_inventory, _sprites);

            presenter.Bind(_view);

            Assert.AreEqual(1, _view.RefreshCallCount);
        }

        // ─ OnInventoryChanged ─────────────────────────────────

        [Test]
        [Description("인벤토리 변경 이벤트 발행 시 View.Refresh 가 추가 호출되어야 한다.")]
        public void OnInventoryChanged_발행시_View가_갱신된다()
        {
            var presenter = new InventoryPresenter(_inventory, _sprites);
            presenter.Bind(_view);

            _inventory.FireInventoryChanged();

            Assert.AreEqual(2, _view.RefreshCallCount);
        }

        // ─ Unbind ─────────────────────────────────────────────

        [Test]
        [Description("Unbind 후 인벤토리 변경 이벤트가 발행되어도 View 가 갱신되지 않아야 한다.")]
        public void Unbind_후_View_갱신이_호출되지_않는다()
        {
            var presenter = new InventoryPresenter(_inventory, _sprites);
            presenter.Bind(_view);
            presenter.Unbind();

            _inventory.FireInventoryChanged();

            Assert.AreEqual(1, _view.RefreshCallCount); // Bind 시 1회만
        }

        // ─ MoveItem ───────────────────────────────────────────

        [Test]
        [Description("MoveItem 호출 시 IInventory.MoveItem 에 인덱스가 전달되어야 한다.")]
        public void MoveItem_호출시_인벤토리의_MoveItem이_위임된다()
        {
            var presenter = new InventoryPresenter(_inventory, _sprites);

            presenter.MoveItem(0, 2);

            Assert.AreEqual(0, _inventory.MoveFromIndex);
            Assert.AreEqual(2, _inventory.MoveToIndex);
        }

        // ─ Dispose ────────────────────────────────────────────

        [Test]
        [Description("Dispose 후 인벤토리 변경 이벤트가 발행되어도 View 가 갱신되지 않아야 한다.")]
        public void Dispose_후_View_갱신이_호출되지_않는다()
        {
            var presenter = new InventoryPresenter(_inventory, _sprites);
            presenter.Bind(_view);
            presenter.Dispose();

            _inventory.FireInventoryChanged();

            Assert.AreEqual(1, _view.RefreshCallCount); // Bind 시 1회만
        }

        // ─ Refresh — ViewModel 내용 ──────────────────────────

        [Test]
        [Description("빈 슬롯은 SlotViewModel.Empty 로 전달되어야 한다.")]
        public void 빈슬롯은_SlotViewModel_Empty로_전달된다()
        {
            var presenter = new InventoryPresenter(_inventory, _sprites);
            presenter.Bind(_view);

            Assert.IsTrue(_view.LastSlots[0].IsEmpty);
        }

        [Test]
        [Description("아이템이 있는 슬롯의 ViewModel 에 ItemName 이 올바르게 설정되어야 한다.")]
        public void 아이템_슬롯의_ItemName이_올바르게_전달된다()
        {
            var entry = new ItemCatalogEntry("wood", "나무", ItemType.Resource, ItemGrade.Normal, 99, 10,
                resourceType: ResourceType.Wood);
            _inventory.SetSlot(0, new ResourceItem(entry, 1));

            var presenter = new InventoryPresenter(_inventory, _sprites);
            presenter.Bind(_view);

            Assert.AreEqual("나무", _view.LastSlots[0].ItemName);
        }

        [Test]
        [Description("수량이 2 이상인 스택 아이템은 QuantityText 에 수량이 담겨야 한다.")]
        public void 스택_아이템은_QuantityText에_수량이_담긴다()
        {
            var entry = new ItemCatalogEntry("wood", "나무", ItemType.Resource, ItemGrade.Normal, 99, 10,
                resourceType: ResourceType.Wood);
            _inventory.SetSlot(0, new ResourceItem(entry, 5));

            var presenter = new InventoryPresenter(_inventory, _sprites);
            presenter.Bind(_view);

            Assert.AreEqual("5", _view.LastSlots[0].QuantityText);
        }

        [Test]
        [Description("수량이 1인 스택 아이템은 QuantityText 가 빈 문자열이어야 한다.")]
        public void 수량이_1인_스택_아이템은_QuantityText가_빈_문자열이다()
        {
            var entry = new ItemCatalogEntry("wood", "나무", ItemType.Resource, ItemGrade.Normal, 99, 10,
                resourceType: ResourceType.Wood);
            _inventory.SetSlot(0, new ResourceItem(entry, 1));

            var presenter = new InventoryPresenter(_inventory, _sprites);
            presenter.Bind(_view);

            Assert.AreEqual(string.Empty, _view.LastSlots[0].QuantityText);
        }

        [Test]
        [Description("Refresh 시 View 에 SlotCount 크기의 배열이 전달되어야 한다.")]
        public void Refresh시_SlotCount_크기의_배열이_전달된다()
        {
            var presenter = new InventoryPresenter(_inventory, _sprites);
            presenter.Bind(_view);

            Assert.AreEqual(_inventory.SlotCount, _view.LastSlots.Length);
        }
    }
}
