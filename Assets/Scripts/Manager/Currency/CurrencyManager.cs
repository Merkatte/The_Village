using System;
using System.Collections.Generic;
using Game.Core.Enums;
using Game.Core.Interfaces;

namespace Game.Manager.Currency
{
    /// <summary>
    /// ICurrencyService 구현체입니다.
    ///
    /// ─ 설계 원칙 ──────────────────────────────────────────────
    ///   - 순수 C# 클래스. Unity 라이프사이클에 무관합니다.
    ///   - CurrencyType 별 잔액을 Dictionary 로 관리합니다.
    ///   - 미등록 타입은 0으로 취급하며, Add/TrySpend 시 자동으로 초기화됩니다.
    ///
    /// ─ 등록 위치 ──────────────────────────────────────────────
    ///   GameBootstrap.RegisterServices() 에서 ICurrencyService 로 등록합니다.
    /// </summary>
    public sealed class CurrencyManager : ICurrencyService
    {
        /// <inheritdoc/>
        public event Action<CurrencyType, int> OnCurrencyChanged;

        private readonly Dictionary<CurrencyType, int> _currencies = new();

        /// <inheritdoc/>
        public int Get(CurrencyType type)
            => _currencies.TryGetValue(type, out var value) ? value : 0;

        /// <inheritdoc/>
        public void Add(CurrencyType type, int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount),
                    "[CurrencyManager] 추가 금액은 1 이상이어야 합니다.");

            _currencies[type] = Get(type) + amount;
            OnCurrencyChanged?.Invoke(type, _currencies[type]);
        }

        /// <inheritdoc/>
        public bool TrySpend(CurrencyType type, int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount),
                    "[CurrencyManager] 소비 금액은 1 이상이어야 합니다.");

            if (Get(type) < amount) return false;

            _currencies[type] = Get(type) - amount;
            OnCurrencyChanged?.Invoke(type, _currencies[type]);
            return true;
        }
    }
}
