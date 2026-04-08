using Game.Core.Enums;
using Game.Dungeon.Interact;
using Game.Item;
using Game.Item.Tool;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Player 계층 구조의 컴포넌트 간 의존성을 연결하는 컴포지션 루트입니다.
    ///
    /// ─ 배치 위치 ──────────────────────────────────────────────
    ///   Player 루트 GameObject 에 부착합니다.
    ///
    /// ─ 역할 ───────────────────────────────────────────────────
    ///   같은 GameObject 내부에서 GetComponent 로 해결할 수 없는
    ///   자식 오브젝트 간 레퍼런스를 한 곳에서 연결합니다.
    ///   ServiceLocator 의존성은 각 스크립트가 직접 처리합니다.
    ///
    /// ─ 초기화 순서 ────────────────────────────────────────────
    ///   Awake() 에서 모든 Init() 을 호출합니다.
    ///   Unity 는 Awake() 를 모두 완료한 뒤 Update() 를 시작하므로
    ///   각 스크립트가 레퍼런스를 사용하기 전에 주입이 보장됩니다.
    /// </summary>
    public sealed class PlayerInitializer : MonoBehaviour
    {
        [Header("루트 컴포넌트")]
        [SerializeField] private HarvestController harvestController;
        [SerializeField] private PlayerEquipment   playerEquipment;

        [Header("자식 컴포넌트")]
        [SerializeField] private InteractionDetector interactionDetector;

        private void Awake()
        {
            harvestController.Init(interactionDetector, playerEquipment);
            HarvestTool tool = new HarvestTool(
                "1", "a", ItemGrade.Epic, ToolType.Axe, 1);
            playerEquipment.EquipHarvestTool(tool);
        }
    }
}
