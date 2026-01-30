using System;
using System.Collections.Generic;
using System.Linq;
using ExchangeRate.Core.Enums;
using ExchangeRate.Core.Interfaces;
using ExchangeRate.Core.Interfaces.Providers;

namespace ExchangeRate.Core
{
    class ExchangeRateProviderFactory : IExchangeRateProviderFactory
    {
        private readonly List<IExchangeRateProvider> _exchangeRateProviders;
        private readonly IServiceProvider _serviceProvider;

        public ExchangeRateProviderFactory(IEnumerable<IExchangeRateProvider> exchangeRateProviders, IServiceProvider serviceProvider)
        {
            _exchangeRateProviders = exchangeRateProviders.ToList();
            _serviceProvider = serviceProvider;
        }

        public IExchangeRateProvider GetExchangeRateProvider(ExchangeRateSources source)
        {
            var provider = _exchangeRateProviders.FirstOrDefault(x => x.Source == source);

            if (provider is null)
            {
                throw new NotSupportedException($"Source {source} is not supported.");
            }

            return (IExchangeRateProvider)_serviceProvider.GetService(provider.GetType());
        }

        public bool TryGetExchangeRateProviderByCurrency(CurrencyTypes currency, out IExchangeRateProvider provider)
        {
            var providerType = _exchangeRateProviders.FirstOrDefault(x => x.Currency == currency)?.GetType();

            if (providerType is null)
            {
                provider = null;
                return false;
            }

            provider = (IExchangeRateProvider)_serviceProvider.GetService(providerType);
            return true;
        }

        public IEnumerable<ExchangeRateSources> ListExchangeRateSources() => _exchangeRateProviders.Select(x => x.Source);
    }
}
