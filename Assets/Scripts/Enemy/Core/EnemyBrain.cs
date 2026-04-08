using System;
using Game.Core.Enums;

namespace Game.Enemy.Core
{
    /// <summary>
    /// 적의 상태 전환 로직을 담당하는 순수 C# 클래스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - MonoBehaviour 가 아니므로 EditMode 에서 단위 테스트 가능
    ///   - 상태 결정만 책임집니다. 이동·공격 실행은 EnemyController 담당. (SRP)
    ///   - 매 Update 마다 Tick() 을 호출하여 상태를 갱신합니다.
    ///
    /// ─ 상태 전환 규칙 (FSM) ───────────────────────────────────
    ///   Idle      → Alert     : 플레이어가 AlertRange 이내 진입
    ///   Alert     → Combat    : 플레이어가 CombatRange 이내 진입
    ///   Alert     → Idle      : 플레이어가 AlertRange 밖으로 이탈
    ///   Combat    → Attacking : 플레이어가 AttackRange 이내 진입
    ///   Combat    → Alert     : 플레이어가 CombatRange 밖으로 이탈 (놓침)
    ///   Attacking → Combat    : 플레이어가 AttackRange 밖으로 이탈
    /// </summary>
    public sealed class EnemyBrain
    {
        private readonly EnemyData _data;

        /// <param name="data">이 적의 수치 데이터</param>
        public EnemyBrain(EnemyData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// 현재 상태와 플레이어까지의 거리를 기반으로 다음 상태를 반환합니다.
        /// </summary>
        /// <param name="current">현재 EnemyState</param>
        /// <param name="distance">플레이어까지의 거리</param>
        /// <param name="isAttacking">현재 공격 시퀀스가 진행 중인지 여부</param>
        /// <returns>전환할 EnemyState</returns>
        public EnemyState Tick(EnemyState current, float distance, bool isAttacking)
        {
            if (current == EnemyState.Dead) return EnemyState.Dead;

            switch (current)
            {
                case EnemyState.Idle:
                    return distance <= _data.AlertRange ? EnemyState.Alert : EnemyState.Idle;

                case EnemyState.Alert:
                    if (distance <= _data.CombatRange) return EnemyState.Combat;
                    if (distance > _data.AlertRange)   return EnemyState.Idle;
                    return EnemyState.Alert;

                case EnemyState.Combat:
                    if (distance <= _data.AttackRange) return EnemyState.Attacking;
                    if (distance > _data.CombatRange)  return EnemyState.Alert;
                    return EnemyState.Combat;

                case EnemyState.Attacking:
                    if (isAttacking)                   return EnemyState.Attacking;
                    if (distance <= _data.AttackRange) return EnemyState.Attacking;
                    return EnemyState.Combat;

                default:
                    return EnemyState.Idle;
            }
        }
    }
}
