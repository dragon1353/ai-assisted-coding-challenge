using System.Net.Http;
using ExchangeRate.Core.Enums;
using ExchangeRate.Core.Models;

namespace ExchangeRate.Core.Providers
{
    /// <summary>
    /// Swedish Central Bank (Riksbank) exchange rate provider.
    /// Provides daily SEK exchange rates.
    /// </summary>
    public class SECBExchangeRateProvider : DailyExternalApiExchangeRateProvider
    {
        public SECBExchangeRateProvider(HttpClient httpClient, ExternalExchangeRateApiConfig externalExchangeRateApiConfig)
            : base(httpClient, externalExchangeRateApiConfig)
        {
        }

        public override CurrencyTypes Currency => CurrencyTypes.SEK;

        public override QuoteTypes QuoteType => QuoteTypes.Direct;

        public override ExchangeRateSources Source => ExchangeRateSources.SECB;

        public override string BankId => "SECB";
    }
}
