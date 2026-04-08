# AI Context Log

스크립트 작성 및 수정 내역을 기록합니다.
과거 이력은 `log_archive/` 폴더를 참조하세요.

---

## 로그 형식

```
### [YYYY-MM-DD] 작업 제목
- **신규**: `경로/파일명.cs` — 설명
- **수정**: `경로/파일명.cs` — 변경 내용 요약
- **삭제**: `경로/파일명.cs` — 삭제 이유
```

---

## 기록

### [2026-04-09] README 개발 현황 표현 정확하게 수정
- **수정**: `README.md` — "Complete" → "Implemented & verified"로 변경. 레벨 디자인/콘텐츠 미시작 항목 추가
- **수정**: `README.ko.md` — "완료" → "구현 및 동작 확인"으로 변경. 동일 내용 반영

### [2026-04-09] README 개발 현황 섹션 상세화
- **수정**: `README.md` — Project Status를 시스템별 완료·진행 중·미시작으로 세분화
- **수정**: `README.ko.md` — 동일 내용 한국어 버전 반영

### [2026-04-09] README 개발 현황 섹션 추가
- **수정**: `README.md` — 상단에 `in progress` 뱃지 추가, Project Status 섹션 추가
- **수정**: `README.ko.md` — 동일 내용 한국어 버전 반영

### [2026-04-09] README 개발 방식 표현 수정
- **수정**: `README.md` — 개발 방식 섹션 표현 수정. "멀티 에이전트 오케스트레이션 직접 설계·운용" → "Agent·Plan Mode·Skills 기능 활용한 AI 보조 개발 워크플로우 구성"으로 정확하게 변경
- **수정**: `README.ko.md` — 동일 내용 한국어 버전 반영

### [2026-04-08] Editor 유틸리티 스크립트 정리 + GitHub 배포 준비
- **삭제**: `Editor/CreateSampleDropTableCsv.cs` — 샘플 CSV 생성 유틸, 더 이상 불필요
- **삭제**: `Editor/CreateSampleSpawnPointsCsv.cs` — 샘플 CSV 생성 유틸, 더 이상 불필요
- **삭제**: `Editor/CreateSampleItemCatalogCsv.cs` — 샘플 CSV 생성 유틸, 더 이상 불필요
- **삭제**: `Editor/CreateSampleEnemyDataCsv.cs` — 샘플 CSV 생성 유틸, 더 이상 불필요
- **삭제**: `Editor/CreateTwoAssemblyDefinitions.cs` — 초기 asmdef 셋업 완료로 불필요
- **삭제**: `Editor/AddAsmdefReferences.cs` — 초기 asmdef 셋업 완료로 불필요
- **삭제**: `Editor/RecreateGameAsmdef.cs` — 초기 asmdef 셋업 완료로 불필요
- **삭제**: `Editor/RebuildTestAssemblyDefinition.cs` — 초기 asmdef 셋업 완료로 불필요
- **신규**: `.gitignore` — Unity 프로젝트용 git 제외 파일 목록
- **신규**: `README.md` — 영문 프로젝트 소개 (GitHub 배포용)
- **신규**: `README.ko.md` — 한국어 프로젝트 소개
- **신규**: `CLAUDE.md` — Claude Code 지침 파일
- **신규**: `.claude/skills/git_commit/SKILL.md` — git 커밋 자동화 스킬

### [2026-04-07] 적 AI 리팩토링 — EnemyWanderer 분리 + CSV 배회 데이터화
- **신규**: `Enemy/Movement/EnemyWanderer.cs` — 배회 이동 계산 순수 C# 클래스. `GetMoveDirection(pos, dt)` / `ResetTimer()`
- **수정**: `Enemy/Core/EnemyData.cs` — `WanderRadius`, `WanderInterval` 필드 추가 (기본값 파라미터)
- **수정**: `Enemy/Data/CsvEnemyParser.cs` — 12열 → 14열. `WanderRadius`, `WanderInterval` 파싱 추가
- **수정**: `Resources/Data/EnemyData.csv` — `WanderRadius`, `WanderInterval` 컬럼 추가
- **수정**: `Enemy/EnemyController.cs` — `EnemyWanderer` 사용으로 리팩토링. `_playerHealth == null` 시에도 배회 동작. `ApplyTransformPosition()` 메서드 추출
- **수정**: `Editor/CreateSampleEnemyDataCsv.cs` — 샘플 CSV 14열로 업데이트
- **수정**: `Tests/EditMode/Enemy/CsvEnemyParserTests.cs` — 14열 기준으로 테스트 데이터 업데이트. WanderRadius·WanderInterval 필드 검증 추가

### [2026-04-07] 적 AI FSM 개선 — 배회·탐색·이탈 로직 추가
- **수정**: `Enemy/Core/EnemyBrain.cs` — 우선순위 방식 → FSM 방식으로 전환. Combat→Alert(플레이어 이탈 시), Alert→Idle(AlertRange 이탈 시) 전환 추가
- **수정**: `Enemy/EnemyController.cs` — 스폰 위치 기록(`_spawnPos`), 마지막 목격 위치 추적(`_lastKnownPlayerPos`), Idle 배회 로직(`UpdateWander`), Alert 탐색 이동 분리
- **수정**: `Tests/EditMode/Enemy/EnemyBrainTests.cs` — FSM 전환 테스트 3개 추가 (Combat→Alert, Alert→Idle, Idle에서 즉시 Attacking 불가)

### [2026-04-07] 플레이어 사망 처리 시스템 구현
- **수정**: `UI/Core/PopupType.cs` — `PlayerDeath` 항목 추가
- **수정**: `UI/Core/PopupBase.cs` — `public virtual bool IsEscClosable => true;` 프로퍼티 추가
- **수정**: `UI/Core/UIManager.cs` — `CloseTopPopup()`에서 `IsEscClosable` 체크 추가. ESC로 닫을 수 없는 팝업 지원
- **신규**: `UI/PlayerDeath/PlayerDeathPopup.cs` — `PopupBase` 상속. `IsEscClosable = false`. "마을로 돌아가기" 버튼 → `TransitionToWithLoading(Town)`
- **신규**: `Dungeon/PlayerDeathHandler.cs` — MonoBehaviour 브릿지. `Start()`에서 `PlayerHealth.OnDied` 구독 → `IUIManager.Open(PlayerDeath)` 호출

### [2026-04-05] AI_Context 파일 구조 분리
- **신규**: `AI_Context/architecture_index.md` — 아키텍처 경량 인덱스 (모듈 목록 + 의존성 흐름 + 미구현 현황)
- **신규**: `AI_Context/architecture_core_manager.md` — Core · Manager · Currency 상세
- **신규**: `AI_Context/architecture_gameplay.md` — Player · Camera · Loading · Dungeon · Graphic · Data 상세
- **신규**: `AI_Context/architecture_enemy.md` — Enemy 시스템 상세
- **신규**: `AI_Context/architecture_item_shop.md` — Item · Inventory · Shop 상세
- **신규**: `AI_Context/architecture_ui.md` — UI · Editor · Tests 상세
- **신규**: `AI_Context/log_archive/2026_03.md` — 2026년 3월 이력 아카이브
- **수정**: `AI_Context/log.md` — 최근 이력만 유지, 아카이브 구조 도입
- **수정**: `AI_Context/architecture.md` — 분리 완료 후 레거시 파일 (참조는 architecture_index.md 사용)

### [2026-04-02] 적 AI 시스템 Phase 5 구현 — 씬 연결
- **신규**: `Data/Config/EnemyPrefabConfig.cs` — ScriptableObject. `EnemyType → GameObject` 프리팹 매핑. `GetPrefab(EnemyType)`
- **신규**: `Data/Config/EnemySpawnConfig.cs` — ScriptableObject. `EnemySpawnEntry[]` 목록 (EnemyId, Position, SpawnChance)
- **신규**: `Enemy/EnemySpawner.cs` — MonoBehaviour. `InitAsync()` 에서 EnemySpawnConfig 순회 → 확률 판정 → EnemyPrefabConfig 프리팹 Instantiate. 항목당 UniTask.Yield()로 프레임 분산
- **수정**: `Enemy/Data/EnemyLoader.cs` — `LoadFromText(string)` 오버로드 추가
- **수정**: `Dungeon/Scene/DungeonSceneInitializer.cs` — `[Header("적 AI")]` 섹션 추가. Step 7에서 `enemySpawner.InitAsync()` 호출. `IEnemyRepository` 해제 추가

### [2026-04-02] 적 AI 시스템 Phase 4 구현 — MonoBehaviour 브릿지
- **신규**: `Player/PlayerHealth.cs` — `IDamageable` + MonoBehaviour. `Awake` 에서 `ServiceLocator.Register<PlayerHealth>(this)`
- **수정**: `Player/PlayerController.cs` — `playerHealth.IsDead` 시 이동 중단 가드 추가
- **신규**: `Enemy/EnemyController.cs` — MonoBehaviour 브릿지. Brain·Mover·Health·Attacker 구성. HandleDeath: CancellationToken 취소 + 0.5초 후 파괴

### [2026-04-02] 적 AI 시스템 Phase 3 구현 — CSV 데이터 파이프라인
- **신규**: `Enemy/Data/IEnemyRepository.cs` / `Enemy/Data/EnemyRepository.cs` / `Enemy/Data/CsvEnemyParser.cs` / `Enemy/Data/EnemyLoader.cs`
- **신규**: `Data/Config/EnemyCsvConfig.cs`
- **신규**: `Resources/Data/EnemyData.csv` — 샘플 데이터 3종 (slime_basic, goblin_warrior, orc_grunt)
- **신규**: `Tests/EditMode/Enemy/CsvEnemyParserTests.cs` — 13개 케이스

### [2026-04-01] 적 AI 시스템 Phase 1·2 구현
- **신규**: `Core/Enums/EnemyState.cs` / `Core/Enums/EnemyType.cs` / `Core/Enums/AttackType.cs`
- **신규**: `Enemy/Core/EnemyData.cs` / `Enemy/Core/EnemyBrain.cs`
- **신규**: `Enemy/Movement/EnemyMover.cs` / `Enemy/Health/EnemyHealth.cs`
- **신규**: `Enemy/Combat/IEnemyAttackStrategy.cs` / `Enemy/Combat/MeleeAttackStrategy.cs` / `Enemy/Combat/EnemyAttacker.cs`
- **신규**: `Tests/EditMode/Enemy/EnemyBrainTests.cs` (8개) / `EnemyMoverTests.cs` (9개) / `EnemyHealthTests.cs` (8개) / `EnemyAttackerTests.cs` (4개)

---

## 아카이브

| 기간 | 파일 |
|------|------|
| 2026년 3월 | `log_archive/2026_03.md` |
