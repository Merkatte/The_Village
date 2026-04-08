 using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Enums;
using Game.UI.Core;

namespace Game.Data.Config
{
    /// <summary>
    /// UI 팝업 단축키 항목 하나를 나타내는 직렬화 가능한 데이터 클래스입니다.
    ///
    /// ─ 필드 설명 ──────────────────────────────────────────────────
    ///   PopupType       : 이 항목이 매핑되는 팝업 종류
    ///   Bindings        : 팝업을 여닫는 키 바인딩 목록 (예: "i", "Tab")
    ///   AllowedContexts : 이 단축키가 동작하는 입력 컨텍스트 목록
    /// </summary>
    [Serializable]
    public sealed class UIShortcutEntry
    {
        [SerializeField, Tooltip("단축키가 매핑되는 팝업 종류")]
        private PopupType _popupType;

        [SerializeField, Tooltip("팝업을 여닫는 키 바인딩 목록 (예: \"i\", \"Tab\")")]
        private List<string> _bindings = new();

        [SerializeField, Tooltip("이 단축키가 동작하는 입력 컨텍스트 목록")]
        private List<InputContext> _allowedContexts = new();

        /// <summary>단축키가 매핑되는 팝업 종류</summary>
        public PopupType PopupType => _popupType;

        /// <summary>팝업을 여닫는 키 바인딩 목록</summary>
        public IReadOnlyList<string> Bindings => _bindings;

        /// <summary>이 단축키가 동작하는 입력 컨텍스트 목록</summary>
        public IReadOnlyList<InputContext> AllowedContexts => _allowedContexts;
    }

    /// <summary>
    /// UI 팝업 단축키 전체 설정을 담는 ScriptableObject 입니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────────
    ///   Entries : UIShortcutEntry 목록. 팝업별 키 바인딩과 허용 컨텍스트를 등록합니다.
    ///
    /// ─ 배치 위치 ──────────────────────────────────────────────────
    ///   Assets/Resources/Data/ 또는 Assets/Data/ 에 에셋으로 저장합니다.
    ///   InputManager 또는 UIManager Initializer 의 [SerializeField] 에 연결합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "UIShortcutConfig", menuName = "Game/Config/UI Shortcut Config")]
    public sealed class UIShortcutConfig : ScriptableObject
    {
        [SerializeField, Tooltip("팝업별 단축키 항목 목록")]
        private List<UIShortcutEntry> _entries = new();

        /// <summary>팝업별 단축키 항목 목록</summary>
        public IReadOnlyList<UIShortcutEntry> Entries => _entries;
    }
}
