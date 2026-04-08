using System.Collections.Generic;
using NUnit.Framework;
using Game.Core.Csv;
using Game.Core.Enums;

namespace Game.Tests.EditMode.Core
{
    /// <summary>
    /// CsvReader 단위 테스트입니다.
    /// 공통 인프라 (헤더 skip, 빈 줄, 주석, 파싱 헬퍼) 를 검증합니다.
    /// </summary>
    [TestFixture]
    public class CsvReaderTests
    {
        private CsvReader                    _reader;
        private List<(int line, string msg)> _warnings;
        private List<string[]>               _rows;

        [SetUp]
        public void SetUp()
        {
            _warnings = new List<(int, string)>();
            _rows     = new List<string[]>();
            _reader   = new CsvReader();
            _reader.OnWarning = (line, msg) => _warnings.Add((line, msg));
        }

        // ─ Read 기본 동작 ───────────────────────────────────────

        [Test]
        public void 헤더행은_콜백에_전달되지_않는다()
        {
            var csv = "Col1,Col2,Col3\n" +
                      "A,B,C\n";

            _reader.Read(csv, (cols, _) => _rows.Add(cols));

            Assert.That(_rows.Count, Is.EqualTo(1));
            Assert.That(_rows[0][0], Is.EqualTo("A"));
        }

        [Test]
        public void 빈줄은_콜백에_전달되지_않는다()
        {
            var csv = "Col1\n\nA\n\nB\n";

            _reader.Read(csv, (cols, _) => _rows.Add(cols));

            Assert.That(_rows.Count, Is.EqualTo(2));
        }

        [Test]
        public void 샵_주석줄은_콜백에_전달되지_않는다()
        {
            var csv = "Col1\n" +
                      "# 주석입니다\n" +
                      "A\n";

            _reader.Read(csv, (cols, _) => _rows.Add(cols));

            Assert.That(_rows.Count, Is.EqualTo(1));
        }

        [Test]
        public void null_csvText는_콜백을_호출하지_않는다()
        {
            _reader.Read(null, (cols, _) => _rows.Add(cols));
            Assert.That(_rows.Count, Is.EqualTo(0));
        }

        [Test]
        public void 빈_csvText는_콜백을_호출하지_않는다()
        {
            _reader.Read("", (cols, _) => _rows.Add(cols));
            Assert.That(_rows.Count, Is.EqualTo(0));
        }

        // ─ RequireColumns ──────────────────────────────────────

        [Test]
        public void RequireColumns_충분하면_true를_반환한다()
        {
            var result = _reader.RequireColumns(new[] { "a", "b", "c" }, 1, 3);
            Assert.That(result, Is.True);
        }

        [Test]
        public void RequireColumns_부족하면_false_및_경고를_반환한다()
        {
            var result = _reader.RequireColumns(new[] { "a", "b" }, 1, 5);
            Assert.That(result,          Is.False);
            Assert.That(_warnings.Count, Is.EqualTo(1));
        }

        // ─ TryParseFloat ───────────────────────────────────────

        [Test]
        public void TryParseFloat_정상값은_true를_반환한다()
        {
            var ok = _reader.TryParseFloat("0.75", 1, "X", out var value);
            Assert.That(ok,    Is.True);
            Assert.That(value, Is.EqualTo(0.75f).Within(0.001f));
        }

        [Test]
        public void TryParseFloat_비정상값은_false_및_경고를_반환한다()
        {
            var ok = _reader.TryParseFloat("abc", 1, "X", out _);
            Assert.That(ok,              Is.False);
            Assert.That(_warnings.Count, Is.EqualTo(1));
        }

        // ─ TryParseInt ─────────────────────────────────────────

        [Test]
        public void TryParseInt_정상값은_true를_반환한다()
        {
            var ok = _reader.TryParseInt("42", 1, "Count", out var value);
            Assert.That(ok,    Is.True);
            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public void TryParseInt_비정상값은_false_및_경고를_반환한다()
        {
            var ok = _reader.TryParseInt("xyz", 1, "Count", out _);
            Assert.That(ok,              Is.False);
            Assert.That(_warnings.Count, Is.EqualTo(1));
        }

        // ─ TryParseEnum ────────────────────────────────────────

        [Test]
        public void TryParseEnum_정상값은_true를_반환한다()
        {
            var ok = _reader.TryParseEnum<ResourceType>("Wood", 1, "ResourceType", out var value);
            Assert.That(ok,    Is.True);
            Assert.That(value, Is.EqualTo(ResourceType.Wood));
        }

        [Test]
        public void TryParseEnum_대소문자_무관하게_파싱된다()
        {
            var ok = _reader.TryParseEnum<ResourceType>("WOOD", 1, "ResourceType", out var value);
            Assert.That(ok,    Is.True);
            Assert.That(value, Is.EqualTo(ResourceType.Wood));
        }

        [Test]
        public void TryParseEnum_없는값은_false_및_경고를_반환한다()
        {
            var ok = _reader.TryParseEnum<ResourceType>("Diamond", 1, "ResourceType", out _);
            Assert.That(ok,              Is.False);
            Assert.That(_warnings.Count, Is.EqualTo(1));
        }
    }
}
