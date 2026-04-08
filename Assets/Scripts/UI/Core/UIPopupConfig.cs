using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.Core
{
    /// <summary>
    /// PopupType 별 프리팹을 보관하는 ScriptableObject 입니다.
    ///
    /// ─ 배치 방법 ──────────────────────────────────────────────
    ///   Assets/Data/Config/ 에 에셋을 생성한 뒤,
    ///   GameBootstrap 또는 SceneInitializer 의 [SerializeField] 에 연결합니다.
    ///   (메뉴: Assets > Create > Game/Config/UI Popup Config)
    ///
    /// ─ 용도별 분리 ────────────────────────────────────────────
    ///   UICommonPopupConfig  — 공통 팝업 (Inventory 등, GameBootstrap 에서 등록)
    ///   UITownPopupConfig    — 타운 전용 (Shop 등, TownSceneInitializer 에서 등록/해제)
    ///   UIDungeonPopupConfig — 던전 전용 (DungeonSceneInitializer 에서 등록/해제)
    /// </summary>
    [CreateAssetMenu(
        menuName = "Game/Config/UI Popup Config",
        fileName = "UIPopupConfig")]
    public sealed class UIPopupConfig : ScriptableObject
    {
        [SerializeField] private List<UIPopupEntry> _entries = new();

        /// <summary>등록된 팝업 항목 목록.</summary>
        public IReadOnlyList<UIPopupEntry> Entries => _entries;
    }

    [Serializable]
    public sealed class UIPopupEntry
    {
        public PopupType  PopupType;
        public GameObject Prefab;
    }
}
