using System;
using Game.Core.Enums;

namespace Game.Enemy.Core
{
    /// <summary>
    /// CSV 에서 로드되는 적의 불변 수치 데이터입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 순수 C# (MonoBehaviour 아님) → EditMode 테스트 가능
    ///   - 생성 후 수치 변경 불가 (get-only 프로퍼티)
    ///   - 범위 관계: AttackRange ≤ CombatRange ≤ AlertRange
    /// </summary>
    public sealed class EnemyData
    {
        /// <summary>CSV EnemyId (예: "slime_basic")</summary>
        public string EnemyId { get; }

        /// <summary>적 종류</summary>
        public EnemyType EnemyType { get; }

        /// <summary>최대 체력</summary>
        public int MaxHp { get; }

        /// <summary>초당 이동 속도 (Unity 유닛/초)</summary>
        public float MoveSpeed { get; }

        /// <summary>플레이어를 감지하는 거리</summary>
        public float AlertRange { get; }

        /// <summary>전투(추적) 상태로 전환되는 거리</summary>
        public float CombatRange { get; }

        /// <summary>공격 판정이 발생하는 거리</summary>
        public float AttackRange { get; }

        /// <summary>1회 공격 피해량</summary>
        public int AttackDamage { get; }

        /// <summary>공격 후 다음 공격까지의 대기 시간 (초)</summary>
        public float AttackCooldown { get; }

        /// <summary>공격 발동 전 선딜 (초)</summary>
        public float PreAttackDelay { get; }

        /// <summary>공격 발동 후 후딜(멍) (초)</summary>
        public float PostAttackStun { get; }

        /// <summary>공격 방식</summary>
        public AttackType AttackType { get; }

        /// <summary>Idle 배회 반경 (스폰 위치 기준, Unity 유닛)</summary>
        public float WanderRadius { get; }

        /// <summary>배회 목적지 도달 후 다음 목적지까지의 대기 시간 (초)</summary>
        public float WanderInterval { get; }

        public EnemyData(
            string     enemyId,
            EnemyType  enemyType,
            int        maxHp,
            float      moveSpeed,
            float      alertRange,
            float      combatRange,
            float      attackRange,
            int        attackDamage,
            float      attackCooldown,
            float      preAttackDelay,
            float      postAttackStun,
            AttackType attackType,
            float      wanderRadius   = 3f,
            float      wanderInterval = 3f)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
                throw new ArgumentException("[EnemyData] enemyId 는 비어 있을 수 없습니다.", nameof(enemyId));
            if (maxHp <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxHp), "[EnemyData] maxHp 은 1 이상이어야 합니다.");

            EnemyId        = enemyId;
            EnemyType      = enemyType;
            MaxHp          = maxHp;
            MoveSpeed      = moveSpeed  < 0f ? 0f : moveSpeed;
            AlertRange     = alertRange < 0f ? 0f : alertRange;
            CombatRange    = combatRange < 0f ? 0f : combatRange;
            AttackRange    = attackRange < 0f ? 0f : attackRange;
            AttackDamage   = attackDamage;
            AttackCooldown = attackCooldown < 0f ? 0f : attackCooldown;
            PreAttackDelay = preAttackDelay < 0f ? 0f : preAttackDelay;
            PostAttackStun = postAttackStun < 0f ? 0f : postAttackStun;
            AttackType     = attackType;
            WanderRadius   = wanderRadius   < 0f ? 0f : wanderRadius;
            WanderInterval = wanderInterval < 0f ? 0f : wanderInterval;
        }
    }
}
