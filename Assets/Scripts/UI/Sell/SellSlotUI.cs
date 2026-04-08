using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.UI.Slot;

namespace Game.UI.Sell
{
    /// <summary>
    /// 판매 팝업 슬롯 하나의 표시와 선택을 담당하는 MonoBehaviour 입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 표시 로직은 BaseSlotUI.SetData(SlotViewModel) 에 위임합니다.
    ///   - 클릭 시 _onClicked(slotIndex) 콜백으로 SellUI 에 위임합니다.
    ///   - 선택 상태는 SetSelected(bool) 로 외부에서 주입받습니다.
    ///   - 빈 슬롯은 클릭 불가합니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   _iconImage          : 아이템 아이콘 Image 컴포넌트     (BaseSlotUI)
    ///   _quantityText       : 수량 표시 TextMeshProUGUI 컴포넌트 (BaseSlotUI)
    ///   _selectionHighlight : 선택 시 표시할 초록 오버레이 Image
    /// </summary>
    public sealed class SellSlotUI : BaseSlotUI, IPointerClickHandler
    {
        [SerializeField] private Image _selectionHighlight;

        private int         _slotIndex;
        private bool        _isEmpty = true;
        private Action<int> _onClicked;

        /// <summary>
        /// 슬롯을 초기화합니다. SellUI.OnOpen() 에서 호출합니다.
        /// </summary>
        /// <param name="slotIndex">이 슬롯의 인벤토리 인덱스</param>
        /// <param name="onClicked">클릭 시 호출할 (slotIndex) 콜백</param>
        public void Init(int slotIndex, Action<int> onClicked)
        {
            _slotIndex = slotIndex;
            _onClicked = onClicked;
        }

        /// <inheritdoc/>
        public override void SetData(Inventory.SlotViewModel viewModel)
        {
            _isEmpty = viewModel.IsEmpty;
            base.SetData(viewModel);

            if (viewModel.IsEmpty)
                SetSelected(false);
        }

        /// <summary>
        /// 선택 강조 오버레이를 갱신합니다. SellUI.Refresh() 에서 호출합니다.
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.enabled = selected;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (_isEmpty) return;
            _onClicked?.Invoke(_slotIndex);
        }
    }
}
