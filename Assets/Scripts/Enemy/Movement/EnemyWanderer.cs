using UnityEngine;

namespace Game.Enemy.Movement
{
    /// <summary>
    /// 적의 배회(Idle) 이동 계산을 담당하는 순수 C# 클래스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - MonoBehaviour 가 아니므로 EditMode 에서 단위 테스트 가능
    ///   - 스폰 위치(_origin) 근처 WanderRadius 반경 내 랜덤 지점으로 주기적 이동
    ///   - 이동 방향 계산만 책임집니다. 실제 이동은 EnemyMover 에 위임. (SRP)
    ///
    /// ─ 사용법 ─────────────────────────────────────────────────
    ///   매 Update 마다 GetMoveDirection(currentPos, deltaTime) 을 호출하고
    ///   반환된 방향을 EnemyMover.Move() 에 전달합니다.
    /// </summary>
    public sealed class EnemyWanderer
    {
        private const float ArrivalThreshold = 0.15f;

        private readonly Vector2 _origin;
        private readonly float   _wanderRadius;
        private readonly float   _wanderInterval;

        private Vector2 _target;
        private float   _timer;

        /// <param name="origin">스폰(귀환) 기준 위치</param>
        /// <param name="wanderRadius">배회 반경 (Unity 유닛)</param>
        /// <param name="wanderInterval">목적지 도달 후 다음 목적지까지 대기 시간 (초)</param>
        public EnemyWanderer(Vector2 origin, float wanderRadius, float wanderInterval)
        {
            _origin         = origin;
            _wanderRadius   = Mathf.Max(0f, wanderRadius);
            _wanderInterval = Mathf.Max(0f, wanderInterval);
            _target         = origin;
        }

        /// <summary>
        /// deltaTime 만큼 진행하여 이동 방향을 반환합니다.
        /// 목적지에 도달했거나 타이머가 만료되면 새 목적지를 선택합니다.
        /// 목적지에 충분히 가까우면 Vector2.zero 를 반환합니다.
        /// </summary>
        public Vector2 GetMoveDirection(Vector2 currentPos, float deltaTime)
        {
            _timer -= deltaTime;

            if (_timer <= 0f || Vector2.Distance(currentPos, _target) < ArrivalThreshold)
            {
                var angle  = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                var radius = Random.Range(0f, _wanderRadius);
                _target = _origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                _timer  = _wanderInterval;
            }

            var direction = _target - currentPos;
            return direction.magnitude < ArrivalThreshold ? Vector2.zero : direction;
        }

        /// <summary>
        /// 타이머를 초기화하여 다음 호출 시 즉시 새 목적지를 선택하게 합니다.
        /// Idle 상태로 복귀할 때 호출하세요.
        /// </summary>
        public void ResetTimer() => _timer = 0f;
    }
}
