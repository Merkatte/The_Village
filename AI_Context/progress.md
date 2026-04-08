# AI Context — Progress (하네스)

> AI 에이전트가 작업 전 현재 상태와 다음 할 일을 빠르게 파악하기 위한 문서입니다.
> 작업 시작 전 반드시 이 파일을 읽고, 작업 완료 후 반드시 이 파일을 업데이트하세요.
> 최종 업데이트: 2026-04-07

---

## 현재 상태 요약

스크립트 레이어는 거의 완성된 상태이며, **Unity 에디터 내 프리팹·에셋 설정 작업**이 남아있습니다.
마을(Town) 씬 기반의 상점/판매/인벤토리 UI 흐름이 코드 상으로 구현되어 있으나, 실제 씬에서 동작하려면 프리팹 세팅이 필요합니다.

---

## 완료된 시스템 (코드 기준)

| 시스템 | 완료일 | 비고 |
|--------|--------|------|
| 인벤토리 (슬롯·스택·이벤트) | 2026-03-16~17 | `Inventory/` |
| 아이템 카탈로그 (CSV) | 2026-03-17 | `Item/Catalog/` |
| 드롭 테이블 | 2026-03-17 | `Dungeon/Resource/` |
| 스폰 시스템 | 2026-03-17 | `Dungeon/Spawn/` |
| 스프라이트 저장소 | 2026-03-17 | `Graphic/Sprite/` |
| 채취 파이프라인 | 2026-03-17 | HarvestController → DropManager → IInventory |
| UI 팝업 시스템 (Core) | 2026-03-18 | `UI/Core/` |
| 인벤토리 UI | 2026-03-18 | `UI/Inventory/` |
| 재화 시스템 (CurrencyManager) | 2026-03-24 | `Manager/Currency/` |
| 상점 시스템 (ShopService) | 2026-03-24 | `Shop/` |
| 상점 UI | 2026-03-24 | `UI/Shop/` (구매 전용, 탭 없음) |
| 판매 팝업 UI | 2026-03-28 | `UI/Sell/` + `UI/Slot/BaseSlotUI.cs` |
| 던전 입장 발판 | 2026-03-28 | `Town/DungeonEntryTrigger.cs`, `UI/DungeonEntry/` |
| 마을 복귀 발판 | 2026-03-28 | `Dungeon/TownReturnTrigger.cs` |
| 팝업 단축키 시스템 (UIShortcutConfig + UIShortcutHandler) | 2026-03-30 | `Data/Config/`, `UI/Core/` |
| TDD 보강 (CurrencyManager, ItemFactory, ItemCatalogRepo, ShopService, Presenter 3종) | 2026-03-31 | `Tests/EditMode/Manager|Item|Shop|UI/` |
| 적 AI 시스템 Phase 1·2 (인터페이스·열거형·순수 C# 로직 + 테스트 4종) | 2026-04-01 | `Core/Enums/Enemy*`, `Enemy/**`, `Tests/EditMode/Enemy/` |
| 적 AI 시스템 Phase 3 (CSV 파이프라인 + 테스트) | 2026-04-02 | `Enemy/Data/`, `Data/Config/EnemyCsvConfig`, `Resources/Data/EnemyData.csv` |
| 적 AI 시스템 Phase 4 (MonoBehaviour 브릿지) | 2026-04-02 | `PlayerHealth`, `PlayerController` 수정, `EnemyController` |
| 적 AI 시스템 Phase 5 (씬 연결) | 2026-04-02 | `EnemyPrefabConfig`, `EnemySpawnConfig`, `EnemySpawner`, `DungeonSceneInitializer` 수정 |
| 적 AI FSM 개선 (배회·탐색·이탈) | 2026-04-07 | `EnemyBrain` FSM 전환, `EnemyController` 배회/탐색 로직, `EnemyBrainTests` 3개 추가 |
| 적 AI EnemyWanderer 분리 + CSV 14열화 | 2026-04-07 | `EnemyWanderer.cs` 신규, `EnemyData`·`CsvEnemyParser`·`EnemyController`·`EnemyData.csv` 수정, 테스트 업데이트 |
| 플레이어 사망 처리 시스템 | 2026-04-07 | `PopupType.PlayerDeath`, `PopupBase.IsEscClosable`, `UIManager` ESC 차단, `PlayerDeathPopup`, `PlayerDeathHandler` |

---

## Unity 설정 대기 중 (프리팹·에셋)

> 코드는 완성. Unity 에디터에서 직접 작업이 필요한 항목들입니다.

| 항목 | 세부 내용 |
|------|-----------|
| 인벤토리 UI 프리팹 | `InventoryUI` + `InventorySlotUI` × 20 배치, 드래그 앤 드롭 핸들러 연결 |
| 상점 UI 프리팹 | `ShopUI` + `ShopSlotUI` Big(3) / Small(6) 배치, `_goldText` 연결 |
| 판매 UI 프리팹 | `SellUI` + `SellSlotUI` × 인벤토리 슬롯 수, `_selectionHighlight` Image 오버레이 |
| ShopConfig 에셋 | ScriptableObject 생성, 판매 품목 ItemId 입력, 구입/판매 비율 설정 |
| UIPopupConfig 에셋 | `PopupType` → 각 프리팹 매핑 등록 (Inventory, Shop, Sell, DungeonEntry, **PlayerDeath**) |
| PlayerDeathPopup 프리팹 | Canvas 포함, 텍스트("플레이어가 사망했습니다") + "마을로 돌아가기" 버튼. `PlayerDeathPopup.OnReturnToTownClicked()` 연결 |
| PlayerDeathHandler 배치 | 던전 씬 오브젝트에 `PlayerDeathHandler` 컴포넌트 부착 |
| UIShortcutConfig 에셋 | ScriptableObject 생성, Entries에 Inventory 항목(`<Keyboard>/i`, `<Gamepad>/select`, AllowedContexts: Gameplay+UI) 추가, GameBootstrap `shortcutConfig` 슬롯 연결 |

---

## 다음 구현 예정 (스크립트)

> 우선순위 순으로 나열. 작업 착수 시 이 섹션을 업데이트하세요.

| 우선순위 | 시스템 | 설명 |
|----------|--------|------|
| - | 적 AI Phase 6 — 원거리 확장 (선택) | `RangedAttackStrategy` (별도 세션) |

---

## 진행 중인 작업

> 현재 AI 에이전트가 작업 중인 항목을 여기에 기록합니다.

| 에이전트 | 작업 내용 | 시작일 |
|----------|-----------|--------|
| (없음) | 모든 코드 구현 완료. Unity Editor 프리팹·에셋 설정 대기 중 | 2026-04-07 |

---

## 업데이트 규칙

- 새 스크립트 생성/수정 → `log.md` 에도 동일하게 기록
- 시스템 완료 → "완료된 시스템" 표에 추가
- Unity 설정 완료 → "Unity 설정 대기 중" 표에서 제거
- 다음 작업 확정 → "다음 구현 예정" 표에 추가
- 작업 착수 → "진행 중인 작업" 표에 본인 항목 추가
- 작업 완료 → "진행 중인 작업" 표에서 제거
