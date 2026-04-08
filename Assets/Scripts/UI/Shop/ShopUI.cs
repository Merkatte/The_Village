using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using Game.Shop;
using Game.UI.Core;
using TMPro;
using UnityEngine;

namespace Game.UI.Shop
{
    /// <summary>
    /// 상점 팝업 UI 의 MonoBehaviour 브릿지입니다.
    ///
    /// ─ 구조 ───────────────────────────────────────────────────
    ///   [Big Sale Zone]    Inspector 에서 _bigSlots 에 3개 연결
    ///   [Small Sale Zone]  Inspector 에서 _smallSlots 에 6개 연결
    ///   [하단 정보]        _goldText 에 보유 골드 표시
    ///
    /// ─ 슬롯 클릭 흐름 ─────────────────────────────────────────
    ///   ShopSlotUI 클릭 → OnSlotClicked(shopItemIndex)
    ///     → ShopPresenter.OnBuyRequested() → ShopResult 처리
    /// </summary>
    public sealed class ShopUI : PopupBase, IShopView
    {
        [SerializeField] private ShopSlotUI[]    _bigSlots;
        [SerializeField] private ShopSlotUI[]    _smallSlots;
        [SerializeField] private TextMeshProUGUI _goldText;

        private ShopPresenter _presenter;

        /// <inheritdoc/>
        public override void OnOpen()
        {
            var shopService     = ServiceLocator.Get<IShopService>();
            var currencyService = ServiceLocator.Get<ICurrencyService>();
            var spriteRepo      = ServiceLocator.Get<ISpriteRepository>();

            _presenter = new ShopPresenter(shopService, currencyService, spriteRepo);

            foreach (var slot in _bigSlots)
                slot.Init(OnSlotClicked);

            foreach (var slot in _smallSlots)
                slot.Init(OnSlotClicked);

            _presenter.Bind(this);
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            _presenter?.Dispose();
            _presenter = null;
        }

        /// <inheritdoc/>
        public void RefreshBigSlots(ShopSlotViewModel[] slots)
        {
            for (int i = 0; i < _bigSlots.Length; i++)
            {
                var vm = i < slots.Length ? slots[i] : ShopSlotViewModel.Empty;
                _bigSlots[i].SetData(vm);
            }
        }

        /// <inheritdoc/>
        public void RefreshSmallSlots(ShopSlotViewModel[] slots)
        {
            for (int i = 0; i < _smallSlots.Length; i++)
            {
                var vm = i < slots.Length ? slots[i] : ShopSlotViewModel.Empty;
                _smallSlots[i].SetData(vm);
            }
        }

        /// <inheritdoc/>
        public void RefreshGold(int gold)
        {
            if (_goldText != null)
                _goldText.text = $"보유 골드: {gold}G";
        }

        // ─ 내부 ──────────────────────────────────────────────────

        private void OnSlotClicked(int shopItemIndex)
        {
            if (_presenter == null) return;

            var result = _presenter.OnBuyRequested(shopItemIndex);
            if (result != ShopResult.Success)
                LogShopResult(result);
        }

        private static void LogShopResult(ShopResult result)
        {
            switch (result)
            {
                case ShopResult.NotEnoughGold:
                    Debug.Log("[ShopUI] 골드가 부족합니다.");
                    break;
                case ShopResult.InventoryFull:
                    Debug.Log("[ShopUI] 인벤토리가 가득 찼습니다.");
                    break;
                case ShopResult.InvalidItem:
                    Debug.LogWarning("[ShopUI] 유효하지 않은 아이템입니다.");
                    break;
            }
        }
    }
}
