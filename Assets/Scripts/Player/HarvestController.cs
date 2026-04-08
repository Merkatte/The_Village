using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Dungeon.Interact;
using Game.Manager.Interfaces.Input;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 채취 홀드 입력을 감지하여 Harvestable 오브젝트를 수확합니다.
    ///
    /// ─ 배치 위치 ────────────────────────────────────────────────
    ///   Player GameObject 에 InteractionDetector, PlayerEquipment 와 함께 부착합니다.
    ///
    /// ─ 채취 흐름 ────────────────────────────────────────────────
    ///   1. 채취 키(임시: Space) 를 누름 → _isHolding = true
    ///   2. Update 마다 가장 가까운 Harvestable 조회
    ///   3. 장착 도구와 Harvestable 타입 호환 여부 확인
    ///      - 도끼(Axe)    → Tree 만 채취 가능
    ///      - 곡괭이(Pickaxe) → Ore  만 채취 가능
    ///   4. 호환되면 _holdTimer 누적 → baseHoldDuration * HarvestDurationMultiplier 도달 시 채취
    ///   5. 채취 후 _holdTimer 초기화 (계속 누르면 다음 사이클 즉시 시작)
    ///   6. 키를 뗌 → _isHolding = false, _holdTimer 초기화
    ///
    /// ─ 도구 미장착 / 호환 불가 ──────────────────────────────────
    ///   _holdTimer 를 누적하지 않고 0 으로 리셋합니다.
    ///   (키를 누르고 있어도 진행되지 않습니다.)
    /// </summary>
    public sealed class HarvestController : MonoBehaviour
    {
        /// <summary>
        /// 기본 채취 홀드 시간(초).
        /// HarvestTool.HarvestDurationMultiplier 가 1.0 일 때 이 값만큼 홀드해야 합니다.
        /// </summary>
        [SerializeField] private float _baseHoldDuration = 2f;

        private IPlayerInputReader  _inputReader;
        private InteractionDetector _interactionDetector;
        private PlayerEquipment     _playerEquipment;

        private bool  _isHolding;
        private float _holdTimer;

        /// <summary>
        /// PlayerInitializer 가 Awake() 에서 호출합니다.
        /// </summary>
        public void Init(InteractionDetector interactionDetector, PlayerEquipment playerEquipment)
        {
            _interactionDetector = interactionDetector;
            _playerEquipment     = playerEquipment;
        }

        private void Awake()
        {
            _inputReader = ServiceLocator.Get<IPlayerInputReader>();

            _inputReader.OnHarvestStarted   += OnHarvestStarted;
            _inputReader.OnHarvestCancelled += OnHarvestCancelled;
        }

        private void OnDestroy()
        {
            if (_inputReader == null) return;
            _inputReader.OnHarvestStarted   -= OnHarvestStarted;
            _inputReader.OnHarvestCancelled -= OnHarvestCancelled;
        }

        private void Update()
        {
            if (!_isHolding) return;

            var target = _interactionDetector.GetNearestHarvestable();
            if (!target)
            {
                _holdTimer = 0f;
                Debug.Log("There is no target! returning...");
                return;
            }

            var tool = _playerEquipment.EquippedTool;
            if (tool == null || !IsCompatible(tool.ToolType, target.HarvestableType))
            {
                _holdTimer = 0f;
                if (tool == null)
                {
                    Debug.Log("There is no tool! returning...");
                }
                else
                {
                    Debug.Log("Tools is not Compatible! returning...");
                }
                return;
            }

            _holdTimer += Time.deltaTime;

            float required = _baseHoldDuration * tool.HarvestDurationMultiplier;
            if (_holdTimer >= required)
            {
                target.Harvest(tool.YieldMultiplier);
                _holdTimer = 0f;
            }
        }

        private void OnHarvestStarted() => _isHolding = true;

        private void OnHarvestCancelled()
        {
            _isHolding = false;
            _holdTimer = 0f;
        }

        /// <summary>
        /// 장착 도구와 Harvestable 타입이 호환되는지 확인합니다.
        /// 도끼 → 나무만 / 곡괭이 → 광물만
        /// </summary>
        private static bool IsCompatible(ToolType toolType, HarvestableType harvestableType)
        {
            return harvestableType switch
            {
                HarvestableType.Tree => toolType == ToolType.Axe,
                HarvestableType.Ore  => toolType == ToolType.Pickaxe,
                _                    => false,
            };
        }
    }
}
