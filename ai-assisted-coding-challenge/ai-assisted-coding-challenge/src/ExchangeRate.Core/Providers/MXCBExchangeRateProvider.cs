using System.Net.Http;
using ExchangeRate.Core.Enums;
using ExchangeRate.Core.Models;

namespace ExchangeRate.Core.Providers
{
    /// <summary>
    /// Mexican Central Bank (Banco de Mexico - BANXICO) exchange rate provider.
    /// Provides MONTHLY USD-MXN exchange rates.
    ///
    /// SPECIAL COMPLEXITY:
    /// - The Mexican central bank publishes MONTHLY USD-MXN rates
    /// - But business requirements need DAILY MXN-EUR rates
    /// - These must be calculated using cross-rates:
    ///   MXN-EUR = 1 / (EUR-USD_daily * USD-MXN_monthly)
    ///
    /// Example calculation:
    ///   If EUR-USD = 1.10 (daily from ECB)
    ///   And USD-MXN = 17.5 (monthly from MXCB)
    ///   Then MXN-EUR = 1 / (1.10 * 17.5) = 0.05195
    ///
    /// This mixed-frequency cross-rate scenario makes refactoring challenging
    /// because the calculation logic must handle different rate frequencies.
    /// </summary>
    public class MXCBExchangeRateProvider : MonthlyExternalApiExchangeRateProvider
    {
        public MXCBExchangeRateProvider(HttpClient httpClient, ExternalExchangeRateApiConfig externalExchangeRateApiConfig)
            : base(httpClient, externalExchangeRateApiConfig)
        {
        }

        /// <summary>
        /// The base currency is MXN (Mexican Peso).
        /// Rates are quoted as how many MXN equal 1 USD (e.g., 17.5 MXN = 1 USD).
        /// </summary>
        public override CurrencyTypes Currency => CurrencyTypes.MXN;

        /// <summary>
        /// Direct quote: Foreign currency to bank currency.
        /// Example: 1 USD = 17.5 MXN (USD is foreign, MXN is local/bank currency)
        /// </summary>
        public override QuoteTypes QuoteType => QuoteTypes.Direct;

        /// <summary>
        /// MXCB = Mexican Central Bank (Banco de Mexico)
        /// </summary>
        public override ExchangeRateSources Source => ExchangeRateSources.MXCB;

        /// <summary>
        /// Bank identifier used in API calls.
        /// </summary>
        public override string BankId => "MXCB";
    }
}
