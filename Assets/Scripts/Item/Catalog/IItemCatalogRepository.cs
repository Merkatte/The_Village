using System.Collections.Generic;
using Game.Core.Enums;

namespace Game.Item.Catalog
{
    /// <summary>
    /// 아이템 카탈로그 저장소 인터페이스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 로딩 방식(CSV, ScriptableObject 등)을 외부에서 교체 가능 (DIP)
    ///   - GameBootstrap 에서 ServiceLocator 에 등록됩니다.
    /// </summary>
    public interface IItemCatalogRepository
    {
        /// <summary>
        /// ItemId 로 카탈로그 항목을 조회합니다.
        /// </summary>
        /// <returns>항목이 없으면 null</returns>
        ItemCatalogEntry GetEntry(string itemId);

        /// <summary>
        /// ResourceType 으로 카탈로그 항목을 조회합니다.
        /// 드롭 결과(ResourceDrop)를 아이템으로 변환할 때 사용합니다.
        /// </summary>
        /// <returns>항목이 없으면 null</returns>
        ItemCatalogEntry GetEntryByResourceType(ResourceType resourceType);

        /// <summary>등록된 모든 카탈로그 항목을 반환합니다.</summary>
        IReadOnlyList<ItemCatalogEntry> GetAll();
    }
}
