using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ExchangeRate.Core.Helpers;
using ExchangeRate.Core.Interfaces.Providers;
using ExchangeRate.Core.Models;
using ExchangeRate.Core.Enums;
using ExchangeRateEntity = ExchangeRate.Core.Entities.ExchangeRate;

namespace ExchangeRate.Core.Providers
{
    /// <summary>
    /// Base class for providers that fetch daily exchange rates.
    /// </summary>
    public abstract class DailyExternalApiExchangeRateProvider : ExternalApiExchangeRateProvider, IDailyExchangeRateProvider
    {
        public const int MaxQueryIntervalInDays = 180;

        protected DailyExternalApiExchangeRateProvider(HttpClient httpClient, ExternalExchangeRateApiConfig externalExchangeRateApiConfig)
            : base(httpClient, externalExchangeRateApiConfig)
        {
        }

        public virtual IEnumerable<ExchangeRateEntity> GetHistoricalDailyFxRates(DateTime from, DateTime to)
        {
            if (to < from)
                throw new ArgumentException("to must be later than or equal to from");

            foreach (var period in GetDateRange(from, to, MaxQueryIntervalInDays))
            {
                var rates = AsyncUtil.RunSync(() => GetDailyRatesAsync(BankId, (period.StartDate, period.EndDate)));
                foreach (var rate in rates)
                {
                    yield return rate;
                }
            }
        }

        public virtual IEnumerable<ExchangeRateEntity> GetDailyFxRates()
        {
            // return AsyncUtil.RunSync(() => GetDailyRatesAsync(BankId));
            // get a longer interval in case some of the previous rates were missed
            return GetHistoricalDailyFxRates(DateTime.UtcNow.Date.AddDays(-4), DateTime.UtcNow.Date);
        }

        public static IEnumerable<(DateTime StartDate, DateTime EndDate)> GetDateRange(DateTime startDate, DateTime endDate, int daysChunkSize)
        {
            DateTime markerDate;

            while ((markerDate = startDate.AddDays(daysChunkSize)) < endDate)
            {
                yield return (StartDate: startDate, EndDate: markerDate.AddDays(-1));
                startDate = markerDate;
            }

            yield return (StartDate: startDate, EndDate: endDate);
        }

        public override async Task<IEnumerable<ExchangeRateEntity>> GetExchangeRatesAsync(DateTime startDate, DateTime endDate, ExchangeRateFrequencies frequency, CancellationToken ct = default)
        {
            // Daily provider only supports Daily, but we return whatever we have.
            // Ideally we could assert frequency == Daily.
            
            if (endDate < startDate)
                throw new ArgumentException("endDate must be later than or equal to startDate");

            var results = new List<ExchangeRateEntity>();
            foreach (var period in GetDateRange(startDate, endDate, MaxQueryIntervalInDays))
            {
                var rates = await GetDailyRatesAsync(BankId, (period.StartDate, period.EndDate), ct);
                results.AddRange(rates);
            }
            return results;
        }

        public async Task<IEnumerable<ExchangeRateEntity>> GetLatestFxRateAsync()
        {
            return await GetDailyRatesAsync(BankId);
        }
    }
}
