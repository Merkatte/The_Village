namespace Game.Item.Equipment
{
    /// <summary>
    /// 무기 아이템 데이터입니다.
    ///
    /// - 공격력(AttackPower)을 보유합니다.
    /// - 전투 시스템에서 플레이어 데미지 계산에 사용됩니다.
    /// - 등급(Grade)이 높을수록 AttackPower가 높습니다.
    /// </summary>
    public sealed class Weapon : ItemData, IEquipment
    {
        /// <summary>공격력</summary>
        public int AttackPower { get; }

        /// <inheritdoc/>
        public ItemData Data => this;

        /// <inheritdoc/>
        public EquipmentSlot Slot => EquipmentSlot.Weapon;

        /// <summary>
        /// Weapon 생성자.
        /// </summary>
        /// <param name="itemId">고유 식별자 (예: "sword_basic")</param>
        /// <param name="itemName">표시 이름 (예: "기본 검")</param>
        /// <param name="grade">아이템 등급</param>
        /// <param name="attackPower">공격력 (1 이상)</param>
        public Weapon(string itemId, string itemName, ItemGrade grade, int attackPower)
            : base(itemId, itemName, grade)
        {
            AttackPower = attackPower < 1 ? 1 : attackPower;
        }
    }
}
