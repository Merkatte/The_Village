using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>
    /// 던전 씬 내 적 스폰 위치를 정의하는 ScriptableObject 입니다.
    ///
    /// ─ 배치 방법 ──────────────────────────────────────────────
    ///   Assets/Data/Config/ 에 에셋을 생성한 뒤,
    ///   EnemySpawner 의 [SerializeField] enemySpawnConfig 에 연결합니다.
    ///   (메뉴: Assets > Create > Game/Config/Enemy Spawn Config)
    ///
    /// ─ 입력 방법 ──────────────────────────────────────────────
    ///   Entries 목록에 항목 추가:
    ///     EnemyId     : EnemyData.csv 의 EnemyId 와 일치해야 합니다.
    ///     Position    : 월드 좌표 스폰 위치
    ///     SpawnChance : 0~1 사이 확률 (1 = 반드시 스폰)
    /// </summary>
    [CreateAssetMenu(
        menuName = "Game/Config/Enemy Spawn Config",
        fileName = "EnemySpawnConfig")]
    public sealed class EnemySpawnConfig : ScriptableObject
    {
        [SerializeField] private List<EnemySpawnEntry> _entries = new();

        public IReadOnlyList<EnemySpawnEntry> Entries => _entries;
    }

    [Serializable]
    public sealed class EnemySpawnEntry
    {
        [Tooltip("EnemyData.csv 의 EnemyId")]
        public string  EnemyId;

        [Tooltip("월드 좌표 스폰 위치")]
        public Vector2 Position;

        [Range(0f, 1f)]
        [Tooltip("스폰 확률 (1 = 반드시 스폰)")]
        public float   SpawnChance = 1f;
    }
}
