# The Village

![Unity](https://img.shields.io/badge/Unity-6-black?logo=unity) ![C#](https://img.shields.io/badge/C%23-purple?logo=csharp) ![Status](https://img.shields.io/badge/status-in%20progress-yellow) ![License](https://img.shields.io/badge/license-MIT-green)

> 🇺🇸 [View in English](README.md)

Unity 6, C# 기반 2D 마을·던전 탐험 게임.

---

## 개발 현황

현재 **개발 진행 중**입니다. 시스템을 단계적으로 설계·구현하고 있으며, 아래 표는 현재 상태를 나타냅니다.

| 시스템 | 상태 |
|--------|------|
| Core / ServiceLocator / 입력 | ✅ 구현 및 동작 확인 |
| 플레이어 (이동·채취·장비) | ✅ 구현 및 동작 확인 |
| 인벤토리 | ✅ 구현 및 동작 확인 |
| 던전 (스폰·채취 파이프라인) | ✅ 구현 및 동작 확인 |
| 적 AI (FSM·이동·전투) | 🔧 스크립트 구현 / 에디터 에셋 설정 진행 중 |
| 상점 / 재화 | 🔧 스크립트 구현 / 에디터 에셋 설정 진행 중 |
| UI (인벤토리·상점·판매 팝업) | 🔧 스크립트 구현 / 프리팹 설정 진행 중 |
| 레벨 디자인 / 콘텐츠 | 🔲 미시작 |
| 적 AI Phase 6 (원거리 공격) | 🔲 미시작 |
| 전체 게임 루프 / 밸런싱 | 🔲 미시작 |

---

## 개발 방식

이 프로젝트는 단순한 AI 코드 생성이 아니라, **Claude Code의 Agent·Plan Mode·Skills 기능을 적극 활용하여 AI 보조 개발 워크플로우를 구성**하여 개발했습니다.

### AI 워크플로우

| 모드 | 활용 내용 |
|------|-----------|
| **Agent Teammates** | Claude Code 실험 기능인 Agent Teammates(Explore·Plan)를 활용해 코드베이스 분석 및 구현 계획 수립. 각 teammate에 명세를 제공하고 결과를 검토. |
| **AI Assist** | 단일 세션 내 구현·리뷰·수정 반복. 생성된 코드를 직접 검토하고 아키텍처 규칙과 맞지 않으면 재지시. |
| **Plan Mode** | 구현 전 계획을 검토·승인하는 게이트키핑 단계로 활용. 설계와 코드 생성 사이의 검증 역할. |
| **Skills** | 반복 작업(커밋, 컨텍스트 업데이트 등)을 커스텀 슬래시 커맨드로 자동화. |

### 직접 수행한 역할

- 전체 모듈 구조 및 인터페이스 설계 (어떤 에이전트도 이 결정을 대신하지 않음)
- 각 에이전트에게 전달할 명세 작성 및 컨텍스트 관리
- `AI_Context/` 폴더 직접 유지 — 아키텍처 문서, 코딩 규칙, 변경 이력 관리
- 에이전트 출력 코드 검토 및 아키텍처 기준으로 수정 지시
- Plan Mode를 통해 코드 작성 전 구현 계획을 직접 검토·승인

---

## 기술 스택

| | |
|---|---|
| 엔진 | Unity 6 |
| 언어 | C# |
| 비동기 | UniTask |
| 입력 | Unity New Input System |
| 테스트 | Unity Test Framework (EditMode) |
| 데이터 | CSV + ScriptableObject |

---

## 아키텍처

### DI — ServiceLocator 패턴

MonoBehaviour 생명주기와 독립적으로 서비스를 관리하기 위해 ServiceLocator 패턴을 채택했습니다.  
전역 서비스는 `GameBootstrap`에서, 씬 전용 서비스는 각 씬의 `XxxSceneInitializer`에서 등록·해제합니다.

```
ServiceLocator
  ├── IInventory
  ├── ICurrencyService
  ├── IItemCatalogRepository
  ├── ISpriteRepository
  ├── IGameStateService
  ├── ISceneTransitionService
  ├── InputManager
  └── IUIManager
```

### MonoBehaviour 브릿지 패턴

Unity API가 필요한 생명주기 처리는 MonoBehaviour에서 담당하고, **모든 로직은 순수 C# 클래스에 위임**합니다.  
이를 통해 Unity 없이도 EditMode 단위 테스트가 가능합니다.

```
PlayerController (MonoBehaviour — 입력 구독, Transform 반영)
    └── PlayerMover (순수 C# — 이동 벡터 계산)

EnemyController (MonoBehaviour — 브릿지)
    ├── EnemyBrain  (순수 C# — FSM 상태 전환)
    ├── EnemyMover  (순수 C# — 이동 계산)
    ├── EnemyHealth (순수 C# — HP 관리)
    └── EnemyAttacker (순수 C# — 공격 전략)
```

### 의존성 흐름

```
GameBootstrap
  └─ ServiceLocator
       ├─ ItemCatalogRepository   ← ItemCatalog.csv
       ├─ SpriteRepository        ← Resources/Sprites/Items/{itemId}
       ├─ GameStateManager
       ├─ InputManager
       ├─ SceneTransitionManager
       ├─ Inventory
       ├─ CurrencyManager
       └─ [던전 씬] DungeonSceneInitializer
            ├─ DropTableRepository
            ├─ SpawnPointRepository → HarvestableSpawner
            └─ EnemyRepository → EnemySpawner
                 └─ EnemyController (Brain / Mover / Health / Attacker)

TownSceneInitializer
  └─ ShopService (IItemCatalogRepository + ICurrencyService + IInventory)
```

---

## 구현된 시스템

| 시스템 | 설명 |
|--------|------|
| **Inventory** | 슬롯 기반 인벤토리. `IStackable` 구현 아이템 자동 수량 합산. |
| **Shop** | 구매/판매 팝업. `CurrencyManager`(Gold) 연동, 판매가 비율 설정 가능. |
| **Enemy AI** | 5-state FSM (`Idle → Alert → Combat → Attacking → Dead`). 배회·탐색·이탈 로직 포함. |
| **Dungeon** | CSV 기반 자원 스폰 · 채취 홀드 파이프라인. `HarvestController → DropManager → IInventory`. |
| **UI System** | `PopupBase` 기반 팝업 스택. `UIShortcutHandler`로 단축키 데이터 주도 관리. |
| **Player** | 이동 · 도구 장착 · 채취 홀드 · 사망 처리. |
| **Scene Transition** | 페이드 인/아웃 + 로딩 씬 경유 전환. UniTask 비동기 초기화로 프리징 방지. |
| **Currency** | `ICurrencyService` — Gold / SkillPoint 다중 재화 관리. |

---

## 프로젝트 구조

```
Assets/Scripts/
├── Camera/         카메라 팔로우 시스템
├── Core/           공통 인프라 (Enum · Interface · ServiceLocator · CsvReader)
├── Data/           ScriptableObject 기반 설정 데이터
├── Dungeon/        던전 자원 채취 · 스폰 · 사망 처리
├── Editor/         에디터 전용 유틸리티
├── Enemy/          적 AI (FSM · 이동 · 체력 · 전투 · CSV 데이터)
├── Graphic/        스프라이트 로딩 · 저장소
├── Inventory/      슬롯 기반 인벤토리
├── Item/           아이템 데이터 모델 · 카탈로그 · 팩토리
├── Loading/        씬 페이드 · 로딩 시퀀스
├── Manager/        Bootstrap · 입력 · 씬 전환 · 상태 · 재화 관리
├── Player/         이동 · 장비 · 채취 · 체력
├── Shop/           상점 서비스 로직
├── Town/           마을 씬 트리거
└── Tests/          EditMode 단위 테스트 (27개)
```

각 모듈은 Assembly Definition으로 분리되어 컴파일 경계와 의존성 방향을 강제합니다.

```
Game.Core → Game.Manager → Game.Player
                         → Game.Tests.EditMode (테스트 전용, 런타임 미포함)
```

---

## 설계 의사결정

**ServiceLocator 채택**  
Unity 환경에서 Zenject 같은 외부 DI 프레임워크 없이 MonoBehaviour 생명주기와 독립적인 의존성 관리가 필요했습니다. 씬별 Initializer에서 등록·해제를 명시적으로 처리해 씬 전환 시 메모리 누수를 방지합니다.

<details>
<summary>ServiceLocator 코드 보기</summary>

```csharp
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<TInterface>(TInterface implementation)
    {
        if (implementation == null)
            throw new ArgumentNullException(nameof(implementation),
                $"[ServiceLocator] '{typeof(TInterface).Name}' 구현체가 null 입니다.");

        _services[typeof(TInterface)] = implementation;
    }

    public static TInterface Get<TInterface>()
    {
        if (_services.TryGetValue(typeof(TInterface), out var service))
            return (TInterface)service;

        throw new InvalidOperationException(
            $"[ServiceLocator] '{typeof(TInterface).Name}' 서비스가 등록되지 않았습니다. " +
            "GameBootstrap.RegisterServices() 가 먼저 실행되었는지 확인하세요.");
    }

    public static void Unregister<TInterface>()
    {
        var key = typeof(TInterface);
        if (!_services.ContainsKey(key))
            throw new InvalidOperationException(
                $"[ServiceLocator] '{key.Name}' 는 등록되어 있지 않습니다.");
        _services.Remove(key);
    }

    public static void Clear() => _services.Clear();
}
```

</details>

---

**MonoBehaviour 브릿지 패턴 — 순수 C# Presenter**  
UI 로직을 순수 C# `InventoryPresenter`에 위임해 Unity 없이 EditMode 테스트가 가능합니다. `IInventoryView` 인터페이스로 View와 통신하므로 실제 UI와 완전히 분리됩니다.

<details>
<summary>InventoryPresenter 코드 보기</summary>

```csharp
public sealed class InventoryPresenter : IDisposable
{
    private readonly IInventory        _inventory;
    private readonly ISpriteRepository _spriteRepository;
    private          IInventoryView    _view;

    public InventoryPresenter(IInventory inventory, ISpriteRepository spriteRepository)
    {
        _inventory        = inventory        ?? throw new ArgumentNullException(nameof(inventory));
        _spriteRepository = spriteRepository ?? throw new ArgumentNullException(nameof(spriteRepository));

        _inventory.OnInventoryChanged += OnInventoryChanged;
    }

    public void Bind(IInventoryView view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        Refresh();
    }

    public void Unbind() => _view = null;

    public void Dispose()
    {
        _inventory.OnInventoryChanged -= OnInventoryChanged;
        _view = null;
    }

    private void OnInventoryChanged() => Refresh();

    private void Refresh()
    {
        if (_view == null) return;

        var viewModels = new SlotViewModel[_inventory.SlotCount];
        for (int i = 0; i < _inventory.SlotCount; i++)
        {
            var item = _inventory.GetSlot(i);
            if (item == null) { viewModels[i] = SlotViewModel.Empty; continue; }

            var icon         = _spriteRepository.GetSprite(item.ItemId);
            var quantityText = item is IStackable s && s.Quantity > 1
                ? s.Quantity.ToString() : string.Empty;

            viewModels[i] = new SlotViewModel(icon, item.ItemName, quantityText);
        }
        _view.Refresh(viewModels);
    }
}
```

</details>

---

**Enemy AI FSM**  
`Idle(배회) → Alert(탐색) → Combat(추적) → Attacking(공격) → Dead` 5-state FSM. 플레이어가 AlertRange를 벗어나면 마지막 목격 위치를 탐색 후 복귀합니다.  
상태 판단(`EnemyBrain`)과 이동·공격 실행(`EnemyController`)을 분리해 각각 독립 테스트가 가능합니다.

<details>
<summary>EnemyBrain FSM 코드 보기</summary>

```csharp
// 상태 결정만 책임집니다. 이동·공격 실행은 EnemyController 담당. (SRP)
public sealed class EnemyBrain
{
    private readonly EnemyData _data;

    public EnemyBrain(EnemyData data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public EnemyState Tick(EnemyState current, float distance, bool isAttacking)
    {
        if (current == EnemyState.Dead) return EnemyState.Dead;

        switch (current)
        {
            case EnemyState.Idle:
                return distance <= _data.AlertRange ? EnemyState.Alert : EnemyState.Idle;

            case EnemyState.Alert:
                if (distance <= _data.CombatRange) return EnemyState.Combat;
                if (distance >  _data.AlertRange)  return EnemyState.Idle;
                return EnemyState.Alert;

            case EnemyState.Combat:
                if (distance <= _data.AttackRange) return EnemyState.Attacking;
                if (distance >  _data.CombatRange) return EnemyState.Alert;
                return EnemyState.Combat;

            case EnemyState.Attacking:
                if (isAttacking)                   return EnemyState.Attacking;
                if (distance <= _data.AttackRange) return EnemyState.Attacking;
                return EnemyState.Combat;

            default: return EnemyState.Idle;
        }
    }
}
```

</details>


