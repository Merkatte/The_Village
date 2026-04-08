using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using UnityEngine;

namespace Game.Dungeon
{
    /// <summary>
    /// 플레이어가 발판에 진입하면 즉시 마을 씬으로 복귀하는 MonoBehaviour 입니다.
    ///
    /// ─ 동작 규칙 ──────────────────────────────────────────────
    ///   - 확인 팝업 없이 로딩 씬 경유로 바로 Town 씬으로 전환합니다.
    ///   - 인벤토리·골드 등 ServiceLocator 에 등록된 데이터는 그대로 유지됩니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   _playerTag : 플레이어 오브젝트의 Tag (기본값 "Player")
    ///
    /// ─ 배치 ───────────────────────────────────────────────────
    ///   발판 GameObject 에 Collider2D (IsTrigger = true) 와 함께 부착합니다.
    /// </summary>
    public sealed class TownReturnTrigger : MonoBehaviour
    {
        [SerializeField] private string _playerTag = "Player";

        private ISceneTransitionService _sceneTransition;

        private void Awake()
        {
            _sceneTransition = ServiceLocator.Get<ISceneTransitionService>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(_playerTag)) return;

            _sceneTransition.TransitionToWithLoading(SceneType.Town);
        }
    }
}
