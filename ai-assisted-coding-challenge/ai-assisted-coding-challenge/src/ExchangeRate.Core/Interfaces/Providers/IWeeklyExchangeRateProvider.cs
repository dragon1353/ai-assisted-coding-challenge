using System;
using System.Collections.Generic;
using ExchangeRateEntity = ExchangeRate.Core.Entities.ExchangeRate;

namespace ExchangeRate.Core.Interfaces.Providers
{
    public interface IWeeklyExchangeRateProvider : IExchangeRateProvider
    {
        IEnumerable<ExchangeRateEntity> GetWeeklyFxRates();
        IEnumerable<ExchangeRateEntity> GetHistoricalWeeklyFxRates(DateTime from, DateTime to);
    }
}
