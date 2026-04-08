using Game.Core.Infrastructure;
using Game.UI.Core;
using UnityEngine;

namespace Game.Town
{
    /// <summary>
    /// 플레이어가 발판에 진입하면 던전 입장 확인 팝업을 여는 MonoBehaviour 입니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   _playerTag : 플레이어 오브젝트의 Tag (기본값 "Player")
    ///
    /// ─ 배치 ───────────────────────────────────────────────────
    ///   발판 GameObject 에 Collider2D (IsTrigger = true) 와 함께 부착합니다.
    /// </summary>
    public sealed class DungeonEntryTrigger : MonoBehaviour
    {
        [SerializeField] private string _playerTag = "Player";

        private IUIManager _uiManager;

        private void Awake()
        {
            _uiManager = ServiceLocator.Get<IUIManager>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(_playerTag)) return;
            if (_uiManager.IsOpen(PopupType.DungeonEntry)) return;

            _uiManager.Open(PopupType.DungeonEntry);
        }
    }
}
