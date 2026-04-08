using System.Collections.Generic;
using Game.Item.Catalog;
using UnityEngine;

namespace Game.Graphic.Sprite
{
    /// <summary>
    /// IItemCatalogRepository 의 모든 ItemId 를 순회하여 스프라이트를 로드합니다.
    ///
    /// ─ 명명 규칙 ──────────────────────────────────────────────
    ///   Resources/Sprites/Items/{itemId}
    ///   ex) ItemId = "iron_ore" → Resources/Sprites/Items/iron_ore.png
    ///
    /// ─ 누락 처리 ──────────────────────────────────────────────
    ///   스프라이트 파일이 없으면 Dictionary 에 추가하지 않고 경고 로그만 출력합니다.
    ///   GetSprite() 는 null 을 반환하므로 UI 에서 fallback 처리를 합니다.
    /// </summary>
    public static class SpriteLoader
    {
        private const string BasePath = "Sprites/Items/";

        public static SpriteRepository Load(IItemCatalogRepository catalog)
        {
            var sprites = new Dictionary<string, UnityEngine.Sprite>();

            foreach (var entry in catalog.GetAll())
            {
                var sprite = Resources.Load<UnityEngine.Sprite>(BasePath + entry.ItemId);

                if (sprite == null)
                {
                    Debug.LogWarning($"[SpriteLoader] 스프라이트 없음: {BasePath}{entry.ItemId}");
                    continue;
                }

                sprites[entry.ItemId] = sprite;
            }

            return new SpriteRepository(sprites);
        }
    }
}
