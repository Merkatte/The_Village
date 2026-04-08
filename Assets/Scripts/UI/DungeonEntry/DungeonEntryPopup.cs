using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using Game.UI.Core;

namespace Game.UI.DungeonEntry
{
    /// <summary>
    /// 던전 입장 확인 팝업입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 예 버튼 : 로딩 씬 경유 → 던전 씬 전환 후 팝업 닫기
    ///   - 아니오 버튼 : 팝업만 닫기
    ///
    /// ─ 확장 포인트 ────────────────────────────────────────────
    ///   - 보험 가입, 장비 확인 등 입장 전 추가 기능은 이 클래스에 구현합니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   - 예 버튼 onClick  → OnYesClicked()
    ///   - 아니오 버튼 onClick → OnNoClicked()
    /// </summary>
    public sealed class DungeonEntryPopup : PopupBase
    {
        private IUIManager              _uiManager;
        private ISceneTransitionService _sceneTransition;

        /// <inheritdoc/>
        public override void OnOpen()
        {
            _uiManager       = ServiceLocator.Get<IUIManager>();
            _sceneTransition = ServiceLocator.Get<ISceneTransitionService>();
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            _uiManager       = null;
            _sceneTransition = null;
        }

        /// <summary>
        /// 예 버튼 onClick 에 연결합니다.
        /// </summary>
        public void OnYesClicked()
        {
            _sceneTransition?.TransitionToWithLoading(SceneType.Dungeon);
            _uiManager?.Close(PopupType.DungeonEntry);
        }

        /// <summary>
        /// 아니오 버튼 onClick 에 연결합니다.
        /// </summary>
        public void OnNoClicked()
        {
            _uiManager?.Close(PopupType.DungeonEntry);
        }
    }
}
