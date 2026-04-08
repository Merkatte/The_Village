namespace Game.Item.Equipment
{
    /// <summary>
    /// 장착 가능한 장비 인터페이스입니다.
    /// 무기와 방어구가 이 인터페이스를 구현합니다.
    ///
    /// 외부(전투 시스템 등)에서는 구체 클래스가 아닌
    /// 이 인터페이스만 바라보도록 합니다. (DIP)
    /// </summary>
    public interface IEquipment
    {
        /// <summary>장비 기반 데이터</summary>
        ItemData Data { get; }

        /// <summary>장비 슬롯 종류 (무기 / 방어구)</summary>
        EquipmentSlot Slot { get; }
    }

    /// <summary>장비 슬롯 종류입니다.</summary>
    public enum EquipmentSlot
    {
        /// <summary>무기 슬롯</summary>
        Weapon,
        /// <summary>방어구 슬롯</summary>
        Armor
    }
}
