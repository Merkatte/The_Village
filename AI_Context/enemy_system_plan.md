# 적 AI 시스템 구현 플랜

> 작성일: 2026-03-31
> 상태: **구현 중** — Phase 1 진행 중 (토큰 소진으로 중단)

---

## 승인된 설계 요약

### 사용자 제안 AI 루프 → 폐기 이유
순수 순차 코루틴(`대기 → 이동 await → 공격 await → 반복`)은 아래 이유로 채택하지 않음:
1. 이동 `await` 도중 플레이어 이탈 시 인터럽트 처리 복잡
2. 상태 전환 로직이 루프 내부에 매몰 → 하위 클래스 재정의 어려움
3. MonoBehaviour가 로직을 담게 되어 EditMode 테스트 불가

### 채택 구조: 상태 머신 + 제한적 UniTask
- **`EnemyBrain.Tick()`** : 순수 C#, 매 Update에서 호출, 상태 전환 판단
- **`Update()`** : 이동 실행 (PlayerMover 패턴 동일)
- **`UniTask`** : 공격 선딜 → 피격 판정 → 후딜 → 쿨타임 이 4구간에만 사용

---

## 아키텍처

```
[MonoBehaviour 브릿지]     [순수 C# 로직]
EnemyController ─────────> EnemyBrain        (상태 전환 판단 / Tick)
                ─────────> EnemyMover         (IMovable — PlayerMover 패턴)
                ─────────> EnemyHealth        (IDamageable 구현)
                ─────────> EnemyAttacker      (공격 실행 + 쿨타임 / UniTask)
                                 └──────────> IEnemyAttackStrategy
                                                ├─ MeleeAttackStrategy
                                                └─ RangedAttackStrategy (Phase 6)
```

### EnemyState 열거형
| 상태 | 전환 조건 | 행동 |
|------|----------|------|
| `Idle` | 거리 > AlertRange (또는 초기) | EnemyWanderer 배회 (스폰 위치 기준) |
| `Alert` | Idle에서 AlertRange 이내 진입. Combat에서 CombatRange 이탈 시 | 마지막 목격 위치 탐색 이동 |
| `Combat` | Alert에서 CombatRange 이내 진입 | 플레이어 직접 추적 |
| `Attacking` | Combat에서 AttackRange 이내 진입 | 정지 + 공격 시퀀스 (UniTask) |
| `Dead` | EnemyHealth.OnDied | 모든 행동 중단 → 0.5초 후 파괴 |

### 공격 시퀀스 (UniTask)
```
Update: Attacking 상태 + !IsOnCooldown
  → EnemyAttacker.RequestAttackAsync(target, ct).Forget()
      ① await UniTask.Delay(PreAttackDelay)   ← 선딜
      ② strategy.Execute(target)               ← 피격 판정 (IDamageable.TakeDamage)
      ③ await UniTask.Delay(PostAttackStun)    ← 후딜(멍)
      ④ await UniTask.Delay(AttackCooldown)    ← 쿨타임
```

---

## 전체 파일 목록

### 신규 생성
```
Assets/Scripts/Core/Interfaces/
  IDamageable.cs              ✅ 완료 (2026-03-31)

Assets/Scripts/Core/Enums/
  EnemyState.cs               ✅ 완료 (2026-04-01)
  EnemyType.cs                ✅ 완료 (2026-04-01)
  AttackType.cs               ✅ 완료 (2026-04-01)

Assets/Scripts/Enemy/Core/
  EnemyData.cs                ✅ 완료 (2026-04-01) — WanderRadius·WanderInterval 추가 (2026-04-07)
  EnemyBrain.cs               ✅ 완료 (2026-04-01) — FSM 전환으로 개선 (2026-04-07)
  EnemyController.cs          ✅ 완료 (2026-04-02) — EnemyWanderer 사용으로 리팩토링 (2026-04-07)

Assets/Scripts/Enemy/Combat/
  IEnemyAttackStrategy.cs     ✅ 완료 (2026-04-01)
  MeleeAttackStrategy.cs      ✅ 완료 (2026-04-01)
  RangedAttackStrategy.cs     ⬜ 미완료 (Phase 6)
  EnemyAttacker.cs            ✅ 완료 (2026-04-01)

Assets/Scripts/Enemy/Movement/
  EnemyMover.cs               ✅ 완료 (2026-04-01)
  EnemyWanderer.cs            ✅ 완료 (2026-04-07)

Assets/Scripts/Enemy/Health/
  EnemyHealth.cs              ✅ 완료 (2026-04-01)

Assets/Scripts/Enemy/Data/
  IEnemyRepository.cs         ✅ 완료 (2026-04-02)
  EnemyRepository.cs          ✅ 완료 (2026-04-02)
  CsvEnemyParser.cs           ✅ 완료 (2026-04-02) — 14열로 확장 (2026-04-07)
  EnemyLoader.cs              ✅ 완료 (2026-04-02)
  EnemySpawner.cs             ✅ 완료 (2026-04-02)

Assets/Scripts/Data/Config/
  EnemyCsvConfig.cs           ✅ 완료 (2026-04-02)
  EnemyPrefabConfig.cs        ✅ 완료 (2026-04-02)
  EnemySpawnConfig.cs         ✅ 완료 (2026-04-02)

Assets/Scripts/Player/Health/
  PlayerHealth.cs             ✅ 완료 (2026-04-02)

Assets/Scripts/Dungeon/
  PlayerDeathHandler.cs       ✅ 완료 (2026-04-07)

Assets/Scripts/UI/PlayerDeath/
  PlayerDeathPopup.cs         ✅ 완료 (2026-04-07)

Assets/Scripts/Tests/EditMode/Enemy/
  EnemyBrainTests.cs          ✅ 완료 (2026-04-01)
  EnemyMoverTests.cs          ✅ 완료 (2026-04-01)
  EnemyHealthTests.cs         ✅ 완료 (2026-04-01)
  EnemyAttackerTests.cs       ✅ 완료 (2026-04-01)
  CsvEnemyParserTests.cs      ✅ 완료 (2026-04-02)
```

### 기존 수정
```
Dungeon/Scene/DungeonSceneInitializer.cs  ✅ EnemyLoader + EnemySpawner 단계 추가 (2026-04-02)
Player/PlayerController.cs               ✅ PlayerHealth 필드 + IsDead 이동 중단 (2026-04-02)
UI/Core/PopupType.cs                     ✅ PlayerDeath 항목 추가 (2026-04-07)
UI/Core/PopupBase.cs                     ✅ IsEscClosable 프로퍼티 추가 (2026-04-07)
UI/Core/UIManager.cs                     ✅ ESC 차단 체크 추가 (2026-04-07)
Resources/Data/EnemyData.csv             ✅ 14열로 확장 (WanderRadius, WanderInterval) (2026-04-07)
```

---

## 구현 순서 (Phase별)

### ✅ Phase 1 — 인터페이스·열거형 (의존 없음)
- [x] `IDamageable.cs` ← 완료 (2026-03-31)
- [x] `EnemyState.cs` ← 완료 (2026-04-01)
- [x] `EnemyType.cs` ← 완료 (2026-04-01)
- [x] `AttackType.cs` ← 완료 (2026-04-01)

### ✅ Phase 2 — 순수 C# 로직 레이어 → EditMode 테스트 작성 포함
- [x] `EnemyData.cs` — 완료 (2026-04-01)
- [x] `EnemyBrain.cs` — 완료 (2026-04-01)
- [x] `EnemyMover.cs` — 완료 (2026-04-01)
- [x] `EnemyHealth.cs` — 완료 (2026-04-01)
- [x] `IEnemyAttackStrategy.cs` + `MeleeAttackStrategy.cs` — 완료 (2026-04-01)
- [x] `EnemyAttacker.cs` — 완료 (2026-04-01)
- [ ] **테스트 4종 Unity Editor 에서 통과 확인 후 Phase 3 진행**

### ✅ Phase 3 — CSV 데이터 파이프라인
- [x] `CsvEnemyParser.cs` — 완료 (2026-04-02)
- [x] `IEnemyRepository.cs` + `EnemyRepository.cs` — 완료 (2026-04-02)
- [x] `EnemyLoader.cs` — 완료 (2026-04-02)
- [x] `EnemyCsvConfig.cs` — 완료 (2026-04-02)
- [x] `Resources/Data/EnemyData.csv` — 샘플 데이터 3종 (Slime, Goblin, Orc) 완료 (2026-04-02)
- [ ] **CsvEnemyParserTests Unity Editor 에서 통과 확인 후 Phase 4 진행**

### ✅ Phase 4 — MonoBehaviour 브릿지
- [x] `EnemyController.cs` — 완료 (2026-04-02)
- [x] `PlayerHealth.cs` — 완료 (2026-04-02)
- [x] `PlayerController.cs` 수정 — playerHealth 필드 + IsDead 이동 중단 완료 (2026-04-02)

### ✅ Phase 5 — 씬 연결
- [x] `EnemyPrefabConfig.cs` — 완료 (2026-04-02)
- [x] `EnemySpawnConfig.cs` — 완료 (2026-04-02)
- [x] `EnemySpawner.cs` — 완료 (2026-04-02)
- [x] `EnemyLoader.LoadFromText()` 추가 — 완료 (2026-04-02)
- [x] `DungeonSceneInitializer.cs` 수정 — 완료 (2026-04-02)

### ⬜ Phase 6 — 원거리 확장 (별도 세션)
- [ ] `RangedAttackStrategy.cs`

---

## CSV 컬럼 설계

### EnemyData.csv (14컬럼, 전부 필수)
```
EnemyId, EnemyType, MaxHp, MoveSpeed, AlertRange, CombatRange,
AttackRange, AttackDamage, AttackCooldown, PreAttackDelay, PostAttackStun, AttackType,
WanderRadius, WanderInterval
```
샘플:
```csv
EnemyId,EnemyType,MaxHp,MoveSpeed,AlertRange,CombatRange,AttackRange,AttackDamage,AttackCooldown,PreAttackDelay,PostAttackStun,AttackType,WanderRadius,WanderInterval
slime_basic,Slime,30,2,10,6,2,5,2,0.2,0.4,Melee,3,3
goblin_warrior,Goblin,60,3,12,8,2.5,12,1.8,0.3,0.5,Melee,4,2
goblin_archer,Goblin,45,2.5,14,10,8,10,2,0.4,0.3,Ranged,4,2
orc_grunt,Orc,120,2.5,14,10,3,20,2.5,0.5,0.8,Melee,2,4
```
- 범위 제약: `AttackRange ≤ CombatRange ≤ AlertRange`
- `WanderRadius`: Idle 배회 반경 (스폰 위치 기준, Unity 유닛)
- `WanderInterval`: 배회 목적지 도달 후 다음 이동까지 대기 시간 (초)

### EnemySpawnPoints.csv (5컬럼)
```
SpawnPointId, X, Y, EnemyId, SpawnChance
```

---

## 재사용할 기존 코드

| 참조 대상 | 파일 경로 |
|----------|----------|
| `IMovable` 인터페이스 | `Manager/Movement/IMovable.cs` |
| `PlayerMover` (EnemyMover 구조 복제) | `Player/Movement/PlayerMover.cs` |
| `CsvReader` | `Core/Csv/CsvReader.cs` |
| `HarvestableSpawner` (EnemySpawner 패턴) | `Dungeon/Manager/HarvestableSpawner.cs` |
| `CsvItemCatalogParser` (CsvEnemyParser 구조) | `Item/Catalog/CsvItemCatalogParser.cs` |
| `DungeonSceneInitializer` (수정 대상) | `Dungeon/Scene/DungeonSceneInitializer.cs` |

---

## 코드 규칙 (code_rule.md 요약)
- 네임스페이스: `Game.Enemy.Core`, `Game.Enemy.Combat`, `Game.Enemy.Data` 등
- private 필드: `_camelCase`
- 이벤트: `On` 접두사
- 구현 클래스: `sealed` 권장
- 예외 메시지: `[클래스명] 설명`
- 이벤트 구독: `Awake()`, 해제: `OnDestroy()` + null 체크
- 테스트 메서드명: 한국어_언더스코어

---

## EditMode 테스트 체크리스트

**EnemyBrainTests.cs**
```
[ ] 거리 > AlertRange → Idle 반환
[ ] CombatRange < 거리 ≤ AlertRange → Alert 반환
[ ] AttackRange < 거리 ≤ CombatRange → Combat 반환
[ ] 거리 ≤ AttackRange, isAttacking=false → Attacking 반환
[ ] 거리 ≤ AttackRange, isAttacking=true → Attacking 유지
[ ] Dead 상태는 어떤 거리에서도 Dead 유지
[ ] 경계값: distance == AlertRange
[ ] 경계값: distance == AttackRange
```

**EnemyMoverTests.cs** — PlayerMoverTests 구조 동일 복제

**EnemyHealthTests.cs**
```
[ ] TakeDamage → CurrentHp 감소
[ ] TakeDamage 초과 → CurrentHp = 0, IsDead = true
[ ] OnDied 이벤트 발행
[ ] IsDead 후 TakeDamage 무시 (중복 사망 방지)
[ ] CurrentHp 하한 0
```

**EnemyAttackerTests.cs**
```
[ ] 초기 IsOnCooldown = false
[ ] FakeStrategy.Execute 호출 여부 (IEnemyAttackStrategy mock)
```

**CsvEnemyParserTests.cs**
```
[ ] 정상 12컬럼 파싱 → 필드 값 검증
[ ] 열 수 부족 → 경고 후 skip
[ ] 잘못된 EnemyType → 경고 후 skip
[ ] 잘못된 float → 경고 후 skip
[ ] 빈 줄·주석 무시
[ ] Repository.GetById 정상 조회
[ ] 없는 ID → null 반환
```
