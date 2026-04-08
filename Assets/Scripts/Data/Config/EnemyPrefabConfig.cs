using System;
using System.Collections.Generic;
using Game.Core.Enums;
using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>
    /// EnemyType 별 프리팹을 보관하는 ScriptableObject 입니다.
    ///
    /// ─ 배치 방법 ──────────────────────────────────────────────
    ///   Assets/Data/Config/ 에 에셋을 생성한 뒤,
    ///   EnemySpawner 의 [SerializeField] enemyPrefabConfig 에 연결합니다.
    ///   (메뉴: Assets > Create > Game/Config/Enemy Prefab Config)
    ///
    /// ─ 동작 규칙 ──────────────────────────────────────────────
    ///   - GetPrefab(EnemyType) : 매핑된 프리팹 반환, 없으면 null 반환
    ///   - 동일 EnemyType 을 중복 등록하면 첫 번째 항목을 사용합니다.
    /// </summary>
    [CreateAssetMenu(
        menuName = "Game/Config/Enemy Prefab Config",
        fileName = "EnemyPrefabConfig")]
    public sealed class EnemyPrefabConfig : ScriptableObject
    {
        [SerializeField] private List<EnemyPrefabEntry> _entries = new();

        /// <summary>
        /// EnemyType 에 해당하는 프리팹을 반환합니다.
        /// 매핑이 없으면 null 을 반환합니다.
        /// </summary>
        public GameObject GetPrefab(EnemyType enemyType)
        {
            foreach (var entry in _entries)
            {
                if (entry.EnemyType == enemyType)
                    return entry.Prefab;
            }

            Debug.LogWarning($"[EnemyPrefabConfig] {enemyType} 에 매핑된 프리팹이 없습니다.");
            return null;
        }
    }

    [Serializable]
    public sealed class EnemyPrefabEntry
    {
        public EnemyType  EnemyType;
        public GameObject Prefab;
    }
}
