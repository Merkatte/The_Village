using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Game.Core.Enums;
using Game.Dungeon.Spawn;

namespace Game.Tests.EditMode.Dungeon
{
    /// <summary>
    /// CsvSpawnPointParser 단위 테스트입니다.
    /// 파일 I/O 없이 CSV 문자열만으로 파싱 로직 전체를 검증합니다.
    /// </summary>
    [TestFixture]
    public class CsvSpawnPointParserTests
    {
        private CsvSpawnPointParser         _parser;
        private List<(int line, string msg)> _warnings;

        [SetUp]
        public void SetUp()
        {
            _warnings = new List<(int, string)>();
            _parser   = new CsvSpawnPointParser();
            _parser.OnWarning = (line, msg) => _warnings.Add((line, msg));
        }

        // ─ 정상 파싱 ───────────────────────────────────────────

        [Test]
        public void 정상_CSV_파싱시_SpawnPointData가_생성된다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Forest_001,-5.0,3.0,Wood,0.8,Tree\n";

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Spawn_Forest_001"), Is.True);
        }

        [Test]
        public void 같은_SpawnPointId_여러행은_하나의_지점으로_합산된다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Forest_001,-5.0,3.0,Wood,0.8,Tree\n" +
                      "Spawn_Forest_001,-5.0,3.0,Herb,0.3,HerbBush\n";

            var result = _parser.Parse(csv);

            Assert.That(result.Keys.Count,                           Is.EqualTo(1));
            Assert.That(result["Spawn_Forest_001"].Entries.Count,    Is.EqualTo(2));
        }

        [Test]
        public void 좌표가_첫번째_행_기준으로_설정된다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Cave_001,2.5,-1.5,Stone,1.0,Stone\n" +
                      "Spawn_Cave_001,2.5,-1.5,IronOre,0.4,IronVein\n";

            var result = _parser.Parse(csv);
            var data   = result["Spawn_Cave_001"];

            Assert.That(data.Position.x, Is.EqualTo(2.5f).Within(0.001f));
            Assert.That(data.Position.y, Is.EqualTo(-1.5f).Within(0.001f));
        }

        [Test]
        public void 파싱된_SpawnEntry_값이_올바르다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Mine_001,0.0,0.0,IronOre,0.75,IronVein\n";

            var result = _parser.Parse(csv);
            var entry  = result["Spawn_Mine_001"].Entries[0];

            Assert.That(entry.ResourceType, Is.EqualTo(ResourceType.IronOre));
            Assert.That(entry.SpawnChance,  Is.EqualTo(0.75f).Within(0.001f));
            Assert.That(entry.DropTableId,  Is.EqualTo("IronVein"));
        }

        [Test]
        public void 여러_SpawnPointId가_각각_독립적으로_생성된다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Forest_001,-5.0,3.0,Wood,0.8,Tree\n" +
                      "Spawn_Cave_001,2.5,-1.5,Stone,1.0,Stone\n" +
                      "Spawn_Cave_001,2.5,-1.5,IronOre,0.4,IronVein\n";

            var result = _parser.Parse(csv);

            Assert.That(result.Keys.Count,                        Is.EqualTo(2));
            Assert.That(result["Spawn_Forest_001"].Entries.Count, Is.EqualTo(1));
            Assert.That(result["Spawn_Cave_001"].Entries.Count,   Is.EqualTo(2));
        }

        [Test]
        public void SpawnPointId에_올바른_Position이_설정된다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Forest_001,-5.5,3.25,Wood,0.8,Tree\n";

            var result = _parser.Parse(csv);
            var pos    = result["Spawn_Forest_001"].Position;

            Assert.That(pos.x, Is.EqualTo(-5.5f).Within(0.001f));
            Assert.That(pos.y, Is.EqualTo(3.25f).Within(0.001f));
        }

        // ─ 헤더 / 빈 줄 / 주석 ───────────────────────────────

        [Test]
        public void 헤더행은_엔트리로_파싱되지_않는다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Forest_001,-5.0,3.0,Wood,0.8,Tree\n";

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("SpawnPointId"), Is.False);
        }

        [Test]
        public void 빈_줄은_무시된다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "\n" +
                      "Spawn_Forest_001,-5.0,3.0,Wood,0.8,Tree\n" +
                      "\n";

            var result = _parser.Parse(csv);

            Assert.That(result.Keys.Count, Is.EqualTo(1));
        }

        [Test]
        public void 샵으로_시작하는_줄은_주석으로_무시된다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "# 이건 주석입니다\n" +
                      "Spawn_Forest_001,-5.0,3.0,Wood,0.8,Tree\n";

            var result = _parser.Parse(csv);

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
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Forest_001,-5.0,3.0\n" +                       // 열 부족
                      "Spawn_Cave_001,2.5,-1.5,Stone,1.0,Stone\n";          // 정상

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Spawn_Forest_001"), Is.False);
            Assert.That(result.ContainsKey("Spawn_Cave_001"),   Is.True);
            Assert.That(_warnings.Count,                        Is.EqualTo(1));
        }

        [Test]
        public void X좌표가_숫자가_아니면_skip되고_경고가_발생한다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance\n" +
                      "Spawn_Forest_001,abc,3.0,Wood,0.8\n";

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Spawn_Forest_001"), Is.False);
            Assert.That(_warnings.Count,                        Is.EqualTo(1));
        }

        [Test]
        public void Y좌표가_숫자가_아니면_skip되고_경고가_발생한다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance\n" +
                      "Spawn_Forest_001,-5.0,abc,Wood,0.8\n";

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Spawn_Forest_001"), Is.False);
            Assert.That(_warnings.Count,                        Is.EqualTo(1));
        }

        [Test]
        public void 알수없는_ResourceType은_skip되고_경고가_발생한다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance\n" +
                      "Spawn_Forest_001,-5.0,3.0,Diamond,0.8\n";

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Spawn_Forest_001"), Is.False);
            Assert.That(_warnings.Count,                        Is.EqualTo(1));
        }

        [Test]
        public void SpawnChance가_숫자가_아니면_skip되고_경고가_발생한다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance\n" +
                      "Spawn_Forest_001,-5.0,3.0,Wood,높음\n";

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Spawn_Forest_001"), Is.False);
            Assert.That(_warnings.Count,                        Is.EqualTo(1));
        }

        [Test]
        public void 잘못된_행이_있어도_올바른_행은_정상_파싱된다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Forest_001,-5.0,3.0,INVALID,0.8,Tree\n" +     // 잘못된 ResourceType
                      "Spawn_Cave_001,2.5,-1.5,Stone,1.0,Stone\n" +         // 정상
                      "Spawn_Cave_001,2.5,-1.5,IronOre,0.4,IronVein\n";     // 정상

            var result = _parser.Parse(csv);

            Assert.That(result.ContainsKey("Spawn_Cave_001"),   Is.True);
            Assert.That(result["Spawn_Cave_001"].Entries.Count, Is.EqualTo(2));
            Assert.That(_warnings.Count,                        Is.EqualTo(1));
        }

        // ─ SpawnEntry 클램프 ──────────────────────────────────

        [Test]
        public void SpawnChance가_1초과이면_1로_클램프된다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Forest_001,-5.0,3.0,Wood,2.5,Tree\n";

            var result = _parser.Parse(csv);
            var chance = result["Spawn_Forest_001"].Entries[0].SpawnChance;

            Assert.That(chance, Is.EqualTo(1.0f).Within(0.001f));
        }

        [Test]
        public void SpawnChance가_음수이면_0으로_클램프된다()
        {
            var csv = "SpawnPointId,X,Y,ResourceType,SpawnChance,DropTableId\n" +
                      "Spawn_Forest_001,-5.0,3.0,Wood,-0.5,Tree\n";

            var result = _parser.Parse(csv);
            var chance = result["Spawn_Forest_001"].Entries[0].SpawnChance;

            Assert.That(chance, Is.EqualTo(0.0f).Within(0.001f));
        }
    }

    /// <summary>
    /// SpawnPointRepository 단위 테스트입니다.
    /// </summary>
    [TestFixture]
    public class SpawnPointRepositoryTests
    {
        private SpawnPointRepository CreateRepo()
        {
            var dict = new Dictionary<string, SpawnPointData>(
                System.StringComparer.OrdinalIgnoreCase)
            {
                ["Spawn_Forest_001"] = new SpawnPointData(
                    "Spawn_Forest_001",
                    new Vector2(-5f, 3f),
                    new List<SpawnEntry>
                    {
                        new SpawnEntry(ResourceType.Wood, 0.8f, "Tree"),
                        new SpawnEntry(ResourceType.Herb, 0.3f, "HerbBush"),
                    }),
                ["Spawn_Cave_001"] = new SpawnPointData(
                    "Spawn_Cave_001",
                    new Vector2(2.5f, -1.5f),
                    new List<SpawnEntry>
                    {
                        new SpawnEntry(ResourceType.Stone, 1.0f, "Stone"),
                    }),
            };
            return new SpawnPointRepository(dict);
        }

        [Test]
        public void 등록된_ID로_조회하면_데이터를_반환한다()
        {
            var repo = CreateRepo();
            var data = repo.GetById("Spawn_Forest_001");

            Assert.That(data,                  Is.Not.Null);
            Assert.That(data.Entries.Count,    Is.EqualTo(2));
        }

        [Test]
        public void 없는_ID로_조회하면_null을_반환한다()
        {
            var repo = CreateRepo();
            var data = repo.GetById("Spawn_NonExistent");

            Assert.That(data, Is.Null);
        }

        [Test]
        public void null_ID로_조회하면_null을_반환한다()
        {
            var repo = CreateRepo();
            var data = repo.GetById(null);

            Assert.That(data, Is.Null);
        }

        [Test]
        public void GetAll은_전체_스폰_지점을_반환한다()
        {
            var repo = CreateRepo();
            var all  = repo.GetAll();

            Assert.That(all.Count, Is.EqualTo(2));
        }

        [Test]
        public void null_딕셔너리_전달시_ArgumentNullException_발생()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new SpawnPointRepository(null));
        }
    }
}
