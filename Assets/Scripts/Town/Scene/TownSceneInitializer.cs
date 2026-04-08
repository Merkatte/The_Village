using Game.Core.Infrastructure;
using Game.Core.Interfaces;
using Game.Data.Config;
using Game.Item.Catalog;
using Game.Inventory;
using Game.Shop;
using Game.UI.Core;
using UnityEngine;

namespace Game.Town.Scene
{
    /// <summary>
    /// 마을 씬 진입 시 씬 전용 서비스를 등록하는 MonoBehaviour 입니다.
    ///
    /// ─ 등록 서비스 ────────────────────────────────────────────
    ///   - IShopService (ShopService)
    ///
    /// ─ 초기화 순서 ────────────────────────────────────────────
    ///   1. IShopService 생성 및 ServiceLocator 등록
    ///   2. UIManager 에 popupCanvas 주입 (SetCanvas)
    ///   3. 마을 전용 팝업 등록 (townPopupConfig)
    ///
    /// ─ 씬 종료 시 ─────────────────────────────────────────────
    ///   OnDestroy 에서 마을 팝업 해제 → IShopService 해제
    ///
    /// ─ 인스펙터 설정 ──────────────────────────────────────────
    ///   popupCanvas    : 팝업을 배치할 Canvas (씬 전용)
    ///   shopConfig     : 상점 품목·가격 비율 설정 ScriptableObject
    ///   townPopupConfig: 마을 씬 전용 팝업 프리팹 매핑 ScriptableObject
    /// </summary>
    public sealed class TownSceneInitializer : MonoBehaviour
    {
        [SerializeField] private Canvas        popupCanvas;
        [SerializeField] private ShopConfig    shopConfig;
        [SerializeField] private UIPopupConfig townPopupConfig;

        private void Awake()
        {
            // ─ Step 1. IShopService 등록 ──────────────────────────
            var shopService = new ShopService(
                ServiceLocator.Get<IItemCatalogRepository>(),
                ServiceLocator.Get<ICurrencyService>(),
                ServiceLocator.Get<IInventory>(),
                shopConfig);

            ServiceLocator.Register<IShopService>(shopService);

            // ─ Step 2. UIManager 캔버스 주입 ──────────────────────
            ServiceLocator.Get<IUIManager>().SetCanvas(popupCanvas);

            // ─ Step 3. 마을 전용 팝업 등록 ────────────────────────
            if (townPopupConfig != null)
                ServiceLocator.Get<IUIManager>().RegisterPopups(townPopupConfig);

            Debug.Log("[TownSceneInitializer] 마을 씬 초기화 완료.");
        }

        private void OnDestroy()
        {
            if (townPopupConfig != null)
                ServiceLocator.Get<IUIManager>().UnregisterPopups(townPopupConfig);

            if (ServiceLocator.IsRegistered<IShopService>())
                ServiceLocator.Unregister<IShopService>();
        }
    }
}
