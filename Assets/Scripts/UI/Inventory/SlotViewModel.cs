using System;
using UnityEngine;

namespace Game.UI.Inventory
{
    /// <summary>
    /// 인벤토리 슬롯 하나를 표시하기 위한 데이터 묶음입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 순수 C# 불변 객체입니다. Unity 타입(Sprite) 만 허용합니다.
    ///   - InventoryPresenter 가 생성하여 IInventoryView 로 전달합니다.
    ///   - 빈 슬롯은 SlotViewModel.Empty 싱글턴을 재사용합니다.
    /// </summary>
    public sealed class SlotViewModel
    {
        /// <summary>슬롯이 비어 있으면 true.</summary>
        public bool IsEmpty { get; }

        /// <summary>아이템 아이콘. 비어 있거나 스프라이트 누락 시 null.</summary>
        public Sprite Icon { get; }

        /// <summary>아이템 이름.</summary>
        public string ItemName { get; }

        /// <summary>
        /// 수량 표시 문자열.
        /// 스택 불가 아이템이거나 수량이 1이면 빈 문자열을 반환합니다.
        /// </summary>
        public string QuantityText { get; }

        /// <summary>빈 슬롯을 나타내는 공유 인스턴스입니다.</summary>
        public static readonly SlotViewModel Empty = new SlotViewModel();

        private SlotViewModel()
        {
            IsEmpty = true;
        }

        /// <param name="icon">아이템 아이콘 스프라이트 (null 허용)</param>
        /// <param name="itemName">아이템 이름</param>
        /// <param name="quantityText">수량 문자열 (스택 없으면 string.Empty)</param>
        public SlotViewModel(Sprite icon, string itemName, string quantityText)
        {
            IsEmpty      = false;
            Icon         = icon;
            ItemName     = itemName     ?? throw new ArgumentNullException(nameof(itemName));
            QuantityText = quantityText ?? string.Empty;
        }
    }
}
