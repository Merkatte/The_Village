# 아키텍처 — Core · Manager · Currency

> 최종 업데이트: 2026-04-02

---

## Core — 공통 인프라

### Enums
| 파일 | 설명 |
|------|------|
| `Core/Enums/GameState.cs` | 게임 전체 상태 정의 (`None`, `Town`, `Dungeon`) |
| `Core/Enums/InputContext.cs` | 입력 맥락 구분 (`Gameplay`, `UI`) |
| `Core/Enums/SceneType.cs` | 씬 종류 (`Bootstrap`, `Town`, `Dungeon`, `Loading`) |
| `Core/Enums/ResourceType.cs` | 채취 자원 종류 (목재, 광석, 드롭템 등) |
| `Core/Enums/ToolType.cs` | 도구 종류 (`Axe`, `Pickaxe`, `Scythe`, `Shovel`) |
| `Core/Enums/EnemyState.cs` | 적 AI 상태 (`Idle`, `Alert`, `Combat`, `Attacking`, `Dead`) |
| `Core/Enums/EnemyType.cs` | 적 종류 (`Slime`, `Goblin`, `Orc`) |
| `Core/Enums/AttackType.cs` | 공격 방식 (`Melee`, `Ranged`) |
| `Core/Enums/HarvestableType.cs` | 채취 가능 오브젝트 종류 (`Tree`, `Ore`). 장착 도구와 호환 여부 판단에 사용 |
| `Core/Enums/ItemType.cs` | 아이템 종류 (`Resource`, `HarvestTool`, `FarmTool`, `Weapon`, `Armor`). ItemCatalog CSV `ItemType` 컬럼에서 사용 |
| `Core/Enums/CurrencyType.cs` | 재화 종류 (`Gold`, `SkillPoint`) |

### Interfaces
| 파일 | 설명 |
|------|------|
| `Core/Interfaces/IGameStateService.cs` | 게임 상태 변경·구독 인터페이스 |
| `Core/Interfaces/ISceneLoader.cs` | Unity 씬 로드 추상화 인터페이스 |
| `Core/Interfaces/ISceneFader.cs` | 페이드 인/아웃 효과 인터페이스 |
| `Core/Interfaces/ISceneTransitionService.cs` | 씬 전환 서비스 인터페이스 |
| `Core/Interfaces/IStackable.cs` | 인벤토리 내 수량 합산 가능 아이템 인터페이스 (`Quantity`, `IsFull`, `AddQuantity`, `SetQuantity`) |
| `Core/Interfaces/ISpriteRepository.cs` | ItemId 기반 Sprite 조회 인터페이스 |
| `Core/Interfaces/IDamageable.cs` | 피해를 입을 수 있는 객체 인터페이스 (`CurrentHp`, `IsDead`, `TakeDamage`, `OnDied`) |
| `Core/Interfaces/ICurrencyService.cs` | 재화 조회·획득·소비 인터페이스 (`Get`, `Add`, `TrySpend`, `OnCurrencyChanged`) |

### Infrastructure
| 파일 | 설명 |
|------|------|
| `Core/Infrastructure/ServiceLocator.cs` | 정적 DI 컨테이너. 서비스 등록·조회·제거 담당 |
| `Core/Csv/CsvReader.cs` | CSV 파일 파싱 공통 유틸리티 |

---

## Manager — 게임 시스템 관리

### Bootstrap
| 파일 | 설명 |
|------|------|
| `Manager/Bootstrap/GameBootstrap.cs` | 게임 진입점. 앱 시작 시 ServiceLocator에 핵심 서비스 등록 |

### State
| 파일 | 설명 |
|------|------|
| `Manager/State/GameStateManager.cs` | `IGameStateService` 구현체. 상태 변경 및 이벤트 발행 |

### Input
| 파일 | 설명 |
|------|------|
| `Manager/Input/IPlayerInputReader.cs` | 게임플레이 입력(이동·상호작용) 인터페이스 |
| `Manager/Input/IUIInputReader.cs` | UI 입력(확인·취소) 인터페이스 |
| `Manager/Input/PlayerInputReader.cs` | New Input System 기반 이동 입력 구현. `IDisposable` |
| `Manager/Input/UIInputReader.cs` | New Input System 기반 UI 입력 구현. `IDisposable` |
| `Manager/Input/InputManager.cs` | 입력 맥락(`InputContext`) 전환 및 리더 생명주기 관리. `CurrentContext` 프로퍼티 제공 |

### Movement
| 파일 | 설명 |
|------|------|
| `Manager/Movement/IMovable.cs` | 이동 가능 객체 인터페이스 (`Move(Vector2)`) |

### Scene
| 파일 | 설명 |
|------|------|
| `Manager/Scene/SceneTransitionManager.cs` | `ISceneTransitionService` 구현체. 로딩 씬 경유 전환 제어 |
| `Manager/Scene/UnitySceneLoader.cs` | `ISceneLoader` 구현체. `UnityEngine.SceneManagement` 래퍼 |

### Currency
| 파일 | 설명 |
|------|------|
| `Manager/Currency/CurrencyManager.cs` | `ICurrencyService` 구현체. `Dictionary<CurrencyType, int>` 로 재화 관리 |

> `CurrencyManager` 는 `GameBootstrap` 에서 `ICurrencyService` 로 등록합니다.
