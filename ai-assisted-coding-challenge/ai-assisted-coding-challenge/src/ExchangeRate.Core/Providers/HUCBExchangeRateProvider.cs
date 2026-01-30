using System.Net.Http;
using ExchangeRate.Core.Enums;
using ExchangeRate.Core.Models;

namespace ExchangeRate.Core.Providers
{
    /// <summary>
    /// Hungarian Central Bank (MNB - Magyar Nemzeti Bank) exchange rate provider.
    /// Provides daily HUF exchange rates.
    /// </summary>
    public class HUCBExchangeRateProvider : DailyExternalApiExchangeRateProvider
    {
        public HUCBExchangeRateProvider(HttpClient httpClient, ExternalExchangeRateApiConfig externalExchangeRateApiConfig)
            : base(httpClient, externalExchangeRateApiConfig)
        {
        }

        public override CurrencyTypes Currency => CurrencyTypes.HUF;

        public override QuoteTypes QuoteType => QuoteTypes.Direct;

        public override ExchangeRateSources Source => ExchangeRateSources.MNB;

        public override string BankId => "HUCB";
    }
}
