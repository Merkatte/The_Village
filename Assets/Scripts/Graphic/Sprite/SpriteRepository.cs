using System.Collections.Generic;
using Game.Core.Interfaces;
using UnityEngine;

namespace Game.Graphic.Sprite
{
    /// <summary>
    /// ISpriteRepository 구현체입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - Bootstrap 에서 생성 시 Dictionary 를 주입받아 런타임 중 Resources.Load 를 하지 않습니다.
    ///   - 조회 실패 시 null 을 반환합니다. (예외 미사용 — UI 가 fallback 스프라이트를 처리)
    /// </summary>
    public sealed class SpriteRepository : ISpriteRepository
    {
        private readonly Dictionary<string, UnityEngine.Sprite> _sprites;

        public SpriteRepository(Dictionary<string, UnityEngine.Sprite> sprites)
        {
            _sprites = sprites;
        }

        public UnityEngine.Sprite GetSprite(string itemId)
        {
            _sprites.TryGetValue(itemId, out var sprite);
            return sprite;
        }
    }
}
