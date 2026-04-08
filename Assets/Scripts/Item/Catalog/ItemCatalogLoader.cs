using System.Collections.Generic;
using UnityEngine;

namespace Game.Item.Catalog
{
    /// <summary>
    /// TextAsset 으로부터 IItemCatalogRepository 를 생성합니다.
    ///
    /// ─ 사용 방법 ──────────────────────────────────────────────
    ///   1. Assets/Resources/Data/ 에 ItemCatalog.csv 파일을 배치합니다.
    ///   2. GlobalCsvConfig 의 itemCatalogCsv 필드에 연결합니다.
    ///   3. GameBootstrap.RegisterServices() 에서:
    ///
    ///      var repo = ItemCatalogLoader.Load(globalCsvConfig.itemCatalogCsv);
    ///      ServiceLocator.Register&lt;IItemCatalogRepository&gt;(repo);
    /// </summary>
    public static class ItemCatalogLoader
    {
        public static IItemCatalogRepository Load(TextAsset csvAsset)
        {
            if (csvAsset == null)
            {
                Debug.LogWarning(
                    "[ItemCatalogLoader] csvAsset 이 null 입니다. " +
                    "GlobalCsvConfig 의 itemCatalogCsv 필드에 CSV 를 연결했는지 확인하세요.");

                return new ItemCatalogRepository(new List<ItemCatalogEntry>());
            }

            var parser = new CsvItemCatalogParser();
            parser.OnWarning = (line, msg)
                => Debug.LogWarning($"[ItemCatalog CSV] 행 {line}: {msg}");

            var entries = parser.Parse(csvAsset.text);
            Debug.Log($"[ItemCatalogLoader] 로드 완료 — {entries.Count}개 아이템 등록.");

            return new ItemCatalogRepository(entries);
        }
    }
}
