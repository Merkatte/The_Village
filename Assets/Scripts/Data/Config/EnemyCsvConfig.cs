using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>
    /// 적 데이터 CSV 파일 참조를 보관하는 ScriptableObject 입니다.
    ///
    /// ─ 배치 방법 ──────────────────────────────────────────────
    ///   Assets/Data/Config/ 에 에셋을 생성한 뒤,
    ///   DungeonSceneInitializer 의 [SerializeField] enemyCsvConfig 에 연결합니다.
    ///   (메뉴: Assets > Create > Game/Config/Enemy CSV Config)
    ///
    /// ─ CSV 형식 (12열) ────────────────────────────────────────
    ///   EnemyId, EnemyType, MaxHp, MoveSpeed,
    ///   AlertRange, CombatRange, AttackRange,
    ///   AttackDamage, AttackCooldown, PreAttackDelay, PostAttackStun,
    ///   AttackType
    ///
    ///   예시:
    ///   slime_basic,Slime,30,2,10,6,2,5,2,0.2,0.4,Melee
    /// </summary>
    [CreateAssetMenu(
        menuName = "Game/Config/Enemy CSV Config",
        fileName = "EnemyCsvConfig")]
    public sealed class EnemyCsvConfig : ScriptableObject
    {
        [Header("적 데이터")]
        [Tooltip("EnemyData.csv 파일을 여기에 연결합니다.")]
        public TextAsset enemyCsv;
    }
}
