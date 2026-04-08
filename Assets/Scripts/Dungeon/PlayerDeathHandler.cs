using Game.Core.Infrastructure;
using Game.Player;
using Game.UI.Core;
using UnityEngine;

namespace Game.Dungeon
{
    /// <summary>
    /// 플레이어 사망 이벤트를 수신하여 사망 팝업을 여는 MonoBehaviour 브릿지입니다.
    /// 던전 씬 오브젝트에 부착합니다.
    /// </summary>
    public sealed class PlayerDeathHandler : MonoBehaviour
    {
        private IUIManager _uiManager;
        private PlayerHealth _playerHealth;

        private void Awake()
        {
            _uiManager = ServiceLocator.Get<IUIManager>();
        }

        private void Start()
        {
            _playerHealth = ServiceLocator.Get<PlayerHealth>();
            _playerHealth.OnDied += HandlePlayerDied;
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.OnDied -= HandlePlayerDied;
        }

        private void HandlePlayerDied()
        {
            _uiManager.Open(PopupType.PlayerDeath);
        }
    }
}
