using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Enums;
using Game.Data.Config;
using UnityEngine.InputSystem;

namespace Game.UI.Core
{
    /// <summary>
    /// UIShortcutConfig 에 등록된 단축키 항목마다 InputAction 을 생성하고,
    /// 현재 InputContext 에 따라 팝업 열기/닫기를 처리하는 클래스입니다.
    ///
    /// ─ 생명주기 ──────────────────────────────────────────────────
    ///   생성자  : 각 Entry 에 대해 InputAction 을 생성·바인딩·활성화합니다.
    ///   Dispose : 모든 InputAction 을 비활성화·해제하고 바인딩 목록을 초기화합니다.
    ///
    /// ─ 토글 규칙 ─────────────────────────────────────────────────
    ///   performed 콜백 시점에 contextProvider() 가 반환한 InputContext 가
    ///   Entry.AllowedContexts 에 포함되어 있을 때만 토글이 동작합니다.
    ///   IsOpen == true  → Close 호출
    ///   IsOpen == false → Open 호출
    /// </summary>
    public sealed class UIShortcutHandler : IDisposable
    {
        // ─── Fields ───────────────────────────────────────────────────────────

        private readonly IUIManager _uiManager;
        private readonly Func<InputContext> _contextProvider;

        /// <summary>Entry 당 InputAction 목록. PopupType 과 1:1 대응합니다.</summary>
        private readonly List<(InputAction action, UIShortcutEntry entry)> _bindings = new();

        // ─── Constructor ──────────────────────────────────────────────────────

        /// <summary>
        /// UIShortcutHandler 를 초기화하고 모든 단축키 InputAction 을 활성화합니다.
        /// </summary>
        /// <param name="config">단축키 설정 ScriptableObject.</param>
        /// <param name="uiManager">팝업 열기/닫기 서비스.</param>
        /// <param name="contextProvider">현재 InputContext 를 반환하는 공급자.</param>
        public UIShortcutHandler(UIShortcutConfig config, IUIManager uiManager, Func<InputContext> contextProvider)
        {
            _uiManager       = uiManager;
            _contextProvider = contextProvider;

            foreach (var entry in config.Entries)
                RegisterEntry(entry);
        }

        // ─── Private Methods ──────────────────────────────────────────────────

        /// <summary>
        /// Entry 하나에 대해 InputAction 을 생성하고 바인딩·활성화합니다.
        /// </summary>
        private void RegisterEntry(UIShortcutEntry entry)
        {
            var action = new InputAction(name: $"Shortcut_{entry.PopupType}", type: InputActionType.Button);
            foreach (var binding in entry.Bindings)
                action.AddBinding(binding);
            action.performed += _ => OnActionPerformed(entry);
            action.Enable();
            _bindings.Add((action, entry));
        }

        /// <summary>
        /// 단축키가 입력되었을 때 컨텍스트를 확인하고 팝업을 열거나 닫습니다.
        /// </summary>
        private void OnActionPerformed(UIShortcutEntry entry)
        {
            var current = _contextProvider();
            if (!entry.AllowedContexts.Contains(current)) return;

            if (_uiManager.IsOpen(entry.PopupType))
                _uiManager.Close(entry.PopupType);
            else
                _uiManager.Open(entry.PopupType);
        }

        // ─── IDisposable ──────────────────────────────────────────────────────

        /// <summary>
        /// 모든 InputAction 을 비활성화·해제하고 바인딩 목록을 초기화합니다.
        /// </summary>
        public void Dispose()
        {
            foreach (var (action, _) in _bindings)
            {
                action.Disable();
                action.Dispose();
            }
            _bindings.Clear();
        }
    }
}
