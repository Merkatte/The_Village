using NUnit.Framework;
using Game.Inventory;
using Game.Item;
using Game.Core.Interfaces;
using InventoryImpl = Game.Inventory.Inventory;

namespace Game.Tests.EditMode.Inventory
{
    /// <summary>
    /// Inventory 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 생성 시 SlotCount 및 모든 슬롯 null 검증
    ///   [v] TryAddItem — 첫 빈 슬롯에 순서대로 추가
    ///   [v] TryAddItem — 인벤토리 가득 찼을 때 false 반환
    ///   [v] TryAddItem — null 입력 시 예외
    ///   [v] GetSlot — 올바른 아이템 반환
    ///   [v] GetSlot — 범위 초과 시 예외
    ///   [v] RemoveAt — 슬롯이 null 로 비워짐
    ///   [v] RemoveAt — 범위 초과 시 예외
    ///   [v] MoveItem — 두 슬롯 교환
    ///   [v] MoveItem — 빈 슬롯과 교환 (사실상 이동)
    ///   [v] MoveItem — 범위 초과 시 예외
    ///   [v] 슬롯 수 0 이하로 생성 시 예외
    ///   [v] Slots 목록이 추가/제거 후 정확히 반영됨
    ///   [v] IStackable — 동일 아이템 기존 슬롯에 수량 합산
    ///   [v] IStackable — 슬롯 가득 찼을 때 새 슬롯에 배치
    ///   [v] IStackable — 남은 수량이 기존 슬롯 + 새 슬롯에 분산
    ///   [v] IStackable — 모든 슬롯 가득 찼을 때 false 반환
    ///   [v] MaxStackSize == 1 이면 스택 합산 없이 빈 슬롯에 배치
    /// </summary>
    [TestFixture]
    public class InventoryTests
    {
        // 테스트용 ItemData 구체 클래스 (ItemData 가 abstract 이므로 인라인 정의)
        private sealed class StubItem : ItemData
        {
            public StubItem(string id) : base(id, id, ItemGrade.Normal) { }
        }

        // 테스트용 스택 가능 아이템
        private sealed class StubStackableItem : ItemData, IStackable
        {
            public int Quantity { get; private set; }
            public bool IsFull => Quantity >= MaxStackSize;
            public override int MaxStackSize { get; }

            public StubStackableItem(string id, int quantity, int maxStackSize = 10)
                : base(id, id, ItemGrade.Normal)
            {
                MaxStackSize = maxStackSize;
                Quantity     = System.Math.Clamp(quantity, 1, maxStackSize);
            }

            public int AddQuantity(int amount)
            {
                int space = MaxStackSize - Quantity;
                int toAdd = System.Math.Min(amount, space);
                Quantity += toAdd;
                return amount - toAdd;
            }

            public void SetQuantity(int quantity)
            {
                Quantity = System.Math.Clamp(quantity, 1, MaxStackSize);
            }
        }

        private InventoryImpl _inventory;

        [SetUp]
        public void SetUp()
        {
            _inventory = new InventoryImpl(slotCount: 5);
        }

        // ─ 생성 ───────────────────────────────────────────────

        [Test]
        [Description("생성 직후 SlotCount 가 지정한 값과 일치해야 한다.")]
        public void 생성_직후_SlotCount가_지정값과_일치()
        {
            Assert.AreEqual(5, _inventory.SlotCount);
        }

        [Test]
        [Description("생성 직후 모든 슬롯이 null 이어야 한다.")]
        public void 생성_직후_모든_슬롯이_null()
        {
            for (int i = 0; i < _inventory.SlotCount; i++)
                Assert.IsNull(_inventory.GetSlot(i));
        }

        [Test]
        [Description("슬롯 수를 0 이하로 지정하면 예외가 발생해야 한다.")]
        public void 슬롯수_0이하로_생성시_예외()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new InventoryImpl(0));
        }

        // ─ TryAddItem ─────────────────────────────────────────

        [Test]
        [Description("TryAddItem 은 첫 번째 빈 슬롯(인덱스 0)에 아이템을 추가해야 한다.")]
        public void TryAddItem_첫번째_빈슬롯에_추가()
        {
            var item = new StubItem("item_a");
            _inventory.TryAddItem(item);

            Assert.AreSame(item, _inventory.GetSlot(0));
        }

        [Test]
        [Description("두 번째 TryAddItem 은 인덱스 1에 추가되어야 한다.")]
        public void TryAddItem_순서대로_다음_빈슬롯에_추가()
        {
            _inventory.TryAddItem(new StubItem("item_a"));
            var second = new StubItem("item_b");
            _inventory.TryAddItem(second);

            Assert.AreSame(second, _inventory.GetSlot(1));
        }

        [Test]
        [Description("TryAddItem 성공 시 true 를 반환해야 한다.")]
        public void TryAddItem_성공시_true_반환()
        {
            var result = _inventory.TryAddItem(new StubItem("item_a"));
            Assert.IsTrue(result);
        }

        [Test]
        [Description("인벤토리가 가득 찼을 때 TryAddItem 은 false 를 반환해야 한다.")]
        public void TryAddItem_가득찼을때_false_반환()
        {
            for (int i = 0; i < _inventory.SlotCount; i++)
                _inventory.TryAddItem(new StubItem($"item_{i}"));

            var result = _inventory.TryAddItem(new StubItem("overflow"));
            Assert.IsFalse(result);
        }

        [Test]
        [Description("TryAddItem 에 null 을 전달하면 예외가 발생해야 한다.")]
        public void TryAddItem_null전달시_예외()
        {
            Assert.Throws<System.ArgumentNullException>(() => _inventory.TryAddItem(null));
        }

        // ─ GetSlot ────────────────────────────────────────────

        [Test]
        [Description("GetSlot 은 해당 슬롯의 아이템을 정확히 반환해야 한다.")]
        public void GetSlot_올바른_아이템_반환()
        {
            var item = new StubItem("item_a");
            _inventory.TryAddItem(item);

            Assert.AreSame(item, _inventory.GetSlot(0));
        }

        [Test]
        [Description("GetSlot 에 범위 초과 인덱스를 전달하면 예외가 발생해야 한다.")]
        public void GetSlot_범위초과_인덱스시_예외()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _inventory.GetSlot(999));
        }

        // ─ RemoveAt ───────────────────────────────────────────

        [Test]
        [Description("RemoveAt 호출 후 해당 슬롯이 null 이 되어야 한다.")]
        public void RemoveAt_호출후_슬롯이_null()
        {
            _inventory.TryAddItem(new StubItem("item_a"));
            _inventory.RemoveAt(0);

            Assert.IsNull(_inventory.GetSlot(0));
        }

        [Test]
        [Description("RemoveAt 에 범위 초과 인덱스를 전달하면 예외가 발생해야 한다.")]
        public void RemoveAt_범위초과_인덱스시_예외()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _inventory.RemoveAt(-1));
        }

        // ─ MoveItem ───────────────────────────────────────────

        [Test]
        [Description("MoveItem 은 두 슬롯의 아이템을 교환해야 한다.")]
        public void MoveItem_두슬롯_교환()
        {
            var a = new StubItem("item_a");
            var b = new StubItem("item_b");
            _inventory.TryAddItem(a);
            _inventory.TryAddItem(b);

            _inventory.MoveItem(0, 1);

            Assert.AreSame(b, _inventory.GetSlot(0));
            Assert.AreSame(a, _inventory.GetSlot(1));
        }

        [Test]
        [Description("MoveItem 으로 아이템이 있는 슬롯과 빈 슬롯을 교환하면 아이템이 이동해야 한다.")]
        public void MoveItem_빈슬롯과_교환시_아이템_이동()
        {
            var item = new StubItem("item_a");
            _inventory.TryAddItem(item);

            _inventory.MoveItem(0, 4);

            Assert.IsNull(_inventory.GetSlot(0));
            Assert.AreSame(item, _inventory.GetSlot(4));
        }

        [Test]
        [Description("MoveItem 에 범위 초과 인덱스를 전달하면 예외가 발생해야 한다.")]
        public void MoveItem_범위초과_인덱스시_예외()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _inventory.MoveItem(0, 999));
        }

        // ─ 스택 합산 ──────────────────────────────────────────

        [Test]
        [Description("동일 ItemId 의 스택 아이템이 추가되면 기존 슬롯 수량에 합산되어야 한다.")]
        public void TryAddItem_스택아이템_기존슬롯에_수량합산()
        {
            var first = new StubStackableItem("wood", quantity: 5, maxStackSize: 10);
            _inventory.TryAddItem(first);

            var second = new StubStackableItem("wood", quantity: 3, maxStackSize: 10);
            _inventory.TryAddItem(second);

            // first 슬롯 수량이 8 이 되어야 한다
            Assert.AreEqual(8, ((IStackable)_inventory.GetSlot(0)).Quantity);
            // 새 슬롯은 비어 있어야 한다
            Assert.IsNull(_inventory.GetSlot(1));
        }

        [Test]
        [Description("기존 슬롯이 가득 찼으면 새 슬롯에 배치되어야 한다.")]
        public void TryAddItem_스택아이템_기존슬롯_가득찼을때_새슬롯_배치()
        {
            var first = new StubStackableItem("wood", quantity: 10, maxStackSize: 10); // 이미 가득 참
            _inventory.TryAddItem(first);

            var second = new StubStackableItem("wood", quantity: 3, maxStackSize: 10);
            _inventory.TryAddItem(second);

            // 슬롯 0 은 가득 찬 채로 유지, 슬롯 1 에 3 수량
            Assert.AreEqual(10, ((IStackable)_inventory.GetSlot(0)).Quantity);
            Assert.AreEqual(3,  ((IStackable)_inventory.GetSlot(1)).Quantity);
        }

        [Test]
        [Description("추가 수량이 기존 슬롯 여유분을 초과하면 나머지가 새 슬롯에 분산되어야 한다.")]
        public void TryAddItem_스택아이템_초과수량_새슬롯에_분산()
        {
            var first = new StubStackableItem("wood", quantity: 8, maxStackSize: 10); // 여유 2
            _inventory.TryAddItem(first);

            var second = new StubStackableItem("wood", quantity: 5, maxStackSize: 10); // 2 합산 + 3 남음
            _inventory.TryAddItem(second);

            Assert.AreEqual(10, ((IStackable)_inventory.GetSlot(0)).Quantity);
            Assert.AreEqual(3,  ((IStackable)_inventory.GetSlot(1)).Quantity);
        }

        [Test]
        [Description("스택 아이템이라도 모든 슬롯이 가득 찼으면 false 를 반환해야 한다.")]
        public void TryAddItem_스택아이템_모든슬롯_가득찼을때_false()
        {
            // 슬롯 5개 모두 가득 찬 wood 로 채움
            for (int i = 0; i < _inventory.SlotCount; i++)
                _inventory.TryAddItem(new StubStackableItem("wood", quantity: 10, maxStackSize: 10));

            var result = _inventory.TryAddItem(new StubStackableItem("wood", quantity: 1, maxStackSize: 10));
            Assert.IsFalse(result);
        }

        [Test]
        [Description("MaxStackSize == 1 인 아이템은 스택 합산 없이 빈 슬롯에 배치되어야 한다.")]
        public void TryAddItem_MaxStackSize1_스택합산_없이_새슬롯_배치()
        {
            // maxStackSize 1 → IStackable 이지만 Inventory 는 스택 경로를 타지 않음
            var first  = new StubStackableItem("sword", quantity: 1, maxStackSize: 1);
            var second = new StubStackableItem("sword", quantity: 1, maxStackSize: 1);
            _inventory.TryAddItem(first);
            _inventory.TryAddItem(second);

            Assert.AreSame(first,  _inventory.GetSlot(0));
            Assert.AreSame(second, _inventory.GetSlot(1));
        }

        // ─ Slots 목록 ─────────────────────────────────────────

        [Test]
        [Description("Slots 목록은 TryAddItem 후 즉시 추가된 아이템을 반영해야 한다.")]
        public void Slots_목록이_추가후_반영됨()
        {
            var item = new StubItem("item_a");
            _inventory.TryAddItem(item);

            Assert.AreSame(item, _inventory.Slots[0]);
        }

        [Test]
        [Description("Slots 목록은 RemoveAt 후 즉시 null 을 반영해야 한다.")]
        public void Slots_목록이_제거후_null_반영()
        {
            _inventory.TryAddItem(new StubItem("item_a"));
            _inventory.RemoveAt(0);

            Assert.IsNull(_inventory.Slots[0]);
        }
    }
}
