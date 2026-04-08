using UnityEngine;

namespace Game.Core.Interfaces
{
    /// <summary>
    /// 아이템 스프라이트 저장소 인터페이스입니다.
    ///
    /// ─ 조회 규칙 ──────────────────────────────────────────────
    ///   ItemId 를 키로 Sprite 를 반환합니다.
    ///   스프라이트 파일은 Resources/Sprites/Items/{itemId} 명명 규칙을 따릅니다.
    ///
    /// ─ 등록 위치 ──────────────────────────────────────────────
    ///   GameBootstrap.RegisterServices() 에서 ServiceLocator 에 등록됩니다.
    /// </summary>
    public interface ISpriteRepository
    {
        /// <summary>
        /// ItemId 에 대응하는 Sprite 를 반환합니다.
        /// </summary>
        /// <returns>스프라이트가 없으면 null</returns>
        Sprite GetSprite(string itemId);
    }
}
