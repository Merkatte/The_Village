using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.UI.Slot;

namespace Game.UI.Inventory
{
    /// <summary>
    /// 인벤토리 슬롯 하나의 표시와 드래그 앤 드롭을 담당하는 MonoBehaviour 입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 표시 로직은 BaseSlotUI.SetData(SlotViewModel) 에 위임합니다.
    ///   - 드래그 시작 슬롯 인덱스는 static 필드로 공유합니다.
    ///     (한 번에 하나의 슬롯만 드래그 가능한 제약 활용)
    ///   - 드롭 완료 시 _onMoveRequested(from, to) 콜백으로 Presenter 에 위임합니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   _iconImage     : 아이템 아이콘 Image 컴포넌트     (BaseSlotUI)
    ///   _quantityText  : 수량 표시 TextMeshProUGUI 컴포넌트 (BaseSlotUI)
    /// </summary>
    public class InventorySlotUI : BaseSlotUI,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        private int              _slotIndex;
        private Action<int, int> _onMoveRequested;
        private Transform        _canvasTransform;
        private CanvasGroup      _canvasGroup;
        private GameObject       _ghostObject;

        // 드래그 중인 슬롯 인덱스를 공유합니다. 드래그 중이 아니면 -1.
        private static int _draggedFromIndex = -1;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>
        /// 슬롯을 초기화합니다. InventoryUI.OnOpen() 에서 호출합니다.
        /// </summary>
        /// <param name="slotIndex">이 슬롯의 인벤토리 인덱스</param>
        /// <param name="onMoveRequested">드래그 완료 시 호출할 (from, to) 콜백</param>
        /// <param name="canvasTransform">드래그 고스트를 배치할 Canvas Transform</param>
        public void Init(int slotIndex, Action<int, int> onMoveRequested, Transform canvasTransform)
        {
            _slotIndex       = slotIndex;
            _onMoveRequested = onMoveRequested;
            _canvasTransform = canvasTransform;
        }

        // ─ 드래그 앤 드롭 ─────────────────────────────────────────

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!_iconImage.enabled) return; // 빈 슬롯은 드래그 불가

            _draggedFromIndex = _slotIndex;

            // 드래그 고스트 생성
            _ghostObject = new GameObject("DragGhost");
            _ghostObject.transform.SetParent(_canvasTransform, false);
            _ghostObject.transform.SetAsLastSibling();

            var ghostRect       = _ghostObject.AddComponent<RectTransform>();
            ghostRect.sizeDelta = GetComponent<RectTransform>().sizeDelta;

            var ghostImage           = _ghostObject.AddComponent<Image>();
            ghostImage.sprite        = _iconImage.sprite;
            ghostImage.raycastTarget = false;

            // 원본 슬롯 반투명 처리
            _canvasGroup.alpha          = 0.4f;
            _canvasGroup.blocksRaycasts = false;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (_ghostObject == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasTransform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint);

            _ghostObject.transform.localPosition = localPoint;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.alpha          = 1f;
            _canvasGroup.blocksRaycasts = true;

            if (_ghostObject != null)
            {
                Destroy(_ghostObject);
                _ghostObject = null;
            }

            _draggedFromIndex = -1;
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            if (_draggedFromIndex < 0 || _draggedFromIndex == _slotIndex) return;
            _onMoveRequested?.Invoke(_draggedFromIndex, _slotIndex);
        }
    }
}
