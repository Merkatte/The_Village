# 아키텍처 — Player · Camera · Loading · Dungeon · Graphic · Data

> 최종 업데이트: 2026-04-07

---

## Player — 플레이어 시스템

| 파일 | 설명 |
|------|------|
| `Player/Movement/PlayerMover.cs` | `IMovable` 구현. 이동 벡터 계산 순수 C# 로직 |
| `Player/PlayerController.cs` | 입력 이벤트 구독 후 `PlayerMover`를 통해 오브젝트 이동 반영. `PlayerHealth.IsDead` 시 이동 중단 |
| `Player/PlayerHealth.cs` | `IDamageable` + MonoBehaviour. `Awake`에서 `ServiceLocator.Register<PlayerHealth>(this)`. EnemyAttacker가 직접 피해 적용 |
| `Player/InteractionDetector.cs` | 플레이어 주변 오브젝트 상호작용 감지 (`OnTriggerEnter/Exit2D` 기반) |
| `Player/PlayerInitializer.cs` | Player 계층 컴포지션 루트. `Awake`에서 `HarvestController.Init()` 호출하여 의존성 연결 |
| `Player/PlayerEquipment.cs` | 현재 장착 도구(`HarvestTool`) 보관. `EquipHarvestTool()` / `UnequipHarvestTool()`으로 교체 |
| `Player/HarvestController.cs` | 채취 홀드 입력 감지 → 장착 도구·Harvestable 타입 호환 확인 → 홀드 타이머 누적 → `target.Harvest()` 호출 |

---

## Camera — 카메라 시스템

| 파일 | 설명 |
|------|------|
| `Camera/ICameraFollower.cs` | 카메라 팔로우 로직 인터페이스 |
| `Camera/CameraFollower.cs` | `ICameraFollower` 구현. 대상 추적 + 범위 클램핑 |
| `Camera/CameraController.cs` | MonoBehaviour. 매 프레임 `CameraFollower`에 계산 위임 |

---

## Loading — 씬 전환 로딩

| 파일 | 설명 |
|------|------|
| `Loading/SceneFader.cs` | `ISceneFader` 구현. 페이드 아웃/인 코루틴 |
| `Loading/Scene/LoadingSceneController.cs` | 로딩 씬 컨트롤러. 진행률 표시 및 다음 씬 로드 시퀀스 |

---

## Dungeon — 던전 시스템

### Data (로더)
| 파일 | 설명 |
|------|------|
| `Dungeon/Data/DropTableLoader.cs` | `Load(TextAsset)` + `LoadFromText(string)` 제공. 후자는 백그라운드 스레드에서 호출 가능 |
| `Dungeon/Data/SpawnPointLoader.cs` | `Load(TextAsset)` + `LoadFromText(string)` 제공. 후자는 백그라운드 스레드에서 호출 가능 |

### Resource (채취 자원 드롭)
| 파일 | 설명 |
|------|------|
| `Dungeon/Resource/ResourceDrop.cs` | 자원 획득 결과 불변 구조체 (`ResourceType`, 수량) |
| `Dungeon/Resource/DropTable.cs` | `DropEntry` 데이터 클래스 + `IDropTable` 인터페이스 (`Roll(IRandom)`) |
| `Dungeon/Resource/IRandom.cs` | 난수 추상화 인터페이스 + `UnityRandom` 구현 (테스트 대응) |
| `Dungeon/Resource/DropTableImpl.cs` | `IDropTable` 구현. `DropEntry` 목록 보관. `Roll(IRandom)` 호출 시 확률 판정 |
| `Dungeon/Resource/IDropTableRepository.cs` | 드롭 테이블 저장소 인터페이스 (`GetTable(sourceId) → IDropTable`) |
| `Dungeon/Resource/DropTableRepository.cs` | `IDropTableRepository` 구현. 생성자에서 `IDropTable` 인스턴스 1회 생성·보관 |
| `Dungeon/Resource/CsvDropTableParser.cs` | CSV 파일 → `Dictionary<string, List<DropEntry>>` 파싱 |
| `Dungeon/Resource/Harvestable.cs` | MonoBehaviour. 채취 가능 오브젝트. `DropTableId` getter+setter |

> `DropTableImpl` 은 entries 만 보관하는 순수 데이터 객체입니다. `IRandom` 은 `Roll()` 호출 시 주입되므로 수확할 때마다 새로 생성하지 않습니다.

### Spawn (오브젝트 스폰)
| 파일 | 설명 |
|------|------|
| `Dungeon/Spawn/SpawnPointData.cs` | 스폰 지점 데이터. `SpawnEntry` 에 `DropTableId` 포함 |
| `Dungeon/Spawn/ISpawnPointRepository.cs` | 스폰 지점 저장소 인터페이스 |
| `Dungeon/Spawn/SpawnPointRepository.cs` | `ISpawnPointRepository` 구현 |
| `Dungeon/Spawn/CsvSpawnPointParser.cs` | CSV 파일 → `SpawnPointData` 파싱 (6열: SpawnPointId, X, Y, ResourceType, SpawnChance, DropTableId) |

### Manager (런타임 관리)
| 파일 | 설명 |
|------|------|
| `Dungeon/Manager/DropManager.cs` | MonoBehaviour. `GetTable(id).Roll(_random)` 으로 드롭 판정 → 인벤토리 추가 |
| `Dungeon/Manager/HarvestableSpawner.cs` | MonoBehaviour. `ResourcePrefabConfig` 로 프리팹 결정 → 스폰 후 `entry.DropTableId` 를 `Harvestable` 에 주입. `InitAsync()` (UniTask) 로 Instantiate 를 프레임 분산 |

### Scene
| 파일 | 설명 |
|------|------|
| `Dungeon/Scene/DungeonSceneInitializer.cs` | MonoBehaviour. 던전 씬 진입 시 던전 전용 서비스 ServiceLocator 등록. `Awake()` 는 비동기(`UniTaskVoid`). CSV 파싱은 백그라운드 스레드, Unity API 호출은 메인 스레드에서 수행. Step 7에서 `EnemySpawner.InitAsync()` 호출 |
| `Dungeon/TownReturnTrigger.cs` | MonoBehaviour. 발판 Trigger2D 진입 시 `TransitionToWithLoading(Town)` 으로 즉시 마을 복귀. 데이터(인벤토리·골드)는 ServiceLocator 에 남아 보존 |
| `Dungeon/PlayerDeathHandler.cs` | MonoBehaviour 브릿지. `Start()`에서 `PlayerHealth.OnDied` 구독 → `IUIManager.Open(PlayerDeath)` 호출. 던전 씬 오브젝트에 부착 |

---

## Graphic — 그래픽 리소스 관리

### Sprite
| 파일 | 설명 |
|------|------|
| `Graphic/Sprite/SpriteRepository.cs` | `ISpriteRepository` 구현체. `Dictionary<string, Sprite>` 캐시. 조회 실패 시 null 반환 |
| `Graphic/Sprite/SpriteLoader.cs` | Bootstrap 호출용 정적 로더. `Resources/Sprites/Items/{itemId}` 명명 규칙으로 일괄 로드 |

> 스프라이트 파일은 `Assets/Resources/Sprites/Items/` 에 ItemId 와 동일한 파일명으로 배치해야 합니다.
> 파일 누락 시 경고 로그 출력, `GetSprite()` 는 null 반환 (UI 에서 fallback 처리).

---

## Data — 설정 데이터 (ScriptableObject)

| 파일 | 설명 |
|------|------|
| `Data/Config/GlobalCsvConfig.cs` | 전역 CSV 파일 경로 참조 관리 |
| `Data/Config/DungeonCsvConfig.cs` | 던전 전용 CSV 파일 경로 참조 관리 |
| `Data/Config/ResourcePrefabConfig.cs` | `ResourceType` → `GameObject` 프리팹 매핑. `GetPrefab(ResourceType)` 으로 조회 |
| `Data/Config/UIShortcutConfig.cs` | `UIShortcutEntry` 목록. `PopupType`, `Bindings`, `AllowedContexts` 보관. `[CreateAssetMenu("Game/Config/UI Shortcut Config")]` |
| `Data/Config/EnemyCsvConfig.cs` | `enemyCsv` TextAsset 필드. `[CreateAssetMenu("Game/Config/Enemy CSV Config")]` |
| `Data/Config/EnemyPrefabConfig.cs` | `EnemyType` → `GameObject` 프리팹 매핑. `GetPrefab(EnemyType)` 으로 조회. `[CreateAssetMenu("Game/Config/Enemy Prefab Config")]` |
| `Data/Config/EnemySpawnConfig.cs` | `EnemySpawnEntry[]` (EnemyId, Position, SpawnChance). `[CreateAssetMenu("Game/Config/Enemy Spawn Config")]` |
| `Data/Config/ShopConfig.cs` | 판매 품목 `ItemId` 목록 + 구입가/판매가 비율 설정 |

---

## Town — 마을 씬

| 파일 | 설명 |
|------|------|
| `Town/Scene/TownSceneInitializer.cs` | MonoBehaviour. 마을 씬 진입 시 `IShopService` 등록, `UIManager` 캔버스 주입, 마을 팝업 등록 |
| `Town/DungeonEntryTrigger.cs` | MonoBehaviour. 발판 Trigger2D 진입 시 `PopupType.DungeonEntry` 팝업 오픈. 이미 열려 있으면 무시 |
