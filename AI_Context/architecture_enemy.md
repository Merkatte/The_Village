# 아키텍처 — Enemy (적 AI 시스템)

> 최종 업데이트: 2026-04-07

---

## Enemy — 적 AI 시스템

### Core (순수 C# 로직)
| 파일 | 설명 |
|------|------|
| `Enemy/Core/EnemyData.cs` | CSV 에서 로드되는 불변 수치 데이터. 14개 필드 (EnemyId, EnemyType, MaxHp, MoveSpeed, AlertRange, CombatRange, AttackRange, AttackDamage, AttackCooldown, PreAttackDelay, PostAttackStun, AttackType, WanderRadius, WanderInterval). 순수 C#, EditMode 테스트 가능 |
| `Enemy/Core/EnemyBrain.cs` | FSM 상태 전환 판단. `Tick(EnemyState, float dist, bool isAttacking) → EnemyState`. 현재 상태 기반 switch 방식. Dead 최우선. 순수 C# |
| `Enemy/EnemyController.cs` | MonoBehaviour 브릿지. `Awake`에서 IEnemyRepository로 데이터 로드 → Brain·Mover·Health·Attacker 구성. `Start`에서 PlayerHealth 획득. `Update`에서 Brain.Tick → 상태별 행동(이동/공격). `HandleDeath`에서 Dead 전환·CTS 취소·0.5초 후 파괴 |
| `Enemy/EnemySpawner.cs` | MonoBehaviour. `InitAsync()` 에서 EnemySpawnConfig 순회 → 확률 판정 → EnemyPrefabConfig 프리팹 Instantiate. 항목당 UniTask.Yield() 로 프레임 분산 |

### Movement
| 파일 | 설명 |
|------|------|
| `Enemy/Movement/EnemyMover.cs` | `IMovable` 구현. PlayerMover 동일 구조 (sealed). 이동 벡터 계산만 담당 |
| `Enemy/Movement/EnemyWanderer.cs` | 순수 C# 배회 이동 계산. `GetMoveDirection(Vector2 pos, float dt)` → 이동 방향 반환. `ResetTimer()` 로 즉시 재개. 스폰 위치 기준 WanderRadius 범위 내 랜덤 목적지 선정 |

### Health
| 파일 | 설명 |
|------|------|
| `Enemy/Health/EnemyHealth.cs` | `IDamageable` 구현 (sealed). TakeDamage·OnDied 이벤트·중복 사망 방지·CurrentHp 하한 0 |

### Combat
| 파일 | 설명 |
|------|------|
| `Enemy/Combat/IEnemyAttackStrategy.cs` | 공격 방식 추상화 인터페이스. `Execute(IDamageable target)` |
| `Enemy/Combat/MeleeAttackStrategy.cs` | 근접 공격. 생성자에서 `_damage` 주입, `target.TakeDamage()` 직접 호출 (sealed) |
| `Enemy/Combat/RangedAttackStrategy.cs` | *(Phase 6 예정)* 원거리 투사체 공격 |
| `Enemy/Combat/EnemyAttacker.cs` | UniTask 공격 시퀀스 관리 (sealed). ①선딜 ②Execute ③후딜 ④쿨타임. `IsOnCooldown` 플래그. `CancellationToken` 주입 |

### Data
| 파일 | 설명 |
|------|------|
| `Enemy/Data/IEnemyRepository.cs` | `GetById(string) → EnemyData` / `GetAll()` 저장소 인터페이스 |
| `Enemy/Data/EnemyRepository.cs` | `Dictionary<string, EnemyData>` 기반 구현체. EnemyId 대소문자 무시 (OrdinalIgnoreCase) |
| `Enemy/Data/CsvEnemyParser.cs` | `CsvReader` 기반 14열 필수 파서. 빈 ID·MaxHp≤0·알 수 없는 Enum은 skip + 경고. `OnWarning` 콜백 지원 |
| `Enemy/Data/EnemyLoader.cs` | `Load(TextAsset)` + `LoadFromText(string)` 제공. 후자는 백그라운드 스레드에서 호출 가능 |

---

## 상태 전환 규칙 (FSM)

```
Idle      → Alert     : 플레이어가 AlertRange 이내 진입
Alert     → Combat    : 플레이어가 CombatRange 이내 진입
Alert     → Idle      : 플레이어가 AlertRange 밖으로 이탈
Combat    → Attacking : 플레이어가 AttackRange 이내 진입
Combat    → Alert     : 플레이어가 CombatRange 밖으로 이탈 (놓침)
Attacking → Combat    : 플레이어가 AttackRange 밖으로 이탈
```

## 상태별 행동 (EnemyController)

```
Idle      : EnemyWanderer.GetMoveDirection() 사용. 스폰 위치 기준 WanderRadius 범위 내 배회
            PlayerHealth == null 이어도 배회 동작 유지
Alert     : 마지막 목격 위치(_lastKnownPlayerPos)로 이동 (탐색)
Combat    : 플레이어 직접 추적. 매 프레임 _lastKnownPlayerPos 갱신
Attacking : 정지 + 공격 시퀀스
```

## UniTask 사용 범위

공격 시퀀스(선딜→피격→후딜→쿨타임) 4구간에만 한정
