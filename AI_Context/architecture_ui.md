# 아키텍처 — UI · Editor · Tests

> 최종 업데이트: 2026-04-07

---

## UI — 팝업 UI 시스템

### Core
| 파일 | 설명 |
|------|------|
| `UI/Core/PopupType.cs` | 팝업 종류 Enum (`Inventory`, `Shop`, `Sell`, `DungeonEntry`, `PlayerDeath`) |
| `UI/Core/PopupBase.cs` | 모든 팝업의 기반 abstract MonoBehaviour. `OnOpen()` / `OnClose()` 훅 + `IsEscClosable` 프로퍼티 제공 (기본 true) |
| `UI/Core/IUIManager.cs` | 팝업 생명주기 관리 인터페이스. `SetCanvas` / `RegisterPopups` / `UnregisterPopups` / `Open` / `Close` / `IsOpen` |
| `UI/Core/UIPopupConfig.cs` | ScriptableObject. `PopupType → Prefab` 매핑 목록 |
| `UI/Core/UIManager.cs` | `IUIManager` 구현체. Instantiate/Destroy 전담. ESC 닫기 처리 시 `IsEscClosable` 체크. `Init()` 에서 `IUIInputReader.OnCancelPerformed` 구독 |
| `UI/Core/UIShortcutHandler.cs` | 순수 C#. `UIShortcutConfig` 기반 팝업별 `InputAction` 동적 생성. 항상 활성 + `AllowedContexts` 컨텍스트 필터링. `IDisposable` |

> 팝업은 자체 Canvas 를 포함하지 않습니다. 씬의 SceneInitializer 가 `popupCanvas` 를 보유하고 씬 진입 시 `SetCanvas(popupCanvas)` 로 UIManager 에 주입합니다.
> 공통 팝업은 GameBootstrap 에서 `commonPopupConfig` 로 등록합니다.
> 씬 전용 팝업은 SceneInitializer 에서 등록/해제합니다.

### Slot (공통 기반)
| 파일 | 설명 |
|------|------|
| `UI/Slot/BaseSlotUI.cs` | 슬롯 공통 기반 abstract MonoBehaviour. `_iconImage` · `_quantityText` 필드 + `SetData(SlotViewModel)` 공유 |

### Inventory UI
| 파일 | 설명 |
|------|------|
| `UI/Inventory/SlotViewModel.cs` | 슬롯 표시 데이터 (Icon, ItemName, QuantityText). 불변 순수 C# |
| `UI/Inventory/IInventoryView.cs` | 인벤토리 View 인터페이스 (`Refresh(SlotViewModel[])`) |
| `UI/Inventory/InventoryPresenter.cs` | 순수 C#. `IInventory.OnInventoryChanged` 구독 → SlotViewModel 빌드 → IInventoryView 갱신. IDisposable |
| `UI/Inventory/InventoryUI.cs` | `PopupBase` + `IInventoryView` 구현 MonoBehaviour. 자체 Canvas 포함 프리팹 루트 |
| `UI/Inventory/InventorySlotUI.cs` | `BaseSlotUI` 상속. 드래그 앤 드롭(IBeginDragHandler 등) 구현 |

### DungeonEntry UI
| 파일 | 설명 |
|------|------|
| `UI/DungeonEntry/DungeonEntryPopup.cs` | 던전 입장 확인 팝업. `PopupBase` 상속. 예→`TransitionToWithLoading(Dungeon)`, 아니오→팝업 닫기. 향후 보험 등 확장 포인트 |

### PlayerDeath UI
| 파일 | 설명 |
|------|------|
| `UI/PlayerDeath/PlayerDeathPopup.cs` | 플레이어 사망 팝업. `PopupBase` 상속. `IsEscClosable = false` (ESC 차단). "마을로 돌아가기" 버튼 → `TransitionToWithLoading(Town)` |

### Sell UI
| 파일 | 설명 |
|------|------|
| `UI/Sell/ISellView.cs` | 판매 팝업 View 인터페이스. `Refresh(SlotViewModel[], IReadOnlyCollection<int>)` / `RefreshGold` / `RefreshSellTotal` |
| `UI/Sell/SellPresenter.cs` | 순수 C#. `HashSet<int>` 로 선택 상태 관리. `ToggleSlot` / `OnSellConfirmed` (내림차순 정렬 후 판매). `IInventory.OnInventoryChanged` · `ICurrencyService.OnCurrencyChanged` 구독. `IDisposable` |
| `UI/Sell/SellUI.cs` | `PopupBase` + `ISellView` 구현 MonoBehaviour. `_slots[]` + `_goldText` + `_sellTotalText` Inspector 연결. `OnSellButtonClicked()` 을 Button.onClick 에 연결 |
| `UI/Sell/SellSlotUI.cs` | `BaseSlotUI` 상속. `IPointerClickHandler` 로 클릭 선택. `_selectionHighlight` Image 오버레이로 초록 강조. `SetSelected(bool)` |

### Shop UI
| 파일 | 설명 |
|------|------|
| `UI/Shop/ShopSlotViewModel.cs` | 슬롯 표시 불변 데이터 (`Icon`, `ItemName`, `PriceText`, `SlotIndex`, `IsEmpty`). `Empty` 싱글턴 제공 |
| `UI/Shop/IShopView.cs` | 상점 View 인터페이스. `RefreshBigSlots` / `RefreshSmallSlots` / `RefreshGold`. Big(최대 3개) / Small(최대 6개) Zone 분리 |
| `UI/Shop/ShopPresenter.cs` | 순수 C#. **구매 전용**. Big Zone(인덱스 0~2) + Small Zone(인덱스 3~8) 분리 빌드. `ISpriteRepository` 생성자 주입. `Bind(IShopView)` / `Unbind()` 패턴. `OnBuyRequested(int)` 으로 구매 처리. `ICurrencyService.OnCurrencyChanged` 구독으로 골드 자동 갱신. `IDisposable` |
| `UI/Shop/ShopUI.cs` | `PopupBase` + `IShopView` 구현 MonoBehaviour. `_bigSlots[]`(3) + `_smallSlots[]`(6) + `_goldText` Inspector 연결 |
| `UI/Shop/ShopSlotUI.cs` | 슬롯 1개. `Init(Action<int> onClicked)` 초기화. 클릭은 Button의 `OnPurchaseButtonClicked()` 콜백으로 처리 |

---

## Editor — 에디터 유틸리티

| 파일 | 설명 |
|------|------|
| `Editor/CreateTwoAssemblyDefinitions.cs` | `Game.asmdef` 및 `Game.Tests.asmdef` 자동 생성 메뉴 |
| `Editor/CreateSampleDropTableCsv.cs` | 샘플 `DropTable.csv` 에디터 메뉴로 생성 |
| `Editor/CreateSampleItemCatalogCsv.cs` | 샘플 `ItemCatalog.csv` 에디터 메뉴로 생성 |
| `Editor/CreateSampleSpawnPointsCsv.cs` | 샘플 `SpawnPoints.csv` 에디터 메뉴로 생성 |
| `Editor/CreateSampleEnemyDataCsv.cs` | 샘플 `EnemyData.csv` 에디터 메뉴로 생성 |
| `Editor/AddAsmdefReferences.cs` | Assembly Definition 간 참조 추가 |
| `Editor/RecreateGameAsmdef.cs` | `Game.asmdef` 재생성 |
| `Editor/RebuildTestAssemblyDefinition.cs` | 테스트 Assembly Definition 재구성 |

---

## Tests — 단위 테스트 (EditMode)

| 파일 | 테스트 대상 |
|------|------------|
| `Tests/EditMode/Camera/CameraFollowerTests.cs` | `CameraFollower` 범위 클램핑 검증 |
| `Tests/EditMode/Core/ServiceLocatorTests.cs` | `ServiceLocator` 등록·조회·제거 검증 |
| `Tests/EditMode/Core/CsvReaderTests.cs` | `CsvReader` 파싱 로직 검증 |
| `Tests/EditMode/Dungeon/ResourceDataTests.cs` | `ResourceDrop`, `DropEntry` 데이터 검증 |
| `Tests/EditMode/Dungeon/DropTableTests.cs` | `DropTableImpl.Roll(IRandom)` 확률 판정 + `FakeRandom` 검증 |
| `Tests/EditMode/Dungeon/CsvDropTableParserTests.cs` | CSV 파싱 및 `DropTableRepository.GetTable()` 검증 |
| `Tests/EditMode/Dungeon/SpawnPointTests.cs` | CSV 파싱(6열) 및 `SpawnPointRepository` 검증. `SpawnEntry.DropTableId` 검증 포함 |
| `Tests/EditMode/Item/EquipmentTests.cs` | `Weapon`, `Armor` 데이터 검증 |
| `Tests/EditMode/Item/ToolTests.cs` | `HarvestTool`, `FarmTool` 배율 검증 |
| `Tests/EditMode/Item/ItemFactoryTests.cs` | `ItemFactory` 타입별 생성 및 null 예외 검증 |
| `Tests/EditMode/Item/ItemCatalogRepositoryTests.cs` | `ItemCatalogRepository` 조회·대소문자·ResourceType 인덱스 검증 |
| `Tests/EditMode/Manager/InputManagerTests.cs` | `InputManager` 맥락 전환 및 이벤트 검증 |
| `Tests/EditMode/Manager/GameStateManagerTests.cs` | `GameStateManager` 상태 전환 검증 |
| `Tests/EditMode/Manager/SceneTransitionManagerTests.cs` | `SceneTransitionManager` 씬 전환 검증 |
| `Tests/EditMode/Manager/SceneTransitionManagerLoadingTests.cs` | 로딩 씬 전환 시나리오 검증 |
| `Tests/EditMode/Manager/CurrencyManagerTests.cs` | `CurrencyManager` Get·Add·TrySpend 정상/예외/이벤트 검증 |
| `Tests/EditMode/Player/PlayerMoverTests.cs` | `PlayerMover` 이동 벡터 계산 검증 |
| `Tests/EditMode/Inventory/InventoryTests.cs` | `Inventory` 슬롯 추가·제거·교환·스택 합산 검증 (총 18개) |
| `Tests/EditMode/Shop/ShopServiceTests.cs` | `ShopService` ShopItems·GetSellPrice·TryBuy·TrySell 검증 |
| `Tests/EditMode/UI/InventoryPresenterTests.cs` | `InventoryPresenter` Bind/Dispose·이벤트·ViewModel 내용 검증 |
| `Tests/EditMode/UI/SellPresenterTests.cs` | `SellPresenter` ToggleSlot·OnSellConfirmed·이벤트 구독 해제 검증 |
| `Tests/EditMode/UI/ShopPresenterTests.cs` | `ShopPresenter` Big/Small 슬롯 분리·OnBuyRequested·RefreshGold 검증 |
| `Tests/EditMode/Enemy/EnemyBrainTests.cs` | `EnemyBrain.Tick` 거리별 상태 전환·Dead 유지·경계값 검증 (8개) |
| `Tests/EditMode/Enemy/EnemyMoverTests.cs` | `EnemyMover` 이동 벡터 계산·SetPosition·음수 속도 처리 검증 (9개) |
| `Tests/EditMode/Enemy/EnemyHealthTests.cs` | `EnemyHealth` TakeDamage·OnDied·중복 사망 방지·예외 방어 검증 (8개) |
| `Tests/EditMode/Enemy/EnemyAttackerTests.cs` | `EnemyAttacker` 초기 IsOnCooldown·null 방어·delay=0 Execute 호출·IsDead 타겟 무시 검증 (4개) |
| `Tests/EditMode/Enemy/CsvEnemyParserTests.cs` | `CsvEnemyParser` 정상 파싱·다중 행·빈 입력·오류 행 skip, `EnemyRepository` GetById 대소문자·null·GetAll 검증 (13개) |
