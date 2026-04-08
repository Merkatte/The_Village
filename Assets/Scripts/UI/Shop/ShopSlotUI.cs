using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Shop
{
    /// <summary>
    /// 상점 슬롯 하나의 표시와 클릭을 담당하는 MonoBehaviour 입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 데이터는 SetData(ShopSlotViewModel) 로만 수신합니다.
    ///   - 클릭 시 _onClicked(SlotIndex) 콜백으로 ShopUI 에 위임합니다.
    ///   - 빈 슬롯은 클릭 불가합니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   _iconImage   : 아이템 아이콘 Image 컴포넌트
    ///   _itemNameText: 아이템 이름 TextMeshProUGUI 컴포넌트
    ///   _priceText   : 가격 표시 TextMeshProUGUI 컴포넌트
    /// </summary>
    public sealed class ShopSlotUI : MonoBehaviour
    {
        [SerializeField] private Image             _iconImage;
        [SerializeField] private TextMeshProUGUI   _itemNameText;
        [SerializeField] private TextMeshProUGUI   _priceText;

        private int         _slotIndex;
        private bool        _isEmpty = true;
        private Action<int> _onClicked;

        /// <summary>
        /// 슬롯을 초기화합니다. ShopUI.OnOpen() 에서 호출합니다.
        /// </summary>
        /// <param name="onClicked">클릭 시 호출할 (slotIndex) 콜백</param>
        public void Init(Action<int> onClicked)
        {
            _onClicked = onClicked;
        }

        /// <summary>
        /// ShopSlotViewModel 로 슬롯 표시를 갱신합니다.
        /// </summary>
        public void SetData(ShopSlotViewModel viewModel)
        {
            _isEmpty = viewModel.IsEmpty;

            if (viewModel.IsEmpty)
            {
                _iconImage.enabled    = false;
                _itemNameText.text    = string.Empty;
                _priceText.text       = string.Empty;
                return;
            }

            _slotIndex            = viewModel.SlotIndex;
            _iconImage.enabled    = viewModel.Icon != null;
            if (viewModel.Icon != null)
                _iconImage.sprite = viewModel.Icon;

            _itemNameText.text = viewModel.ItemName;
            _priceText.text    = viewModel.PriceText;
        }

        /// <summary>
        /// Button Click 이벤트 콜백 입니다.
        /// </summary>
        public void OnPurchaseButtonClicked()
        {
            if (_isEmpty) return;
            _onClicked?.Invoke(_slotIndex);
        }

        // void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        // {
        //
        // }
    }
}
