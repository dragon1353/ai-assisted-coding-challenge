using System.Net.Http;
using ExchangeRate.Core.Enums;
using ExchangeRate.Core.Models;

namespace ExchangeRate.Core.Providers
{
    /// <summary>
    /// Polish Central Bank (NBP - Narodowy Bank Polski) exchange rate provider.
    /// Provides daily PLN exchange rates.
    /// </summary>
    public class PLCBExchangeRateProvider : DailyExternalApiExchangeRateProvider
    {
        public PLCBExchangeRateProvider(HttpClient httpClient, ExternalExchangeRateApiConfig externalExchangeRateApiConfig)
            : base(httpClient, externalExchangeRateApiConfig)
        {
        }

        public override CurrencyTypes Currency => CurrencyTypes.PLN;

        public override QuoteTypes QuoteType => QuoteTypes.Direct;

        public override ExchangeRateSources Source => ExchangeRateSources.PLCB;

        public override string BankId => "PLCB";
    }
}
