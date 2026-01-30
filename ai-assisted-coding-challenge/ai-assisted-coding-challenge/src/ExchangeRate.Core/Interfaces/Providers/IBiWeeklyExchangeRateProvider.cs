using System;
using System.Collections.Generic;
using ExchangeRateEntity = ExchangeRate.Core.Entities.ExchangeRate;

namespace ExchangeRate.Core.Interfaces.Providers
{
    public interface IBiWeeklyExchangeRateProvider : IExchangeRateProvider
    {
        IEnumerable<ExchangeRateEntity> GetBiWeeklyFxRates();
        IEnumerable<ExchangeRateEntity> GetHistoricalBiWeeklyFxRates(DateTime from, DateTime to);
    }
}
