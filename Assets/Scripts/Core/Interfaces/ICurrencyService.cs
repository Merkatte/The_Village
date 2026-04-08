using System;
using Game.Core.Enums;

namespace Game.Core.Interfaces
{
    /// <summary>
    /// 재화 조회·획득·소비 인터페이스입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - CurrencyType 별로 독립적으로 관리합니다.
    ///   - 재화가 변경될 때마다 OnCurrencyChanged 이벤트를 발행합니다.
    ///   - TrySpend 는 잔액 부족 시 false 를 반환하며 재화를 소비하지 않습니다.
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>재화가 변경될 때 발행합니다. (변경된 타입, 변경 후 금액)</summary>
        event Action<CurrencyType, int> OnCurrencyChanged;

        /// <summary>지정한 재화의 현재 보유량을 반환합니다. 미등록 타입은 0을 반환합니다.</summary>
        int Get(CurrencyType type);

        /// <summary>지정한 재화를 amount 만큼 추가합니다.</summary>
        /// <exception cref="ArgumentOutOfRangeException">amount 가 1 미만인 경우</exception>
        void Add(CurrencyType type, int amount);

        /// <summary>
        /// 지정한 재화를 amount 만큼 소비합니다.
        /// 잔액이 충분하면 소비 후 true, 부족하면 false 를 반환합니다.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">amount 가 1 미만인 경우</exception>
        bool TrySpend(CurrencyType type, int amount);
    }
}
