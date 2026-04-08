using System.Collections.Generic;
using Game.UI.Inventory;

namespace Game.UI.Sell
{
    /// <summary>
    /// 판매 팝업 UI View 인터페이스입니다.
    /// </summary>
    public interface ISellView
    {
        /// <summary>슬롯 목록과 선택 상태를 갱신합니다.</summary>
        void Refresh(SlotViewModel[] slots, IReadOnlyCollection<int> selectedIndices);

        /// <summary>보유 골드 표시를 갱신합니다.</summary>
        void RefreshGold(int gold);

        /// <summary>선택된 아이템의 합산 판매가 표시를 갱신합니다.</summary>
        void RefreshSellTotal(int totalGold);
    }
}
