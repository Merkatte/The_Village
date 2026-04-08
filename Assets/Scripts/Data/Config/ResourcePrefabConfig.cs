using System;
using System.Collections.Generic;
using Game.Core.Enums;
using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>
    /// ResourceType 별 프리팹을 보관하는 ScriptableObject 입니다.
    ///
    /// ─ 배치 방법 ──────────────────────────────────────────────
    ///   Assets/Data/Config/ 에 에셋을 생성한 뒤,
    ///   HarvestableSpawner 의 [SerializeField] resourcePrefabConfig 에 연결합니다.
    ///   (메뉴: Assets > Create > Game/Config/Resource Prefab Config)
    ///
    /// ─ 동작 규칙 ──────────────────────────────────────────────
    ///   - GetPrefab(ResourceType) : 매핑된 프리팹 반환, 없으면 null 반환
    ///   - 동일 ResourceType 을 중복 등록하면 첫 번째 항목을 사용합니다.
    /// </summary>
    [CreateAssetMenu(
        menuName = "Game/Config/Resource Prefab Config",
        fileName = "ResourcePrefabConfig")]
    public sealed class ResourcePrefabConfig : ScriptableObject
    {
        [SerializeField] private List<ResourcePrefabEntry> _entries = new();

        /// <summary>
        /// ResourceType 에 해당하는 프리팹을 반환합니다.
        /// 매핑이 없으면 null 을 반환합니다.
        /// </summary>
        public GameObject GetPrefab(ResourceType resourceType)
        {
            foreach (var entry in _entries)
            {
                if (entry.ResourceType == resourceType)
                    return entry.Prefab;
            }

            Debug.LogWarning($"[ResourcePrefabConfig] {resourceType} 에 매핑된 프리팹이 없습니다.");
            return null;
        }
    }

    [Serializable]
    public sealed class ResourcePrefabEntry
    {
        public ResourceType ResourceType;
        public GameObject   Prefab;
    }
}
