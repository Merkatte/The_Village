using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Enemy.Combat;
using Game.Enemy.Core;
using Game.Enemy.Data;
using Game.Enemy.Health;
using Game.Enemy.Movement;
using Game.Player;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// 적 GameObject 에 붙는 MonoBehaviour 브릿지입니다.
    ///
    /// ─ 역할 (SRP) ─────────────────────────────────────────────
    ///   - Awake  : IEnemyRepository 에서 EnemyData 로드 후 순수 C# 로직 객체 구성
    ///   - Start  : ServiceLocator 에서 PlayerHealth 참조 획득
    ///   - Update : Brain.Tick() 으로 상태 결정 → 상태별 행동 실행
    ///   - 이동·체력·공격 로직 자체는 각 순수 C# 클래스에 위임합니다. (DIP)
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   enemyId : EnemyData.csv 의 EnemyId 값과 일치해야 합니다.
    ///
    /// ─ 상태별 행동 ────────────────────────────────────────────
    ///   Idle      : 스폰 지점 근처를 배회 (WanderRadius 범위 내 랜덤 이동)
    ///   Alert     : 마지막으로 플레이어를 목격한 위치로 이동 (탐색)
    ///   Combat    : 플레이어를 직접 추적. 매 프레임 마지막 목격 위치 갱신
    ///   Attacking : 정지 + 쿨타임 끝나면 공격 시퀀스 실행
    ///   Dead      : 모든 행동 중단 → 0.5 초 후 파괴
    /// </summary>
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private string enemyId;

        private EnemyBrain    _brain;
        private EnemyMover    _mover;
        private EnemyHealth   _health;
        private EnemyAttacker _attacker;

        private PlayerHealth  _playerHealth;
        private EnemyState    _currentState;

        private CancellationTokenSource _cts;

        // ─ 이동 ───────────────────────────────────────────────
        private const float ArrivalThreshold = 0.15f;

        private EnemyWanderer _wanderer;
        private Vector2       _lastKnownPlayerPos;

        // ─ 생명주기 ───────────────────────────────────────────

        private void Awake()
        {
            var repo = ServiceLocator.Get<IEnemyRepository>();
            if (repo == null)
            {
                Debug.LogError("[EnemyController] IEnemyRepository 가 ServiceLocator 에 등록되지 않았습니다.");
                enabled = false;
                return;
            }

            var data = repo.GetById(enemyId);
            if (data == null)
            {
                Debug.LogError($"[EnemyController] enemyId '{enemyId}' 를 찾을 수 없습니다. EnemyData.csv 를 확인하세요.");
                enabled = false;
                return;
            }

            _wanderer = new EnemyWanderer(transform.position, data.WanderRadius, data.WanderInterval);

            _brain   = new EnemyBrain(data);
            _mover   = new EnemyMover(data.MoveSpeed, transform.position);
            _health  = new EnemyHealth(data.MaxHp);

            // TODO Phase 6: data.AttackType == Ranged 일 때 RangedAttackStrategy 로 분기
            var strategy = new MeleeAttackStrategy(data.AttackDamage);
            _attacker    = new EnemyAttacker(strategy, data.PreAttackDelay, data.PostAttackStun, data.AttackCooldown);

            _health.OnDied += HandleDeath;
            _cts = new CancellationTokenSource();
        }

        private void Start()
        {
            _playerHealth = ServiceLocator.Get<PlayerHealth>();
            if (_playerHealth == null)
                Debug.LogWarning("[EnemyController] PlayerHealth 가 ServiceLocator 에 없습니다. 플레이어 GameObject 에 PlayerHealth 가 부착되어 있는지 확인하세요.");
        }

        private void Update()
        {
            if (_currentState == EnemyState.Dead) return;

            // 플레이어 없으면 배회만 수행
            if (_playerHealth == null)
            {
                _mover.Move(_wanderer.GetMoveDirection(_mover.Position, Time.deltaTime), Time.deltaTime);
                ApplyTransformPosition();
                return;
            }

            var playerPos = (Vector2)_playerHealth.transform.position;
            var distance  = Vector2.Distance(_mover.Position, playerPos);

            var prevState = _currentState;
            _currentState = _brain.Tick(_currentState, distance, _attacker.IsOnCooldown);

            // 상태 진입 시 처리
            if (prevState != _currentState)
                OnStateEntered(prevState, _currentState, playerPos);

            // Combat·Attacking 중에는 매 프레임 마지막 목격 위치 갱신
            if (_currentState == EnemyState.Combat || _currentState == EnemyState.Attacking)
                _lastKnownPlayerPos = playerPos;

            switch (_currentState)
            {
                case EnemyState.Idle:
                    _mover.Move(_wanderer.GetMoveDirection(_mover.Position, Time.deltaTime), Time.deltaTime);
                    break;

                case EnemyState.Alert:
                    MoveToward(_lastKnownPlayerPos);
                    break;

                case EnemyState.Combat:
                    MoveToward(playerPos);
                    break;

                case EnemyState.Attacking:
                    if (!_attacker.IsOnCooldown)
                        _attacker.RequestAttackAsync(_playerHealth, _cts.Token).Forget();
                    break;
            }

            ApplyTransformPosition();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            if (_health != null)
                _health.OnDied -= HandleDeath;
        }

        // ─ 상태 진입 처리 ─────────────────────────────────────

        private void OnStateEntered(EnemyState from, EnemyState to, Vector2 playerPos)
        {
            if (to == EnemyState.Alert && from == EnemyState.Idle)
                _lastKnownPlayerPos = playerPos;   // 처음 감지한 위치 기록

            if (to == EnemyState.Idle)
                _wanderer.ResetTimer();            // 귀환 시 배회 즉시 재개
        }

        // ─ 행동 로직 ──────────────────────────────────────────

        private void MoveToward(Vector2 target)
        {
            var direction = target - _mover.Position;
            if (direction.magnitude < ArrivalThreshold) return;
            _mover.Move(direction, Time.deltaTime);
        }

        private void ApplyTransformPosition()
        {
            transform.position = new Vector3(_mover.Position.x, _mover.Position.y, transform.position.z);
        }

        private void HandleDeath()
        {
            _currentState = EnemyState.Dead;
            _cts.Cancel();

            // 향후 사망 애니메이션·이펙트 훅 포인트
            Destroy(gameObject, 0.5f);
        }
    }
}
