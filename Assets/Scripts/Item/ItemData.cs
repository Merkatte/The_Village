namespace Game.Item
{
    /// <summary>
    /// 모든 아이템이 공통으로 가지는 기반 데이터 클래스입니다.
    /// Equipment, Tool 등 모든 아이템 클래스가 이 클래스를 상속합니다.
    /// </summary>
    public abstract class ItemData
    {
        /// <summary>아이템 고유 식별자 (예: "axe_iron", "sword_basic")</summary>
        public string ItemId { get; }

        /// <summary>아이템 표시 이름 (예: "철 도끼", "기본 검")</summary>
        public string ItemName { get; }

        /// <summary>아이템 등급</summary>
        public ItemGrade Grade { get; }

        /// <summary>
        /// 최대 스택 수. 1 = 겹치지 않음.
        /// 기본값은 1이며, 겹치는 아이템(ResourceItem 등)에서 오버라이드합니다.
        /// </summary>
        public virtual int MaxStackSize => 1;

        /// <summary>
        /// 아이템 기본 가치 (골드).
        /// 상점 매입/판매가는 이 값에 ShopConfig 의 비율을 곱해 계산합니다.
        /// </summary>
        public virtual int Value => 0;

        /// <summary>ItemData 생성자.</summary>
        /// <param name="itemId">고유 식별자</param>
        /// <param name="itemName">표시 이름</param>
        /// <param name="grade">아이템 등급</param>
        protected ItemData(string itemId, string itemName, ItemGrade grade)
        {
            ItemId   = itemId;
            ItemName = itemName;
            Grade    = grade;
        }
    }

    /// <summary>
    /// 아이템 등급입니다.
    /// 등급이 높을수록 성능 보정치가 커집니다.
    /// </summary>
    public enum ItemGrade
    {
        /// <summary>일반</summary>
        Normal = 0,
        /// <summary>희귀</summary>
        Rare   = 1,
        /// <summary>영웅</summary>
        Epic   = 2
    }
}
