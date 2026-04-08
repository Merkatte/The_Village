using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using Game.Inventory;
using Game.UI.Core;
using UnityEngine;

namespace Game.UI.Inventory
{
    /// <summary>
    /// 인벤토리 팝업 UI 의 MonoBehaviour 브릿지입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - PopupBase 를 상속하여 UIManager 의 생명주기 제어를 받습니다.
    ///   - IInventoryView 를 구현하여 InventoryPresenter 로부터 갱신 신호를 받습니다.
    ///   - 로직은 InventoryPresenter (순수 C#) 에 위임합니다.
    ///
    /// ─ 프리팹 설정 ────────────────────────────────────────────
    ///   1. 루트 GameObject 에 Canvas 컴포넌트를 추가하세요.
    ///      (Render Mode: Screen Space - Overlay, Sorting Order: 10 권장)
    ///   2. _popupCanvas 에 해당 Canvas 를 연결하세요.
    ///   3. 슬롯 Grid 하위에 InventorySlotUI 컴포넌트를 20개 배치하세요.
    ///      (GetComponentsInChildren 으로 자동 수집됩니다)
    ///
    /// ─ 채취 흐름 ──────────────────────────────────────────────
    ///   IInventory.OnInventoryChanged → InventoryPresenter.Refresh()
    ///     → IInventoryView.Refresh() → InventorySlotUI.SetData() × SlotCount
    /// </summary>
    public sealed class InventoryUI : PopupBase, IInventoryView
    {
        [SerializeField] private Canvas _popupCanvas;

        [SerializeField] InventorySlotUI[]   _slots;
        private InventoryPresenter  _presenter;

        private void Awake()
        {
            //_slots = GetComponentsInChildren<InventorySlotUI>();
        }

        /// <inheritdoc/>
        public override void OnOpen()
        {
            var inventory  = ServiceLocator.Get<IInventory>();
            var spriteRepo = ServiceLocator.Get<ISpriteRepository>();

            _presenter = new InventoryPresenter(inventory, spriteRepo);

            var canvasTransform = _popupCanvas != null
                ? _popupCanvas.transform
                : transform;

            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Init(i, _presenter.MoveItem, canvasTransform);

            _presenter.Bind(this);
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            _presenter?.Dispose();
            _presenter = null;
        }

        /// <inheritdoc/>
        public void Refresh(SlotViewModel[] slots)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                var vm = i < slots.Length ? slots[i] : SlotViewModel.Empty;
                _slots[i].SetData(vm);
            }
        }
    }
}
