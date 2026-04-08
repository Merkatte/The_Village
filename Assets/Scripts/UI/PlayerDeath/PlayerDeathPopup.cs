using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using Game.UI.Core;

namespace Game.UI.PlayerDeath
{
    /// <summary>
    /// 플레이어 사망 시 표시되는 팝업입니다.
    /// ESC 키로 닫을 수 없으며, 버튼을 통해서만 닫힙니다.
    /// </summary>
    public sealed class PlayerDeathPopup : PopupBase
    {
        /// <inheritdoc/>
        public override bool IsEscClosable => false;

        private ISceneTransitionService _sceneTransition;

        /// <inheritdoc/>
        public override void OnOpen()
        {
            _sceneTransition = ServiceLocator.Get<ISceneTransitionService>();
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            _sceneTransition = null;
        }

        /// <summary>
        /// "마을로 돌아가기" 버튼 클릭 시 호출합니다.
        /// </summary>
        public void OnReturnToTownClicked()
        {
            _sceneTransition.TransitionToWithLoading(SceneType.Town);
        }
    }
}
