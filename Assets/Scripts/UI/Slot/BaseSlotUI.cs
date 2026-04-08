using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.UI.Inventory;

namespace Game.UI.Slot
{
    /// <summary>
    /// 아이템 슬롯 UI 의 공통 기반 클래스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 아이콘·수량 표시 필드와 SetData(SlotViewModel) 로직을 공유합니다.
    ///   - 상호작용(드래그, 클릭 선택 등)은 각 서브클래스에서 구현합니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   _iconImage    : 아이템 아이콘 Image 컴포넌트
    ///   _quantityText : 수량 표시 TextMeshProUGUI 컴포넌트
    /// </summary>
    public abstract class BaseSlotUI : MonoBehaviour
    {
        [SerializeField] protected Image           _iconImage;
        [SerializeField] protected TextMeshProUGUI _quantityText;

        /// <summary>
        /// SlotViewModel 로 슬롯 표시를 갱신합니다.
        /// </summary>
        public virtual void SetData(SlotViewModel viewModel)
        {
            if (viewModel.IsEmpty)
            {
                _iconImage.enabled = false;
                _quantityText.text = string.Empty;
                Debug.Log("no data");
                return;
            }

            Debug.Log("there is data!");
            _iconImage.enabled = viewModel.Icon != null;
            if (viewModel.Icon != null)
                _iconImage.sprite = viewModel.Icon;

            _quantityText.text = "x" + viewModel.QuantityText;
        }
    }
}
