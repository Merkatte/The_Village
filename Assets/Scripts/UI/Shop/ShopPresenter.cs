using System;
using Game.Core.Enums;
using Game.Core.Interfaces;
using Game.Shop;

namespace Game.UI.Shop
{
    /// <summary>
    /// 상점 UI 의 로직을 담당하는 순수 C# Presenter 입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 구매 전용. 판매 기능 없음.
    ///   - ShopItems 압 3개 → Big Zone, 나머지 최대 6개 → Small Zone.
    ///   - ICurrencyService.OnCurrencyChanged 구독으로 골드 자동 갱신.
    ///
    /// ─ Big / Small 분리 기준 ──────────────────────────────────
    ///   ShopItems 인덱스 0 ~ 2 → Big Sale Zone  (최대 3개)
    ///   ShopItems 인덱스 3 ~ 8 → Small Sale Zone (최대 6개)
    /// </summary>
    public sealed class ShopPresenter : IDisposable
    {
        private const int BIG_SLOT_COUNT   = 3;
        private const int SMALL_SLOT_COUNT = 6;

        private readonly IShopService      _shopService;
        private readonly ICurrencyService  _currencyService;
        private readonly ISpriteRepository _spriteRepo;
        private          IShopView         _view;

        public ShopPresenter(
            IShopService      shopService,
            ICurrencyService  currencyService,
            ISpriteRepository spriteRepo)
        {
            _shopService     = shopService     ?? throw new ArgumentNullException(nameof(shopService));
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            _spriteRepo      = spriteRepo      ?? throw new ArgumentNullException(nameof(spriteRepo));

            _currencyService.OnCurrencyChanged += OnCurrencyChanged;
        }

        /// <summary>View 를 연결하고 현재 상태로 즉시 갱신합니다.</summary>
        public void Bind(IShopView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            Refresh();
        }

        /// <summary>View 연결을 해제합니다.</summary>
        public void Unbind() => _view = null;

        /// <summary>
        /// 구매 요청을 처리합니다.
        /// ShopUI 의 슬롯 클릭 시 호출됩니다.
        /// </summary>
        public ShopResult OnBuyRequested(int shopItemIndex)
            => _shopService.TryBuy(shopItemIndex);

        /// <inheritdoc/>
        public void Dispose()
        {
            _currencyService.OnCurrencyChanged -= OnCurrencyChanged;
            _view = null;
        }

        // ─ 내부 ──────────────────────────────────────────────────

        private void Refresh()
        {
            RefreshSlots();
            RefreshGold();
        }

        private void RefreshSlots()
        {
            if (_view == null) return;

            var items = _shopService.ShopItems;

            // Big Zone: 앞 3개
            int bigCount = Math.Min(items.Count, BIG_SLOT_COUNT);
            var bigVMs   = new ShopSlotViewModel[bigCount];
            for (int i = 0; i < bigCount; i++)
            {
                var icon  = _spriteRepo.GetSprite(items[i].ItemId);
                bigVMs[i] = new ShopSlotViewModel(icon, items[i].ItemName, items[i].BuyPrice, i);
            }

            // Small Zone: 인덱스 3부터 최대 6개
            int smallStart = BIG_SLOT_COUNT;
            int smallCount = Math.Max(Math.Min(items.Count - smallStart, SMALL_SLOT_COUNT), 0);
            var smallVMs   = new ShopSlotViewModel[smallCount];
            for (int i = 0; i < smallCount; i++)
            {
                int idx     = smallStart + i;
                var icon    = _spriteRepo.GetSprite(items[idx].ItemId);
                smallVMs[i] = new ShopSlotViewModel(icon, items[idx].ItemName, items[idx].BuyPrice, idx);
            }

            _view.RefreshBigSlots(bigVMs);
            _view.RefreshSmallSlots(smallVMs);
        }

        private void RefreshGold()
        {
            if (_view == null) return;
            _view.RefreshGold(_currencyService.Get(CurrencyType.Gold));
        }

        private void OnCurrencyChanged(CurrencyType type, int _)
        {
            if (type == CurrencyType.Gold) RefreshGold();
        }
    }
}
