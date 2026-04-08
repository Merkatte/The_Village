using UnityEngine;

namespace Game.UI.Core
{
    /// <summary>
    /// 팝업 UI 생명주기를 관리하는 서비스 인터페이스입니다.
    ///
    /// ─ 등록 흐름 ──────────────────────────────────────────────
    ///   GameBootstrap         → RegisterPopups(commonConfig)   공통 팝업
    ///   TownSceneInitializer  → RegisterPopups(townConfig)     타운 전용
    ///   DungeonSceneInitializer → RegisterPopups(dungeonConfig) 던전 전용
    ///
    ///   씬 종료 시 SceneInitializer.OnDestroy() 에서 UnregisterPopups 를 호출하여
    ///   씬 전용 팝업 프리팹 참조를 정리합니다.
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// 팝업을 인스턴스화할 부모 Canvas 를 설정합니다.
        /// SceneInitializer 에서 씬 진입 시 호출하세요.
        /// </summary>
        void SetCanvas(Canvas popupCanvas);

        /// <summary>
        /// ScriptableObject 설정에 등록된 팝업 프리팹을 UIManager 에 추가합니다.
        /// </summary>
        void RegisterPopups(UIPopupConfig config);

        /// <summary>
        /// ScriptableObject 설정에 등록된 팝업 프리팹을 UIManager 에서 제거합니다.
        /// 열려 있는 팝업은 닫은 뒤 제거됩니다.
        /// </summary>
        void UnregisterPopups(UIPopupConfig config);

        /// <summary>
        /// 지정된 팝업을 엽니다. 이미 열려 있으면 무시합니다.
        /// </summary>
        void Open(PopupType type);

        /// <summary>
        /// 지정된 팝업을 닫습니다. 열려 있지 않으면 무시합니다.
        /// </summary>
        void Close(PopupType type);

        /// <summary>
        /// 해당 팝업이 현재 열려 있는지 반환합니다.
        /// </summary>
        bool IsOpen(PopupType type);
    }
}
