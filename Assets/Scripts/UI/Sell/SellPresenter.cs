using System;
using System.Collections.Generic;
using Game.Core.Enums;
using Game.Core.Interfaces;
using Game.Inventory;
using Game.Shop;
using Game.UI.Inventory;

namespace Game.UI.Sell
{
    /// <summary>
    /// 판매 팝업 UI 의 로직을 담당하는 순수 C# Presenter 입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 선택 상태(HashSet)를 보관하며 ToggleSlot / OnSellConfirmed 으로 조작합니다.
    ///   - IInventory.OnInventoryChanged 구독으로 판매 후 View 자동 갱신합니다.
    ///   - ICurrencyService.OnCurrencyChanged 구독으로 골드 표시 자동 갱신합니다.
    ///
    /// ─ 생명주기 ───────────────────────────────────────────────
    ///   생성    → 이벤트 구독
    ///   Bind()  → View 연결 + 즉시 전체 갱신
    ///   Dispose → 구독 해제, 선택 초기화, View 참조 해제
    /// </summary>
    public sealed class SellPresenter : IDisposable
    {
        private readonly IInventory        _inventory;
        private readonly IShopService      _shopService;
        private readonly ICurrencyService  _currencyService;
        private readonly ISpriteRepository _spriteRepo;
        private readonly HashSet<int>      _selectedSlots = new HashSet<int>();
        private          ISellView         _view;

        public SellPresenter(
            IInventory        inventory,
            IShopService      shopService,
            ICurrencyService  currencyService,
            ISpriteRepository spriteRepo)
        {
            _inventory       = inventory       ?? throw new ArgumentNullException(nameof(inventory));
            _shopService     = shopService     ?? throw new ArgumentNullException(nameof(shopService));
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            _spriteRepo      = spriteRepo      ?? throw new ArgumentNullException(nameof(spriteRepo));

            _inventory.OnInventoryChanged          += OnInventoryChanged;
            _currencyService.OnCurrencyChanged     += OnCurrencyChanged;
        }

        /// <summary>View 를 연결하고 현재 상태로 즉시 갱신합니다.</summary>
        public void Bind(ISellView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            Refresh();
        }

        /// <summary>View 연결을 해제합니다.</summary>
        public void Unbind() => _view = null;

        /// <summary>
        /// 슬롯 선택 상태를 토글합니다. SellUI 의 슬롯 클릭 시 호출됩니다.
        /// </summary>
        public void ToggleSlot(int slotIndex)
        {
            if (_inventory.GetSlot(slotIndex) == null) return;

            if (_selectedSlots.Contains(slotIndex))
                _selectedSlots.Remove(slotIndex);
            else
                _selectedSlots.Add(slotIndex);

            Refresh();
        }

        /// <summary>
        /// 선택된 슬롯의 아이템을 모두 판매합니다. 판매 버튼 클릭 시 호출됩니다.
        /// </summary>
        public void OnSellConfirmed()
        {
            if (_selectedSlots.Count == 0) return;

            // 높은 인덱스부터 제거해야 앞 슬롯 제거 시 인덱스 밀림을 방지합니다.
            var toSell = new int[_selectedSlots.Count];
            _selectedSlots.CopyTo(toSell);
            Array.Sort(toSell);
            Array.Reverse(toSell);

            foreach (var idx in toSell)
                _shopService.TrySell(idx);

            _selectedSlots.Clear();
            // OnInventoryChanged 이벤트로 Refresh 가 자동 호출됩니다.
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _inventory.OnInventoryChanged      -= OnInventoryChanged;
            _currencyService.OnCurrencyChanged -= OnCurrencyChanged;
            _selectedSlots.Clear();
            _view = null;
        }

        // ─ 내부 ──────────────────────────────────────────────────

        private void OnInventoryChanged()
        {
            // 판매되어 사라진 슬롯의 선택 상태를 제거합니다.
            _selectedSlots.RemoveWhere(i => _inventory.GetSlot(i) == null);
            Refresh();
        }

        private void OnCurrencyChanged(CurrencyType type, int _)
        {
            if (type == CurrencyType.Gold) RefreshGold();
        }

        private void Refresh()
        {
            RefreshSlots();
            RefreshGold();
            RefreshSellTotal();
        }

        private void RefreshSlots()
        {
            if (_view == null) return;

            var viewModels = new SlotViewModel[_inventory.SlotCount];

            for (int i = 0; i < _inventory.SlotCount; i++)
            {
                var item = _inventory.GetSlot(i);

                if (item == null)
                {
                    viewModels[i] = SlotViewModel.Empty;
                    continue;
                }

                var icon         = _spriteRepo.GetSprite(item.ItemId);
                var quantityText = item is IStackable stackable && stackable.Quantity > 1
                    ? stackable.Quantity.ToString()
                    : string.Empty;

                viewModels[i] = new SlotViewModel(icon, item.ItemName, quantityText);
            }

            _view.Refresh(viewModels, _selectedSlots);
        }

        private void RefreshGold()
        {
            if (_view == null) return;
            _view.RefreshGold(_currencyService.Get(CurrencyType.Gold));
        }

        private void RefreshSellTotal()
        {
            if (_view == null) return;

            int total = 0;
            foreach (var idx in _selectedSlots)
            {
                var item = _inventory.GetSlot(idx);
                if (item != null)
                    total += _shopService.GetSellPrice(item);
            }

            _view.RefreshSellTotal(total);
        }
    }
}
