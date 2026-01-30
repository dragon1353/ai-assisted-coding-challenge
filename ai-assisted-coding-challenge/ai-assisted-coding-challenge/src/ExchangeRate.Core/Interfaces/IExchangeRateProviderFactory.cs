using System.Collections.Generic;
using ExchangeRate.Core.Enums;
using ExchangeRate.Core.Interfaces.Providers;

namespace ExchangeRate.Core.Interfaces
{
    public interface IExchangeRateProviderFactory
    {
        IExchangeRateProvider GetExchangeRateProvider(ExchangeRateSources source);

        IEnumerable<ExchangeRateSources> ListExchangeRateSources();

        bool TryGetExchangeRateProviderByCurrency(CurrencyTypes currency, out IExchangeRateProvider provider);
    }
}
