using NUnit.Framework;
using Game.Item;
using Game.Item.Equipment;

namespace Game.Tests.EditMode.Item
{
    /// <summary>
    /// Weapon 클래스 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 정상 생성 시 ItemId / ItemName / Grade 반영
    ///   [v] AttackPower 정상값 설정
    ///   [v] AttackPower 0 입력 시 1로 보정
    ///   [v] AttackPower 음수 입력 시 1로 보정
    ///   [v] Slot == EquipmentSlot.Weapon
    ///   [v] Data 프로퍼티가 자기 자신을 반환
    ///   [v] IEquipment 인터페이스 구현 여부
    /// </summary>
    [TestFixture]
    public class WeaponTests
    {
        // ─ 정상 생성 ──────────────────────────────────────────

        [Test]
        [Description("정상 생성 시 ItemId 가 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_ItemId가_올바르게_설정된다()
        {
            var weapon = new Weapon("sword_basic", "기본 검", ItemGrade.Normal, 10);

            Assert.That(weapon.ItemId, Is.EqualTo("sword_basic"));
        }

        [Test]
        [Description("정상 생성 시 ItemName 이 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_ItemName이_올바르게_설정된다()
        {
            var weapon = new Weapon("sword_basic", "기본 검", ItemGrade.Normal, 10);

            Assert.That(weapon.ItemName, Is.EqualTo("기본 검"));
        }

        [Test]
        [Description("정상 생성 시 Grade 가 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_Grade가_올바르게_설정된다()
        {
            var weapon = new Weapon("sword_rare", "희귀 검", ItemGrade.Rare, 25);

            Assert.That(weapon.Grade, Is.EqualTo(ItemGrade.Rare));
        }

        // ─ AttackPower 보정 ───────────────────────────────────

        [Test]
        [Description("AttackPower 를 10 으로 생성하면 AttackPower 가 10 이어야 한다.")]
        public void 정상_AttackPower_그대로_설정된다()
        {
            var weapon = new Weapon("sword_basic", "기본 검", ItemGrade.Normal, 10);

            Assert.That(weapon.AttackPower, Is.EqualTo(10));
        }

        [Test]
        [Description("AttackPower 가 0 이면 1 로 보정되어야 한다.")]
        public void AttackPower_0_입력시_1로_보정된다()
        {
            var weapon = new Weapon("sword_basic", "기본 검", ItemGrade.Normal, 0);

            Assert.That(weapon.AttackPower, Is.EqualTo(1));
        }

        [Test]
        [Description("AttackPower 가 음수이면 1 로 보정되어야 한다.")]
        public void AttackPower_음수_입력시_1로_보정된다()
        {
            var weapon = new Weapon("sword_basic", "기본 검", ItemGrade.Normal, -10);

            Assert.That(weapon.AttackPower, Is.EqualTo(1));
        }

        // ─ Slot / Data / 인터페이스 ───────────────────────────

        [Test]
        [Description("Weapon 의 Slot 은 EquipmentSlot.Weapon 이어야 한다.")]
        public void Slot이_Weapon이다()
        {
            var weapon = new Weapon("sword_basic", "기본 검", ItemGrade.Normal, 10);

            Assert.That(weapon.Slot, Is.EqualTo(EquipmentSlot.Weapon));
        }

        [Test]
        [Description("Data 프로퍼티는 Weapon 자기 자신을 반환해야 한다.")]
        public void Data_프로퍼티가_자기_자신을_반환한다()
        {
            var weapon = new Weapon("sword_basic", "기본 검", ItemGrade.Normal, 10);

            Assert.That(weapon.Data, Is.SameAs(weapon));
        }

        [Test]
        [Description("Weapon 은 IEquipment 를 구현해야 한다.")]
        public void Weapon은_IEquipment를_구현한다()
        {
            var weapon = new Weapon("sword_basic", "기본 검", ItemGrade.Normal, 10);

            Assert.That(weapon, Is.InstanceOf<IEquipment>());
        }

        [Test]
        [Description("IEquipment 로 캐스팅해도 Slot 이 Weapon 이어야 한다.")]
        public void IEquipment로_캐스팅시_Slot이_Weapon이다()
        {
            IEquipment equipment = new Weapon("sword_basic", "기본 검", ItemGrade.Normal, 10);

            Assert.That(equipment.Slot, Is.EqualTo(EquipmentSlot.Weapon));
        }
    }

    /// <summary>
    /// Armor 클래스 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 정상 생성 시 ItemId / ItemName / Grade 반영
    ///   [v] Defense 정상값 설정
    ///   [v] Defense 0 입력 시 그대로 유지 (0은 유효값)
    ///   [v] Defense 음수 입력 시 0 으로 보정
    ///   [v] Slot == EquipmentSlot.Armor
    ///   [v] Data 프로퍼티가 자기 자신을 반환
    ///   [v] IEquipment 인터페이스 구현 여부
    /// </summary>
    [TestFixture]
    public class ArmorTests
    {
        // ─ 정상 생성 ──────────────────────────────────────────

        [Test]
        [Description("정상 생성 시 ItemId 가 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_ItemId가_올바르게_설정된다()
        {
            var armor = new Armor("armor_leather", "가죽 갑옷", ItemGrade.Normal, 5);

            Assert.That(armor.ItemId, Is.EqualTo("armor_leather"));
        }

        [Test]
        [Description("정상 생성 시 ItemName 이 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_ItemName이_올바르게_설정된다()
        {
            var armor = new Armor("armor_leather", "가죽 갑옷", ItemGrade.Normal, 5);

            Assert.That(armor.ItemName, Is.EqualTo("가죽 갑옷"));
        }

        [Test]
        [Description("정상 생성 시 Grade 가 생성자 인자와 일치해야 한다.")]
        public void 정상_생성시_Grade가_올바르게_설정된다()
        {
            var armor = new Armor("armor_epic", "영웅 갑옷", ItemGrade.Epic, 50);

            Assert.That(armor.Grade, Is.EqualTo(ItemGrade.Epic));
        }

        // ─ Defense 보정 ───────────────────────────────────────

        [Test]
        [Description("Defense 를 5 로 생성하면 Defense 가 5 이어야 한다.")]
        public void 정상_Defense_그대로_설정된다()
        {
            var armor = new Armor("armor_leather", "가죽 갑옷", ItemGrade.Normal, 5);

            Assert.That(armor.Defense, Is.EqualTo(5));
        }

        [Test]
        [Description("Defense 가 0 이면 0 으로 유지되어야 한다. (0 은 유효값)")]
        public void Defense_0_입력시_0으로_유지된다()
        {
            var armor = new Armor("armor_none", "맨몸", ItemGrade.Normal, 0);

            Assert.That(armor.Defense, Is.EqualTo(0));
        }

        [Test]
        [Description("Defense 가 음수이면 0 으로 보정되어야 한다.")]
        public void Defense_음수_입력시_0으로_보정된다()
        {
            var armor = new Armor("armor_leather", "가죽 갑옷", ItemGrade.Normal, -5);

            Assert.That(armor.Defense, Is.EqualTo(0));
        }

        // ─ Slot / Data / 인터페이스 ───────────────────────────

        [Test]
        [Description("Armor 의 Slot 은 EquipmentSlot.Armor 이어야 한다.")]
        public void Slot이_Armor이다()
        {
            var armor = new Armor("armor_leather", "가죽 갑옷", ItemGrade.Normal, 5);

            Assert.That(armor.Slot, Is.EqualTo(EquipmentSlot.Armor));
        }

        [Test]
        [Description("Data 프로퍼티는 Armor 자기 자신을 반환해야 한다.")]
        public void Data_프로퍼티가_자기_자신을_반환한다()
        {
            var armor = new Armor("armor_leather", "가죽 갑옷", ItemGrade.Normal, 5);

            Assert.That(armor.Data, Is.SameAs(armor));
        }

        [Test]
        [Description("Armor 는 IEquipment 를 구현해야 한다.")]
        public void Armor는_IEquipment를_구현한다()
        {
            var armor = new Armor("armor_leather", "가죽 갑옷", ItemGrade.Normal, 5);

            Assert.That(armor, Is.InstanceOf<IEquipment>());
        }

        [Test]
        [Description("IEquipment 로 캐스팅해도 Slot 이 Armor 이어야 한다.")]
        public void IEquipment로_캐스팅시_Slot이_Armor이다()
        {
            IEquipment equipment = new Armor("armor_leather", "가죽 갑옷", ItemGrade.Normal, 5);

            Assert.That(equipment.Slot, Is.EqualTo(EquipmentSlot.Armor));
        }
    }
}
