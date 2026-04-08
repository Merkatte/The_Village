using System;
using System.Collections.Generic;
using System.Globalization;

namespace Game.Core.Csv
{
    /// <summary>
    /// CSV 파일의 공통 파싱 인프라를 제공하는 유틸리티 클래스입니다.
    ///
    /// ─ 담당 역할 (이것만 합니다) ──────────────────────────────
    ///   - 줄 단위 분리
    ///   - 첫 번째 데이터 행(헤더) 건너뜀
    ///   - 빈 줄 / 공백 줄 건너뜀
    ///   - '#' 으로 시작하는 주석 줄 건너뜀
    ///   - 경고 콜백 (행 번호 + 메시지)
    ///   - float / int 파싱 헬퍼 (InvariantCulture 고정)
    ///
    /// ─ 담당하지 않는 역할 ─────────────────────────────────────
    ///   - 컬럼 의미 해석 (각 도메인 파서가 담당)
    ///   - 반환 타입 결정 (각 도메인 파서가 담당)
    ///
    /// ─ 사용법 ────────────────────────────────────────────────
    ///   var reader = new CsvReader();
    ///   reader.OnWarning = (line, msg) => Debug.LogWarning(...);
    ///   reader.Read(csvText, (cols, lineNum) =>
    ///   {
    ///       // cols[0], cols[1] ... 사용
    ///   });
    /// </summary>
    public sealed class CsvReader
    {
        // ─ 공개 API ────────────────────────────────────────────

        /// <summary>파싱 경고 발생 시 호출되는 콜백 (행 번호, 메시지)</summary>
        public Action<int, string> OnWarning { get; set; }

        /// <summary>
        /// CSV 텍스트를 읽어 각 데이터 행을 콜백으로 전달합니다.
        /// 헤더·빈 줄·주석은 자동으로 건너뜁니다.
        /// </summary>
        /// <param name="csvText">CSV 파일 전체 텍스트</param>
        /// <param name="onRow">데이터 행 콜백 (컬럼 배열, 행 번호)</param>
        public void Read(string csvText, Action<string[], int> onRow)
        {
            if (string.IsNullOrWhiteSpace(csvText) || onRow == null)
                return;

            var lines    = csvText.Split('\n');
            var lineNum  = 0;
            var dataRows = 0;

            foreach (var rawLine in lines)
            {
                lineNum++;
                var line = rawLine.Trim();

                // 빈 줄, 주석 건너뜀
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                // 첫 번째 데이터 행 = 헤더 건너뜀
                if (dataRows == 0)
                {
                    dataRows++;
                    continue;
                }
                dataRows++;

                var cols = line.Split(',');
                onRow(cols, lineNum);
            }
        }

        // ─ 파싱 헬퍼 ───────────────────────────────────────────

        /// <summary>
        /// 문자열을 float 으로 파싱합니다. 실패 시 경고를 발생시키고 false 를 반환합니다.
        /// </summary>
        public bool TryParseFloat(string raw, int lineNum, string fieldName, out float value)
        {
            if (float.TryParse(raw.Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value))
                return true;

            Warn(lineNum, $"{fieldName} 파싱 실패: '{raw.Trim()}'");
            return false;
        }

        /// <summary>
        /// 문자열을 int 로 파싱합니다. 실패 시 경고를 발생시키고 false 를 반환합니다.
        /// </summary>
        public bool TryParseInt(string raw, int lineNum, string fieldName, out int value)
        {
            if (int.TryParse(raw.Trim(), out value))
                return true;

            Warn(lineNum, $"{fieldName} 파싱 실패: '{raw.Trim()}'");
            return false;
        }

        /// <summary>
        /// 문자열을 Enum 으로 파싱합니다. 실패 시 경고를 발생시키고 false 를 반환합니다.
        /// </summary>
        public bool TryParseEnum<TEnum>(string raw, int lineNum, string fieldName, out TEnum value)
            where TEnum : struct, Enum
        {
            if (Enum.TryParse<TEnum>(raw.Trim(), true, out value))
                return true;

            Warn(lineNum, $"알 수 없는 {fieldName}: '{raw.Trim()}'");
            return false;
        }

        /// <summary>
        /// 최소 열 수를 검사합니다. 부족하면 경고를 발생시키고 false 를 반환합니다.
        /// </summary>
        public bool RequireColumns(string[] cols, int lineNum, int minCount)
        {
            if (cols.Length >= minCount)
                return true;

            Warn(lineNum, $"열 수가 부족합니다 (필요 {minCount}, 실제 {cols.Length})");
            return false;
        }

        // ─ 내부 ────────────────────────────────────────────────

        private void Warn(int lineNum, string message)
            => OnWarning?.Invoke(lineNum, message);
    }
}
