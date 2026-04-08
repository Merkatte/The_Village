# The Village — Claude Code 지침

## 프로젝트 개요

Unity 기반 2D 게임 프로젝트. C# / Unity 6 사용.

코드 작성 전 `AI_Context/architecture_index.md`(구조 인덱스)와 `AI_Context/code_rule.md`(규칙)를 먼저 확인한다.
작업 모듈에 해당하는 architecture 상세 파일을 추가로 읽는다.
변경 이력은 `AI_Context/log.md`에서 확인한다.

### AI_Context 파일 목록

| 파일 | 역할 |
|------|------|
| `AI_Context/architecture_index.md` | 아키텍처 경량 인덱스. 모듈 목록·의존성 흐름·미구현 현황. **항상 먼저 읽는다** |
| `AI_Context/architecture_core_manager.md` | Core (Enum·Interface·Infrastructure) · Manager · Currency 상세 |
| `AI_Context/architecture_gameplay.md` | Player · Camera · Loading · Dungeon · Graphic · Data/Config · Town 상세 |
| `AI_Context/architecture_enemy.md` | Enemy 시스템 (Core·Movement·Health·Combat·Data) 상세 |
| `AI_Context/architecture_item_shop.md` | Item · Inventory · Shop 상세 |
| `AI_Context/architecture_ui.md` | UI · Editor · Tests 상세 |
| `AI_Context/code_rule.md` | 실제 코드베이스 분석으로 도출한 코딩 규칙. 새 스크립트 작성 시 반드시 준수 |
| `AI_Context/log.md` | AI 작업 변경 이력 (최근). 날짜별 생성·수정·삭제 기록 |
| `AI_Context/log_archive/2026_03.md` | 2026년 3월 이력 아카이브 |
| `AI_Context/progress.md` | 작업 전 현재 상태·다음 할 일 파악용. 작업 시작 전 읽고, 완료 후 반드시 업데이트 |
| `AI_Context/enemy_system_plan.md` | 적 AI 시스템 구현 플랜. 설계 결정 사항 및 Phase별 진행 현황 |

---

## 아키텍처

- **DI**: ServiceLocator 패턴 (`Core/Infrastructure/ServiceLocator.cs`)
- **전역 서비스 등록**: `Manager/Bootstrap/GameBootstrap.cs`
- **씬 전용 서비스**: 해당 씬의 `XxxSceneInitializer`에서 등록 + `OnDestroy()`에서 해제
- **MonoBehaviour는 브릿지만**: 로직은 반드시 순수 C# 클래스에 위임

---

## 코드 규칙 요약

### 네임스페이스
```
Game.<모듈>.<서브모듈>   예) Game.Inventory, Game.Dungeon.Resource
```

### 네이밍
| 대상 | 규칙 |
|------|------|
| 인터페이스 | `IFoo` |
| private 필드 | `_camelCase` |
| 이벤트 | `OnFoo` |
| 충돌 회피 구현체 | `FooImpl` |

### 설계
- 확장 불필요 클래스는 `sealed` 선언
- `ServiceLocator.Get<I>()`는 `Awake()`에서 호출
- 이벤트 구독은 `Awake()`, 해제는 `OnDestroy()` (null 체크 필수)
- 예외 메시지 형식: `[클래스명] 설명`

### 주석
- public 멤버는 XML 문서 주석 필수 (`/// <summary>`)
- 인터페이스 구현체는 `/// <inheritdoc/>` 사용

---

## 단위 테스트

- **위치**: `Tests/EditMode/<모듈>/`
- **클래스명**: `<대상>Tests`
- **메서드명**: 한국어 + 언더스코어 (`TryAddItem_가득찼을때_false_반환`)
- `[TearDown]`에서 `ServiceLocator.Clear()` 호출 (ServiceLocator 사용 시)
- 인터페이스 의존은 `FakeXxx` / `StubXxx` 인라인 정의

---

## CSV 데이터 패턴

| 역할 | 클래스 |
|------|--------|
| 파싱 | `CsvXxxParser` |
| 로드 후 주입 | `XxxLoader` (static) |
| 런타임 조회 | `IXxxRepository` / `XxxRepository` |

---

## 주요 시스템 위치

| 시스템 | 경로 |
|--------|------|
| 인벤토리 | `Inventory/` |
| 아이템 카탈로그 | `Item/Catalog/` |
| 스프라이트 | `Graphic/Sprite/` |
| 드롭 테이블 | `Dungeon/Resource/` |
| 스폰 시스템 | `Dungeon/Spawn/` |
| UI 팝업 | `UI/Core/` |
| 인벤토리 UI | `UI/Inventory/` |
| 적 AI | `Enemy/` |

