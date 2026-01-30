using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExchangeRateEntity = ExchangeRate.Core.Entities.ExchangeRate;

namespace ExchangeRate.Core.Interfaces.Providers
{
    public interface IDailyExchangeRateProvider : IExchangeRateProvider
    {
        IEnumerable<ExchangeRateEntity> GetDailyFxRates();

        IEnumerable<ExchangeRateEntity> GetHistoricalDailyFxRates(DateTime from, DateTime to);

        Task<IEnumerable<ExchangeRateEntity>> GetLatestFxRateAsync();
    }
}
