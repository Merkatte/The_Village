using System.Collections.Generic;
using UnityEngine;

namespace Game.Data.Config
{
    /// <summary>
    /// 상점 설정 ScriptableObject 입니다.
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   ItemIds          : 상점이 판매할 아이템 ID 목록 (ItemCatalog.csv 의 ItemId 와 일치해야 함)
    ///   BuyPriceRatio    : 구입가 = Value × 이 비율 (기본 1.0)
    ///   SellPriceRatio   : 판매가 = Value × 이 비율 (기본 0.5)
    ///
    /// ─ 배치 위치 ──────────────────────────────────────────────
    ///   Assets/Resources/Data/ 또는 Assets/Data/ 에 에셋으로 저장합니다.
    ///   TownSceneInitializer 의 [SerializeField] shopConfig 에 연결합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ShopConfig", menuName = "Game/Shop Config")]
    public sealed class ShopConfig : ScriptableObject
    {
        [SerializeField, Tooltip("상점이 판매하는 아이템 ID 목록 (ItemCatalog.csv 의 ItemId 기준)")]
        private List<string> _itemIds = new();

        [SerializeField, Range(0f, 10f), Tooltip("구입가 배율. 구입가 = Value × BuyPriceRatio")]
        private float _buyPriceRatio = 1.0f;

        [SerializeField, Range(0f, 1f), Tooltip("판매가 배율. 판매가 = Value × SellPriceRatio")]
        private float _sellPriceRatio = 0.5f;

        /// <summary>상점이 판매하는 아이템 ID 목록</summary>
        public IReadOnlyList<string> ItemIds => _itemIds;

        /// <summary>구입가 배율 (Value × BuyPriceRatio)</summary>
        public float BuyPriceRatio => _buyPriceRatio;

        /// <summary>판매가 배율 (Value × SellPriceRatio)</summary>
        public float SellPriceRatio => _sellPriceRatio;
    }
}
