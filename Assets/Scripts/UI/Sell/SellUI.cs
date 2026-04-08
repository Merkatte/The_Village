using System.Collections.Generic;
using System.Linq;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using Game.Inventory;
using Game.Shop;
using Game.UI.Core;
using Game.UI.Inventory;
using TMPro;
using UnityEngine;

namespace Game.UI.Sell
{
    /// <summary>
    /// 판매 팝업 UI 의 MonoBehaviour 브릿지입니다.
    ///
    /// ─ 구조 ───────────────────────────────────────────────────
    ///   [슬롯 Grid]  Inspector 에서 _slots 에 인벤토리 슬롯 수만큼 연결
    ///   [하단 정보]  _goldText(보유 골드), _sellTotalText(선택 합산 판매가)
    ///   [판매 버튼]  OnSellButtonClicked() 을 Button.onClick 에 연결
    ///
    /// ─ 슬롯 클릭 흐름 ─────────────────────────────────────────
    ///   SellSlotUI 클릭 → OnSlotClicked(slotIndex)
    ///     → SellPresenter.ToggleSlot() → 선택 토글 → View 갱신
    ///
    /// ─ 판매 흐름 ──────────────────────────────────────────────
    ///   판매 버튼 클릭 → SellPresenter.OnSellConfirmed()
    ///     → 선택 슬롯 전부 판매 → OnInventoryChanged → 자동 갱신
    /// </summary>
    public sealed class SellUI : PopupBase, ISellView
    {
        [SerializeField] private SellSlotUI[]      _slots;
        [SerializeField] private TextMeshProUGUI   _goldText;
        [SerializeField] private TextMeshProUGUI   _sellTotalText;

        private SellPresenter _presenter;

        /// <inheritdoc/>
        public override void OnOpen()
        {
            var inventory       = ServiceLocator.Get<IInventory>();
            var shopService     = ServiceLocator.Get<IShopService>();
            var currencyService = ServiceLocator.Get<ICurrencyService>();
            var spriteRepo      = ServiceLocator.Get<ISpriteRepository>();

            _presenter = new SellPresenter(inventory, shopService, currencyService, spriteRepo);

            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Init(i, OnSlotClicked);

            _presenter.Bind(this);
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            _presenter?.Dispose();
            _presenter = null;
        }

        /// <inheritdoc/>
        public void Refresh(SlotViewModel[] slots, IReadOnlyCollection<int> selectedIndices)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                var vm = i < slots.Length ? slots[i] : SlotViewModel.Empty;
                _slots[i].SetData(vm);
                _slots[i].SetSelected(selectedIndices.Contains(i));
            }
        }

        /// <inheritdoc/>
        public void RefreshGold(int gold)
        {
            if (_goldText != null)
                _goldText.text = $"보유 골드: {gold}G";
        }

        /// <inheritdoc/>
        public void RefreshSellTotal(int totalGold)
        {
            if (_sellTotalText != null)
                _sellTotalText.text = $"판매가: {totalGold}G";
        }

        /// <summary>
        /// 판매 버튼 onClick 에 연결합니다.
        /// </summary>
        public void OnSellButtonClicked()
        {
            _presenter?.OnSellConfirmed();
        }

        // ─ 내부 ──────────────────────────────────────────────────

        private void OnSlotClicked(int slotIndex)
        {
            _presenter?.ToggleSlot(slotIndex);
        }
    }
}
