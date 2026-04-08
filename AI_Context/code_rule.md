# The Village - 코드 규칙 (Code Rules)

> 최종 업데이트: 2026-04-07
> 이 문서는 실제 코드베이스를 분석하여 도출한 규칙입니다. 새 스크립트 작성 시 반드시 따르세요.

---

## 1. 네임스페이스

```
Game.<모듈>.<서브모듈>
```

| 예시 | 대상 |
|------|------|
| `Game.Core.Infrastructure` | ServiceLocator 등 인프라 |
| `Game.Core.Enums` | 전역 Enum |
| `Game.Core.Interfaces` | 전역 인터페이스 |
| `Game.Manager.Input` | 입력 관리 |
| `Game.Manager.State` | 게임 상태 관리 |
| `Game.Dungeon.Resource` | 던전 자원 |
| `Game.Player` | 플레이어 시스템 |
| `Game.Inventory` | 인벤토리 시스템 |
| `Game.Tests.EditMode.<모듈>` | 단위 테스트 |

> **네임스페이스-클래스 이름 충돌 주의**
> 클래스 이름이 네임스페이스 세그먼트와 동일한 경우(예: `Game.Inventory.Inventory`)
> 테스트나 외부 파일에서 참조할 때 using 별칭으로 해결합니다.
> ```csharp
> using InventoryImpl = Game.Inventory.Inventory;
> ```
> 클래스 이름에 `Impl` 접미사를 붙이는 것도 허용됩니다 (예: `DropTableImpl`).

---

## 2. 명명 규칙

### 클래스 / 인터페이스

| 종류 | 규칙 | 예시 |
|------|------|------|
| 인터페이스 | `I` 접두사 + PascalCase | `IInventory`, `IGameStateService` |
| 구현 클래스 | PascalCase | `GameStateManager`, `CameraFollower` |
| 충돌 회피용 구현 | `Impl` 접미사 | `DropTableImpl` |
| MonoBehaviour | PascalCase + 역할 명시 | `PlayerController`, `CameraController` |

### 멤버

| 종류 | 규칙 | 예시 |
|------|------|------|
| private 필드 | `_camelCase` | `_moveDirection`, `_holdTimer` |
| public 프로퍼티 | PascalCase | `CurrentState`, `SlotCount` |
| public 메서드 | PascalCase | `ChangeState()`, `TryAddItem()` |
| private 메서드 | PascalCase | `RegisterServices()`, `ValidateIndex()` |
| 이벤트 | `On` 접두사 + PascalCase | `OnStateChanged`, `OnMovePerformed` |
| SerializeField | 접근 제한자 없이 또는 `private` | `[SerializeField] InputManager inputManager` |

---

## 3. 클래스 설계 원칙

### MonoBehaviour vs 순수 C#

| 역할 | 타입 | 이유 |
|------|------|------|
| 입력 구독, Update, Transform 반영 | MonoBehaviour | Unity 생명주기 필요 |
| 이동 계산, 상태 관리, 데이터 처리 | 순수 C# | EditMode 테스트 가능 |

> MonoBehaviour는 "브릿지" 역할만 합니다. 로직은 반드시 순수 C# 클래스에 위임합니다.
> 예: `PlayerController`(브릿지) → `PlayerMover`(로직), `CameraController` → `CameraFollower`

### sealed 사용 기준

- 확장이 필요 없는 구현 클래스는 `sealed` 선언
- 예: `sealed class Inventory`, `sealed class DropTableImpl`, `sealed class HarvestController`
- 기반 동작을 가지거나 테스트에서 상속될 수 있는 클래스는 생략 가능

### SRP (단일 책임 원칙)

한 클래스는 한 가지 책임만 가집니다.
- `PlayerController` = 입력 구독 + Transform 반영 (이동 계산 ❌)
- `PlayerMover` = 이동 벡터 계산 (Unity API 직접 호출 ❌)
- `GameStateManager` = 상태 변경 + 이벤트 발행 (씬 전환 ❌)

---

## 4. 의존성 주입 (ServiceLocator)

```csharp
// 등록: GameBootstrap.RegisterServices() 또는 씬 Initializer
ServiceLocator.Register<IInventory>(new Inventory());

// 조회: Awake() 에서 수행
_inventory = ServiceLocator.Get<IInventory>();

// 해제: OnDestroy() 또는 씬 종료 시
ServiceLocator.Unregister<IInventory>();
```

- **전역 서비스** (`IGameStateService`, `IInventory` 등): `GameBootstrap`에서 등록
- **씬 전용 서비스** (`ISpawnPointRepository` 등): 해당 씬의 SceneInitializer에서 등록 + 해제
- 외부는 반드시 **인터페이스 타입**으로만 참조합니다 (DIP)

---

## 5. 이벤트 구독 패턴

```csharp
private void Awake()
{
    _inputReader = ServiceLocator.Get<IPlayerInputReader>();
    _inputReader.OnHarvestStarted   += OnHarvestStarted;
    _inputReader.OnHarvestCancelled += OnHarvestCancelled;
}

private void OnDestroy()
{
    if (_inputReader == null) return;   // null 방어 필수
    _inputReader.OnHarvestStarted   -= OnHarvestStarted;
    _inputReader.OnHarvestCancelled -= OnHarvestCancelled;
}
```

- 구독은 `Awake()`, 해제는 `OnDestroy()`
- `OnDestroy()` 에서는 반드시 **null 체크** 후 해제

---

## 6. 예외 처리

```csharp
// null 방어 — 생성자에서 즉시 검증
_entries = entries ?? throw new ArgumentNullException(nameof(entries));

// 범위 검증
if (index < 0 || index >= _slots.Length)
    throw new ArgumentOutOfRangeException(nameof(index),
        $"[Inventory] 슬롯 인덱스 {index}는 유효 범위(0~{_slots.Length - 1})를 벗어났습니다.");
```

- 예외 메시지 형식: `[클래스명] 설명`
- 파라미터 검증은 생성자 또는 메서드 진입부에서 즉시 수행

---

## 7. 주석 규칙

### XML 문서 주석 (public 멤버 필수)

```csharp
/// <summary>
/// 인벤토리 서비스 인터페이스입니다.
/// </summary>
public interface IInventory { ... }

/// <inheritdoc/>          ← 인터페이스 구현 시 중복 작성 금지
public bool TryAddItem(ItemData item) { ... }
```

### 클래스 헤더 블록

```csharp
/// <summary>
/// 한 줄 역할 설명입니다.
///
/// ─ 섹션 제목 ──────────────────────────────────────────────
///   - 항목 1
///   - 항목 2
///
/// ─ 다른 섹션 ──────────────────────────────────────────────
///   설명...
/// </summary>
```

자주 쓰는 섹션 키워드: `설계 원칙`, `동작 규칙`, `배치 위치`, `채취 흐름`, `등록 서비스`, `인스펙터 설정`

---

## 8. IDisposable 패턴

`InputAction` 등 비관리 리소스를 보유하는 순수 C# 클래스는 `IDisposable`을 구현합니다.

```csharp
public void Dispose()
{
    _moveAction.performed -= OnMoveActionPerformed;
    _moveAction.Dispose();
}
```

관리 주체(InputManager 등)가 `Dispose()`를 명시적으로 호출합니다.

---

## 9. 단위 테스트 규칙

- **위치**: `Tests/EditMode/<모듈>/`
- **클래스명**: `<대상클래스>Tests`
- **메서드명**: 한국어, 언더스코어로 구분 (`TryAddItem_가득찼을때_false_반환`)
- **속성**: `[TestFixture]`, `[SetUp]`, `[Test]`, `[Description("설명")]`

```csharp
[TestFixture]
public class InventoryTests
{
    private InventoryImpl _inventory;

    [SetUp]
    public void SetUp() { _inventory = new InventoryImpl(5); }

    [Test]
    [Description("...해야 한다.")]
    public void 한국어_테스트명() { ... }
}
```

- `[TearDown]`에서 `ServiceLocator.Clear()` 호출 (ServiceLocator를 사용한 경우)
- 추상 클래스 의존 시 테스트 파일 내부에 `private sealed class StubXxx : AbstractXxx` 인라인 정의
- 인터페이스 의존 시 `FakeXxx` 또는 `StubXxx` 클래스로 주입

### 테스트 체크리스트 주석

클래스 헤더에 테스트 항목을 체크리스트로 작성합니다.

```csharp
/// ─ TDD 체크리스트 ─────────────────────────────────────────
///   [v] 정상 케이스 검증
///   [v] 경계값 검증
///   [v] 예외 케이스 검증
```

---

## 10. CSV 데이터 패턴

| 역할 | 클래스 종류 |
|------|-------------|
| 파일 파싱 | `CsvXxxParser` (순수 C#) |
| 데이터 로드 후 저장소 주입 | `XxxLoader` (static 유틸) |
| 런타임 조회 | `IXxxRepository` / `XxxRepository` |

---

## 11. 요약 치트시트

```
네임스페이스  Game.<모듈>.<서브모듈>
인터페이스    I접두사, 외부 노출만
구현 클래스   sealed 권장, 로직은 순수 C#
private 필드  _camelCase
이벤트        On접두사 + PascalCase
DI            ServiceLocator.Get<I>() in Awake()
구독 해제     OnDestroy() + null 체크
예외 메시지   [클래스명] 설명
테스트명      한국어_언더스코어
```
