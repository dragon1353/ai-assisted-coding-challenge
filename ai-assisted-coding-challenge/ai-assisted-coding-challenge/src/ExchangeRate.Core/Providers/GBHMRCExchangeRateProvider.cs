using System.Net.Http;
using ExchangeRate.Core.Enums;
using ExchangeRate.Core.Models;

namespace ExchangeRate.Core.Providers
{
    /// <summary>
    /// UK HMRC (Her Majesty's Revenue and Customs) exchange rate provider.
    /// Provides monthly GBP exchange rates.
    /// </summary>
    public class GBHMRCExchangeRateProvider : MonthlyExternalApiExchangeRateProvider
    {
        public GBHMRCExchangeRateProvider(HttpClient httpClient, ExternalExchangeRateApiConfig externalExchangeRateApiConfig)
            : base(httpClient, externalExchangeRateApiConfig)
        {
        }

        public override CurrencyTypes Currency => CurrencyTypes.GBP;

        public override QuoteTypes QuoteType => QuoteTypes.Indirect;

        public override ExchangeRateSources Source => ExchangeRateSources.HMRC;

        public override string BankId => "GBHMRC";
    }
}
