# The Village - 아키텍처 인덱스

> 최종 업데이트: 2026-04-02
> 총 파일 수: 164개 (구현 129개 / 테스트 27개 / 에디터 유틸 8개)

---

## 전체 구조 개요

```
Assets/Scripts/
├── Camera/             카메라 팔로우 시스템
├── Core/               공통 인프라 (Enum, Interface, DI, CSV)
├── Data/               ScriptableObject 기반 설정 데이터
├── Dungeon/            던전 자원 채취 · 스폰 시스템
├── Editor/             에디터 전용 유틸리티
├── Enemy/              적 AI 시스템 (상태 머신 + UniTask 공격)
├── Graphic/            그래픽 리소스 관리 (스프라이트 로딩·저장소)
├── Inventory/          슬롯 기반 인벤토리 (IStackable 자동 합산)
├── Item/               아이템 데이터 모델 + 카탈로그
├── Loading/            씬 페이드 · 로딩 시퀀스
├── Manager/            게임 진입점, 입력, 씬 전환, 상태 관리
├── Player/             플레이어 이동 · 상호작용
└── Tests/              편집 모드 단위 테스트
```

---

## 모듈별 상세 파일 목록

| 파일 | 포함 모듈 |
|------|-----------|
| `architecture_core_manager.md` | Core (Enum·Interface·Infrastructure) · Manager · Currency |
| `architecture_gameplay.md` | Player · Camera · Loading · Dungeon · Graphic · Data/Config |
| `architecture_enemy.md` | Enemy (Core·Movement·Health·Combat·Data) |
| `architecture_item_shop.md` | Item · Inventory · Shop |
| `architecture_ui.md` | UI (Core·Slot·Inventory·DungeonEntry·Sell·Shop) · Editor · Tests |

---

## 의존성 흐름 요약

```
GameBootstrap
  └─ ServiceLocator
       ├─ ItemCatalogRepository   (IItemCatalogRepository)  ← ItemCatalog.csv
       ├─ SpriteRepository        (ISpriteRepository)       ← Resources/Sprites/Items/{itemId}
       ├─ GameStateManager        (IGameStateService)
       ├─ InputManager            (PlayerInputReader / UIInputReader)
       ├─ SceneTransitionManager  (ISceneTransitionService)
       │    ├─ UnitySceneLoader   (ISceneLoader)
       │    └─ SceneFader         (ISceneFader)
       ├─ Inventory               (IInventory)
       ├─ CurrencyManager         (ICurrencyService)
       ├─ UIShortcutHandler       ← UIShortcutConfig, IUIManager, InputManager.CurrentContext
       └─ [던전 씬 진입 시] DungeonSceneInitializer
            ├─ DropTableRepository   (IDropTableRepository)
            ├─ SpawnPointRepository  (ISpawnPointRepository)
            │    └─ HarvestableSpawner → Harvestable (채취 오브젝트 배치)
            └─ EnemyLoader.LoadFromText(enemyText)
                 └─ IEnemyRepository  → ServiceLocator 등록
                      └─ EnemySpawner.InitAsync()
                           ├─ EnemySpawnConfig
                           └─ EnemyPrefabConfig
                                └─ Instantiate(prefab) → EnemyController
                                     ├─ EnemyBrain
                                     ├─ EnemyMover
                                     ├─ EnemyHealth  (IDamageable)
                                     └─ EnemyAttacker
                                          └─ MeleeAttackStrategy
                                               └─ PlayerHealth.TakeDamage()

TownSceneInitializer
  └─ ShopService  (IShopService)
       ├─ IItemCatalogRepository
       ├─ ICurrencyService
       ├─ IInventory
       └─ ShopConfig

Player (PlayerInitializer — 컴포지션 루트)
  ├─ PlayerController   ← PlayerInputReader 구독
  ├─ PlayerEquipment    ← 장착 도구 보관
  ├─ HarvestController  ← Init(InteractionDetector, PlayerEquipment)
  └─ [자식] InteractionDetector  ← OnTriggerEnter/Exit2D 기반 감지

CameraController
  └─ CameraFollower     (팔로우 계산)
```

---

## 미구현 / 진행 중

| 시스템 | 상태 |
|--------|------|
| 인벤토리 UI 프리팹 | 스크립트 완료 / 유니티 프리팹 설정 필요 |
| 상점 UI 프리팹 | 스크립트 완료 / 유니티 프리팹 설정 필요 |
| ShopConfig 에셋 | ScriptableObject 생성 및 품목 입력 필요 |
| 판매 UI 프리팹 | 스크립트 완료 / 유니티 프리팹 설정 필요 |
| 적 AI Unity Editor 설정 | EnemyCsvConfig·EnemyPrefabConfig·EnemySpawnConfig 에셋 생성 및 연결 필요 |
| 적 AI Phase 6 — 원거리 확장 | `RangedAttackStrategy` (별도 세션) |
