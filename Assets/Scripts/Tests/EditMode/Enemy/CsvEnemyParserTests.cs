using System.Collections.Generic;
using NUnit.Framework;
using Game.Core.Enums;
using Game.Enemy.Data;

namespace Game.Tests.EditMode.Enemy
{
    /// <summary>
    /// CsvEnemyParser 및 EnemyRepository 의 단위 테스트입니다.
    ///
    /// ─ TDD 체크리스트 ─────────────────────────────────────────
    ///   [v] 정상 행 파싱 — 모든 필드 값 검증
    ///   [v] 다중 행 파싱 — 개수 검증
    ///   [v] 빈 문자열 → 0개 반환
    ///   [v] 헤더만 있는 CSV → 0개 반환
    ///   [v] 열 수 부족 행 → skip 후 경고 콜백 호출
    ///   [v] 빈 EnemyId 행 → skip 후 경고 콜백 호출
    ///   [v] MaxHp 0 이하 → skip 후 경고 콜백 호출
    ///   [v] 알 수 없는 EnemyType → skip 후 경고 콜백 호출
    ///   [v] # 주석 줄 → 건너뜀
    ///   [v] EnemyRepository.GetById 정상 조회
    ///   [v] EnemyRepository.GetById 대소문자 무시
    ///   [v] EnemyRepository.GetById 없는 ID → null
    ///   [v] EnemyRepository.GetAll 개수 검증
    /// </summary>
    [TestFixture]
    public class CsvEnemyParserTests
    {
        private const string ValidHeader =
            "EnemyId,EnemyType,MaxHp,MoveSpeed,AlertRange,CombatRange,AttackRange," +
            "AttackDamage,AttackCooldown,PreAttackDelay,PostAttackStun,AttackType," +
            "WanderRadius,WanderInterval";

        private const string SlimeRow =
            "slime_basic,Slime,30,2,10,6,2,5,2,0.2,0.4,Melee,3,3";

        private const string GoblinRow =
            "goblin_warrior,Goblin,60,3,12,8,2.5,12,1.8,0.3,0.5,Melee,4,2";

        // ─ 정상 파싱 ──────────────────────────────────────────

        [Test]
        [Description("정상 행을 파싱하면 모든 필드가 올바르게 채워져야 한다.")]
        public void 정상_행_파싱시_모든_필드가_올바르다()
        {
            var csv = $"{ValidHeader}\n{SlimeRow}";
            var entries = new CsvEnemyParser().Parse(csv);

            Assert.AreEqual(1, entries.Count);
            var e = entries[0];
            Assert.AreEqual("slime_basic",   e.EnemyId);
            Assert.AreEqual(EnemyType.Slime, e.EnemyType);
            Assert.AreEqual(30,              e.MaxHp);
            Assert.AreEqual(2f,              e.MoveSpeed,      delta: 0.001f);
            Assert.AreEqual(10f,             e.AlertRange,     delta: 0.001f);
            Assert.AreEqual(6f,              e.CombatRange,    delta: 0.001f);
            Assert.AreEqual(2f,              e.AttackRange,    delta: 0.001f);
            Assert.AreEqual(5,               e.AttackDamage);
            Assert.AreEqual(2f,              e.AttackCooldown, delta: 0.001f);
            Assert.AreEqual(0.2f,            e.PreAttackDelay, delta: 0.001f);
            Assert.AreEqual(0.4f,            e.PostAttackStun, delta: 0.001f);
            Assert.AreEqual(AttackType.Melee, e.AttackType);
            Assert.AreEqual(3f,               e.WanderRadius,   delta: 0.001f);
            Assert.AreEqual(3f,               e.WanderInterval, delta: 0.001f);
        }

        [Test]
        [Description("여러 행을 파싱하면 모두 반환되어야 한다.")]
        public void 다중_행_파싱시_모두_반환된다()
        {
            var csv = $"{ValidHeader}\n{SlimeRow}\n{GoblinRow}";
            var entries = new CsvEnemyParser().Parse(csv);

            Assert.AreEqual(2, entries.Count);
        }

        // ─ 빈 입력 ────────────────────────────────────────────

        [Test]
        [Description("빈 문자열을 파싱하면 0개를 반환해야 한다.")]
        public void 빈_문자열_파싱시_0개_반환()
        {
            var entries = new CsvEnemyParser().Parse(string.Empty);

            Assert.AreEqual(0, entries.Count);
        }

        [Test]
        [Description("헤더만 있는 CSV 를 파싱하면 0개를 반환해야 한다.")]
        public void 헤더만_있는_CSV_파싱시_0개_반환()
        {
            var entries = new CsvEnemyParser().Parse(ValidHeader);

            Assert.AreEqual(0, entries.Count);
        }

        // ─ 오류 행 skip ───────────────────────────────────────

        [Test]
        [Description("열 수가 부족한 행은 skip 하고 경고 콜백을 호출해야 한다.")]
        public void 열수_부족_행은_skip되고_경고_콜백_호출()
        {
            var warnings = new List<string>();
            var parser   = new CsvEnemyParser();
            parser.OnWarning = (_, msg) => warnings.Add(msg);

            var csv = $"{ValidHeader}\nslime_basic,Slime,30";
            var entries = parser.Parse(csv);

            Assert.AreEqual(0, entries.Count);
            Assert.IsTrue(warnings.Count > 0);
        }

        [Test]
        [Description("EnemyId 가 비어있는 행은 skip 하고 경고 콜백을 호출해야 한다.")]
        public void 빈_EnemyId_행은_skip되고_경고_콜백_호출()
        {
            var warnings = new List<string>();
            var parser   = new CsvEnemyParser();
            parser.OnWarning = (_, msg) => warnings.Add(msg);

            var csv = $"{ValidHeader}\n,Slime,30,2,10,6,2,5,2,0.2,0.4,Melee,3,3";
            var entries = parser.Parse(csv);

            Assert.AreEqual(0, entries.Count);
            Assert.IsTrue(warnings.Count > 0);
        }

        [Test]
        [Description("MaxHp 가 0 이하인 행은 skip 하고 경고 콜백을 호출해야 한다.")]
        public void MaxHp_0이하_행은_skip되고_경고_콜백_호출()
        {
            var warnings = new List<string>();
            var parser   = new CsvEnemyParser();
            parser.OnWarning = (_, msg) => warnings.Add(msg);

            var csv = $"{ValidHeader}\nslime_basic,Slime,0,2,10,6,2,5,2,0.2,0.4,Melee,3,3";
            var entries = parser.Parse(csv);

            Assert.AreEqual(0, entries.Count);
            Assert.IsTrue(warnings.Count > 0);
        }

        [Test]
        [Description("알 수 없는 EnemyType 이 있는 행은 skip 하고 경고 콜백을 호출해야 한다.")]
        public void 알수없는_EnemyType_행은_skip되고_경고_콜백_호출()
        {
            var warnings = new List<string>();
            var parser   = new CsvEnemyParser();
            parser.OnWarning = (_, msg) => warnings.Add(msg);

            var csv = $"{ValidHeader}\nslime_basic,Unknown,30,2,10,6,2,5,2,0.2,0.4,Melee,3,3";
            var entries = parser.Parse(csv);

            Assert.AreEqual(0, entries.Count);
            Assert.IsTrue(warnings.Count > 0);
        }

        [Test]
        [Description("# 주석 줄은 파싱 결과에 포함되지 않아야 한다.")]
        public void 주석_줄은_결과에_포함되지_않는다()
        {
            var csv = $"{ValidHeader}\n# 이것은 주석입니다\n{SlimeRow}";
            var entries = new CsvEnemyParser().Parse(csv);

            Assert.AreEqual(1, entries.Count);
        }

        // ─ EnemyRepository ────────────────────────────────────

        [Test]
        [Description("EnemyRepository.GetById 는 EnemyId 로 올바른 데이터를 반환해야 한다.")]
        public void GetById_정상_조회()
        {
            var csv  = $"{ValidHeader}\n{SlimeRow}";
            var repo = new EnemyRepository(new CsvEnemyParser().Parse(csv));

            var data = repo.GetById("slime_basic");

            Assert.IsNotNull(data);
            Assert.AreEqual("slime_basic", data.EnemyId);
        }

        [Test]
        [Description("EnemyRepository.GetById 는 대소문자를 무시해야 한다.")]
        public void GetById_대소문자_무시()
        {
            var csv  = $"{ValidHeader}\n{SlimeRow}";
            var repo = new EnemyRepository(new CsvEnemyParser().Parse(csv));

            Assert.IsNotNull(repo.GetById("SLIME_BASIC"));
            Assert.IsNotNull(repo.GetById("Slime_Basic"));
        }

        [Test]
        [Description("EnemyRepository.GetById 는 존재하지 않는 ID 에 대해 null 을 반환해야 한다.")]
        public void GetById_없는_ID는_null_반환()
        {
            var csv  = $"{ValidHeader}\n{SlimeRow}";
            var repo = new EnemyRepository(new CsvEnemyParser().Parse(csv));

            Assert.IsNull(repo.GetById("nonexistent"));
        }

        [Test]
        [Description("EnemyRepository.GetAll 은 파싱된 모든 데이터를 반환해야 한다.")]
        public void GetAll_전체_데이터_반환()
        {
            var csv  = $"{ValidHeader}\n{SlimeRow}\n{GoblinRow}";
            var repo = new EnemyRepository(new CsvEnemyParser().Parse(csv));

            Assert.AreEqual(2, repo.GetAll().Count);
        }
    }
}
