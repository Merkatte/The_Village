namespace Game.Item.Equipment
{
    /// <summary>
    /// 방어구 아이템 데이터입니다.
    ///
    /// - 방어력(Defense)을 보유합니다.
    /// - 전투 시스템에서 플레이어 피해 감소 계산에 사용됩니다.
    /// - 등급(Grade)이 높을수록 Defense가 높습니다.
    /// </summary>
    public sealed class Armor : ItemData, IEquipment
    {
        /// <summary>방어력</summary>
        public int Defense { get; }

        /// <inheritdoc/>
        public ItemData Data => this;
 
        /// <inheritdoc/>
        public EquipmentSlot Slot => EquipmentSlot.Armor;

        /// <summary>
        /// Armor 생성자.
        /// </summary>
        /// <param name="itemId">고유 식별자 (예: "armor_leather")</param>
        /// <param name="itemName">표시 이름 (예: "가죽 갑옷")</param>
        /// <param name="grade">아이템 등급</param>
        /// <param name="defense">방어력 (0 이상)</param>
        public Armor(string itemId, string itemName, ItemGrade grade, int defense)
            : base(itemId, itemName, grade)
        {
            Defense = defense < 0 ? 0 : defense;
        }
    }
}
