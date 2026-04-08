using System.Collections.Generic;
using Game.Core.Enums;
using Game.Core.Infrastructure;
using Game.Manager.Input;
using Game.Manager.Interfaces.Input;
using UnityEngine;

namespace Game.UI.Core
{
    /// <summary>
    /// 팝업 UI 생명주기를 전담하는 MonoBehaviour 입니다.
    ///
    /// ─ 등록 서비스 ────────────────────────────────────────────
    ///   GameBootstrap → ServiceLocator.Register&lt;IUIManager&gt;(this)
    ///
    /// ─ 초기화 순서 ────────────────────────────────────────────
    ///   GameBootstrap.RegisterServices() 에서 inputManager.Init() 이후에
    ///   Init() 을 호출해야 합니다.
    ///   (IUIInputReader 가 ServiceLocator 에 등록된 이후)
    ///
    /// ─ 팝업 열기/닫기 흐름 ────────────────────────────────────
    ///   Open()  → Instantiate → OnOpen() → InputContext.UI 전환
    ///   Close() → OnClose()  → Destroy  → 팝업 없으면 InputContext.Gameplay 복귀
    ///   ESC     → 가장 마지막에 열린 팝업 닫기 (스택 방식)
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   Root Canvas    : 팝업을 배치할 부모 Canvas
    ///   Input Manager  : 팝업 열림/닫힘 시 InputContext 전환에 사용
    /// </summary>
    public sealed class UIManager : MonoBehaviour, IUIManager
    {
        private Canvas _popupCanvas;
        private InputManager _inputManager;

        private readonly Dictionary<PopupType, GameObject> _prefabs    = new();
        private readonly Dictionary<PopupType, PopupBase>  _openPopups = new();
        private readonly List<PopupType>                   _openOrder  = new();

        private IUIInputReader _uiInputReader;

        /// <summary>
        /// UIManager 를 초기화합니다.
        /// GameBootstrap 에서 inputManager.Init() 이후에 호출하세요.
        /// </summary>
        public void Init()
        {
            _inputManager  = ServiceLocator.Get<InputManager>();
            _uiInputReader = ServiceLocator.Get<IUIInputReader>();

            _uiInputReader.OnCancelPerformed += CloseTopPopup;
        }

        /// <inheritdoc/>
        public void SetCanvas(Canvas popupCanvas)
        {
            _popupCanvas = popupCanvas;
        }

        /// <inheritdoc/>
        public void RegisterPopups(UIPopupConfig config)
        {
            foreach (var entry in config.Entries)
                _prefabs[entry.PopupType] = entry.Prefab;
        }

        /// <inheritdoc/>
        public void UnregisterPopups(UIPopupConfig config)
        {
            foreach (var entry in config.Entries)
            {
                Close(entry.PopupType);
                _prefabs.Remove(entry.PopupType);
            }
        }

        /// <inheritdoc/>
        public void Open(PopupType type)
        {
            if (_openPopups.ContainsKey(type)) return;

            if (!_prefabs.TryGetValue(type, out var prefab))
            {
                Debug.LogWarning($"[UIManager] {type} 에 등록된 프리팹이 없습니다.");
                return;
            }

            if (_popupCanvas == null)
            {
                Debug.LogWarning("[UIManager] popupCanvas 가 설정되지 않았습니다. SetCanvas() 를 먼저 호출하세요.");
                return;
            }

            var go    = Instantiate(prefab, _popupCanvas.transform);
            var popup = go.GetComponent<PopupBase>();

            if (popup == null)
            {
                Debug.LogWarning($"[UIManager] {type} 프리팹에 PopupBase 컴포넌트가 없습니다.");
                Destroy(go);
                return;
            }

            popup.OnOpen();
            _openPopups[type] = popup;
            _openOrder.Add(type);

            _inputManager.SwitchContext(InputContext.UI);
        }

        /// <inheritdoc/>
        public void Close(PopupType type)
        {
            if (!_openPopups.TryGetValue(type, out var popup)) return;

            popup.OnClose();
            Destroy(popup.gameObject);

            _openPopups.Remove(type);
            _openOrder.Remove(type);

            if (_openPopups.Count == 0)
                _inputManager.SwitchContext(InputContext.Gameplay);
        }

        /// <inheritdoc/>
        public bool IsOpen(PopupType type) => _openPopups.ContainsKey(type);

        // ─ 내부 ──────────────────────────────────────────────────

        /// <summary>
        /// 가장 마지막에 열린 팝업을 닫습니다. (ESC 키 동작)
        /// </summary>
        private void CloseTopPopup()
        {
            if (_openOrder.Count == 0) return;
            var type = _openOrder[_openOrder.Count - 1];
            if (!_openPopups[type].IsEscClosable) return;
            Close(type);
        }

        private void OnDestroy()
        {
            if (_uiInputReader != null)
                _uiInputReader.OnCancelPerformed -= CloseTopPopup;
        }
    }
}
