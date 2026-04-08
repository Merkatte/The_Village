using System;
using Game.Core.Interfaces;
using Game.Inventory;

namespace Game.UI.Inventory
{
    /// <summary>
    /// 인벤토리 UI 의 로직을 담당하는 순수 C# Presenter 입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - IInventory.OnInventoryChanged 를 구독하여 View 를 자동 갱신합니다.
    ///   - IInventoryView 인터페이스를 통해 View 와 통신합니다. (Unity 비의존)
    ///   - IDisposable 로 이벤트 구독을 안전하게 해제합니다.
    ///     InventoryUI.OnClose() 에서 반드시 Dispose() 를 호출하세요.
    ///
    /// ─ 생명주기 ───────────────────────────────────────────────
    ///   생성    → IInventory.OnInventoryChanged 구독
    ///   Bind()  → View 연결 + 즉시 전체 갱신
    ///   Dispose → 구독 해제, View 참조 해제
    /// </summary>
    public sealed class InventoryPresenter : IDisposable
    {
        private readonly IInventory          _inventory;
        private readonly ISpriteRepository   _spriteRepository;
        private          IInventoryView      _view;

        public InventoryPresenter(IInventory inventory, ISpriteRepository spriteRepository)
        {
            _inventory        = inventory        ?? throw new ArgumentNullException(nameof(inventory));
            _spriteRepository = spriteRepository ?? throw new ArgumentNullException(nameof(spriteRepository));

            _inventory.OnInventoryChanged += OnInventoryChanged;
        }

        /// <summary>
        /// View 를 연결하고 현재 인벤토리 상태로 즉시 갱신합니다.
        /// </summary>
        public void Bind(IInventoryView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            Refresh();
        }

        /// <summary>
        /// View 연결을 해제합니다.
        /// </summary>
        public void Unbind()
        {
            _view = null;
        }

        /// <summary>
        /// 두 슬롯의 아이템을 교환합니다. 드래그 앤 드롭에서 호출합니다.
        /// </summary>
        public void MoveItem(int fromIndex, int toIndex)
        {
            _inventory.MoveItem(fromIndex, toIndex);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _inventory.OnInventoryChanged -= OnInventoryChanged;
            _view = null;
        }

        // ─ 내부 ──────────────────────────────────────────────────

        private void OnInventoryChanged() => Refresh();

        private void Refresh()
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

                var icon         = _spriteRepository.GetSprite(item.ItemId);
                var quantityText = item is IStackable stackable && stackable.Quantity > 1
                    ? stackable.Quantity.ToString()
                    : string.Empty;

                viewModels[i] = new SlotViewModel(icon, item.ItemName, quantityText);
            }

            _view.Refresh(viewModels);
        }
    }
}
