using System;
using System.Collections.Generic;
using ExchangeRateEntity = ExchangeRate.Core.Entities.ExchangeRate;

namespace ExchangeRate.Core.Interfaces.Providers
{
    public interface IMonthlyExchangeRateProvider : IExchangeRateProvider
    {
        IEnumerable<ExchangeRateEntity> GetMonthlyFxRates();
        IEnumerable<ExchangeRateEntity> GetHistoricalMonthlyFxRates(DateTime from, DateTime to);
    }
}
