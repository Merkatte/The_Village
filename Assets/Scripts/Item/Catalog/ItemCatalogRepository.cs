using System;
using System.Collections.Generic;
using Game.Core.Enums;
using UnityEngine;

namespace Game.Item.Catalog
{
    /// <summary>
    /// IItemCatalogRepository 구현체입니다.
    /// 파싱된 목록을 받아 ItemId 및 ResourceType 기반 조회를 제공합니다.
    /// </summary>
    public sealed class ItemCatalogRepository : IItemCatalogRepository
    {
        private readonly Dictionary<string, ItemCatalogEntry>       _byId;
        private readonly Dictionary<ResourceType, ItemCatalogEntry> _byResourceType;
        private readonly List<ItemCatalogEntry>                     _all;

        public ItemCatalogRepository(IReadOnlyList<ItemCatalogEntry> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            _byId           = new Dictionary<string, ItemCatalogEntry>(StringComparer.OrdinalIgnoreCase);
            _byResourceType = new Dictionary<ResourceType, ItemCatalogEntry>();
            _all            = new List<ItemCatalogEntry>(entries);

            foreach (var entry in entries)
            {
                _byId[entry.ItemId] = entry;

                if (entry.ResourceType.HasValue)
                {
                    Debug.Log(entry.ResourceType.Value);
                    _byResourceType[entry.ResourceType.Value] = entry;
                }
            }
        }

        /// <inheritdoc/>
        public ItemCatalogEntry GetEntry(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;
            return _byId.TryGetValue(itemId, out var entry) ? entry : null;
        }

        /// <inheritdoc/>
        public ItemCatalogEntry GetEntryByResourceType(ResourceType resourceType)
            => _byResourceType.TryGetValue(resourceType, out var entry) ? entry : null;

        /// <inheritdoc/>
        public IReadOnlyList<ItemCatalogEntry> GetAll() => _all;
    }
}
