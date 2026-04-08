using System.Collections.Generic;
using NUnit.Framework;
using Game.Core.Enums;
using Game.Dungeon.Resource;

namespace Game.Tests.EditMode.Dungeon
{
    /// <summary>
    /// CsvDropTableParser 단위 테스트입니다.
    /// 파일 I/O 없이 CSV 문자열만으로 파싱 로직 전체를 검증합니다.
    /// </summary>
    [TestFixture]
    public class CsvDropTableParserTests
    {
        private CsvDropTableParser _parser;
        private List<(int line, string msg)> _warnings;

        [SetUp]
        public void SetUp()
        {
            _warnings = new List<(int, string)>();
            _parser   = new CsvDropTableParser();
            _parser.OnWarning = (line, msg) => _warnings.Add((line, msg));
        }

        // ─ 정상 파싱 ───────────────────────────────────────────

        [Test]
        public void 정상_CSV_파싱시_sourceId별_엔트리를_반환한다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "Tree,Wood,0.9,1,3\n" +
                      "Tree,Herb,0.3,1,1\n";

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Tree"), Is.True);
            Assert.That(result["Tree"].Count, Is.EqualTo(2));
        }

        [Test]
        public void 여러_sourceId가_있으면_각각_그룹화된다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "Tree,Wood,0.9,1,3\n" +
                      "Stone,Stone,1.0,2,4\n" +
                      "Stone,IronOre,0.2,1,1\n";

            var result = _parser.Parse(csv);

            Assert.That(result.Keys.Count, Is.EqualTo(2));
            Assert.That(result["Tree"].Count,  Is.EqualTo(1));
            Assert.That(result["Stone"].Count, Is.EqualTo(2));
        }

        [Test]
        public void 파싱된_DropEntry_값이_올바르다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "GoblinSlime,Bone,0.8,1,2\n";

            var result = _parser.Parse(csv);
            var entry  = result["GoblinSlime"][0];

            Assert.That(entry.ResourceType, Is.EqualTo(ResourceType.Bone));
            Assert.That(entry.DropChance,   Is.EqualTo(0.8f).Within(0.001f));
            Assert.That(entry.MinAmount,    Is.EqualTo(1));
            Assert.That(entry.MaxAmount,    Is.EqualTo(2));
        }

        [Test]
        public void 소수점_DropChance가_올바르게_파싱된다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "Tree,Wood,0.75,1,3\n";

            var result = _parser.Parse(csv);

            Assert.That(result["Tree"][0].DropChance, Is.EqualTo(0.75f).Within(0.001f));
        }

        // ─ 헤더 / 빈 줄 / 주석 처리 ──────────────────────────

        [Test]
        public void 헤더행은_엔트리로_파싱되지_않는다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "Tree,Wood,0.9,1,3\n";

            var result = _parser.Parse(csv);

            // 헤더가 sourceId 로 등록되지 않아야 함
            Assert.That(result.ContainsKey("SourceId"), Is.False);
        }

        [Test]
        public void 빈줄은_무시된다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "\n" +
                      "Tree,Wood,0.9,1,3\n" +
                      "\n";

            var result = _parser.Parse(csv);

            Assert.That(result["Tree"].Count, Is.EqualTo(1));
        }

        [Test]
        public void 샵으로_시작하는_줄은_주석으로_무시된다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "# 이건 주석입니다\n" +
                      "Tree,Wood,0.9,1,3\n";

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Tree"), Is.True);
            Assert.That(result.Keys.Count, Is.EqualTo(1));
        }

        [Test]
        public void 빈_CSV이면_빈_딕셔너리를_반환한다()
        {
            var result = _parser.Parse("");
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void null_CSV이면_빈_딕셔너리를_반환한다()
        {
            var result = _parser.Parse(null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        // ─ 잘못된 행 처리 (skip + warning) ────────────────────

        [Test]
        public void 열수가_부족한_행은_skip되고_경고가_발생한다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "Tree,Wood,0.9\n" +       // 열 부족
                      "Stone,Stone,1.0,2,4\n";  // 정상

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Tree"),  Is.False);
            Assert.That(result.ContainsKey("Stone"), Is.True);
            Assert.That(_warnings.Count, Is.EqualTo(1));
        }

        [Test]
        public void 알수없는_ResourceType은_skip되고_경고가_발생한다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "Tree,Diamond,0.9,1,3\n"; // 없는 타입

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Tree"), Is.False);
            Assert.That(_warnings.Count, Is.EqualTo(1));
        }

        [Test]
        public void DropChance가_숫자가_아니면_skip되고_경고가_발생한다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "Tree,Wood,높음,1,3\n"; // 숫자 아님

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Tree"), Is.False);
            Assert.That(_warnings.Count, Is.EqualTo(1));
        }

        [Test]
        public void 잘못된_행이_있어도_올바른_행은_정상_파싱된다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "Tree,INVALID_TYPE,0.9,1,3\n" +   // 잘못된 행
                      "Stone,Stone,1.0,2,4\n" +          // 정상
                      "Stone,IronOre,0.2,1,1\n";         // 정상

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Stone"), Is.True);
            Assert.That(result["Stone"].Count, Is.EqualTo(2));
            Assert.That(_warnings.Count, Is.EqualTo(1));
        }

        // ─ sourceId 대소문자 구분 없음 ────────────────────────

        [Test]
        public void sourceId는_대소문자를_구분하지_않는다()
        {
            var csv = "SourceId,ResourceType,DropChance,MinAmount,MaxAmount\n" +
                      "tree,Wood,0.9,1,3\n" +
                      "TREE,Herb,0.3,1,1\n";

            var result = _parser.Parse(csv);

            // 대소문자 무관하게 같은 키로 그룹화
            Assert.That(result.Keys.Count, Is.EqualTo(1));
            Assert.That(result["tree"].Count, Is.EqualTo(2));
        }
    }

    /// <summary>
    /// DropTableRepository 단위 테스트입니다.
    /// </summary>
    [TestFixture]
    public class DropTableRepositoryTests
    {
        private DropTableRepository CreateRepo()
        {
            var dict = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<DropEntry>>(
                System.StringComparer.OrdinalIgnoreCase)
            {
                ["Tree"]  = new System.Collections.Generic.List<DropEntry>
                {
                    new DropEntry(ResourceType.Wood, 0.9f, 1, 3),
                    new DropEntry(ResourceType.Herb, 0.3f, 1, 1),
                },
                ["Stone"] = new System.Collections.Generic.List<DropEntry>
                {
                    new DropEntry(ResourceType.Stone, 1.0f, 2, 4),
                }
            };
            return new DropTableRepository(dict);
        }

        [Test]
        public void 등록된_sourceId로_조회하면_IDropTable을_반환한다()
        {
            var repo = CreateRepo();

            var table = repo.GetTable("Tree");

            Assert.That(table, Is.Not.Null);
        }

        [Test]
        public void 없는_sourceId로_조회하면_null을_반환한다()
        {
            var repo = CreateRepo();

            var table = repo.GetTable("Dragon");

            Assert.That(table, Is.Null);
        }

        [Test]
        public void null_sourceId로_조회하면_null을_반환한다()
        {
            var repo = CreateRepo();

            var table = repo.GetTable(null);

            Assert.That(table, Is.Null);
        }

        [Test]
        public void GetAllSourceIds는_모든_키를_반환한다()
        {
            var repo = CreateRepo();

            var ids = repo.GetAllSourceIds();

            Assert.That(ids.Count, Is.EqualTo(2));
            Assert.That(ids, Does.Contain("Tree"));
            Assert.That(ids, Does.Contain("Stone"));
        }

        [Test]
        public void null_딕셔너리_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new DropTableRepository(null));
        }
    }
}
