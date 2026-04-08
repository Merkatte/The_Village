using Game.Item.Tool;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 플레이어의 현재 장착 도구를 보관합니다.
    ///
    /// ─ 배치 위치 ────────────────────────────────────────────────
    ///   Player GameObject 에 HarvestController 와 함께 부착합니다.
    ///
    /// ─ 사용 방법 ────────────────────────────────────────────────
    ///   인벤토리 / 장비 시스템에서 EquipHarvestTool() 을 호출하여 도구를 교체합니다.
    ///   HarvestController 가 EquippedTool 을 읽어 채취 가능 여부를 판단합니다.
    /// </summary>
    public sealed class PlayerEquipment : MonoBehaviour
    {
        private HarvestTool _equippedTool;

        /// <summary>현재 장착된 채취 도구. 미장착 시 null.</summary>
        public HarvestTool EquippedTool => _equippedTool;

        /// <summary>채취 도구를 장착합니다.</summary>
        public void EquipHarvestTool(HarvestTool tool) => _equippedTool = tool;

        /// <summary>채취 도구를 해제합니다.</summary>
        public void UnequipHarvestTool() => _equippedTool = null;
    }
}
