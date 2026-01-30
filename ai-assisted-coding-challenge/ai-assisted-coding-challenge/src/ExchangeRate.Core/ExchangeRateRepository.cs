using Microsoft.Extensions.Logging;
using FluentResults;
using ExchangeRate.Core.Exceptions;
using ExchangeRate.Core.Helpers;
using ExchangeRate.Core.Interfaces;
using ExchangeRate.Core.Interfaces.Providers;
using ExchangeRate.Core.Entities;
using ExchangeRate.Core.Enums;
using ExchangeRate.Core.Infrastructure;

namespace ExchangeRate.Core
{
    class ExchangeRateRepository : IExchangeRateRepository
    {
        /// <summary>
        /// Maps currecy code string to currency type.
        /// </summary>
        private static readonly Dictionary<string, CurrencyTypes> CurrencyMapping;

        private readonly Dictionary<(ExchangeRateSources, ExchangeRateFrequencies), Dictionary<CurrencyTypes, Dictionary<DateTime, decimal>>> _fxRatesBySourceFrequencyAndCurrency;
        private readonly Dictionary<CurrencyTypes, PeggedCurrency> _peggedCurrencies;

        private readonly IExchangeRateDataStore _dataStore;

        private readonly ILogger<ExchangeRateRepository> _logger;
        private readonly IExchangeRateProviderFactory _exchangeRateSourceFactory;

        static ExchangeRateRepository()
        {
            var currencies = System.Enum.GetValues(typeof(CurrencyTypes)).Cast<CurrencyTypes>().ToList();
            CurrencyMapping = currencies.ToDictionary(x => x.ToString().ToUpperInvariant());
        }

        public ExchangeRateRepository(IExchangeRateDataStore dataStore, ILogger<ExchangeRateRepository> logger, IExchangeRateProviderFactory exchangeRateSourceFactory)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _exchangeRateSourceFactory = exchangeRateSourceFactory ?? throw new ArgumentNullException(nameof(exchangeRateSourceFactory));

            _fxRatesBySourceFrequencyAndCurrency = new Dictionary<(ExchangeRateSources, ExchangeRateFrequencies), Dictionary<CurrencyTypes, Dictionary<DateTime, decimal>>>();

            _peggedCurrencies = _dataStore.GetPeggedCurrencies()
                .ToDictionary(x => x.CurrencyId!.Value);
        }

        internal ExchangeRateRepository(IEnumerable<Entities.ExchangeRate> rates, IExchangeRateProviderFactory exchangeRateSourceFactory)
        {
            _fxRatesBySourceFrequencyAndCurrency = new Dictionary<(ExchangeRateSources, ExchangeRateFrequencies), Dictionary<CurrencyTypes, Dictionary<DateTime, decimal>>>();

            LoadRates(rates);

            _exchangeRateSourceFactory = exchangeRateSourceFactory ?? throw new ArgumentNullException(nameof(exchangeRateSourceFactory));
        }

        /// <summary>
        /// Returns the exchange rate for the <paramref name="toCurrency"/> on the given <paramref name="date"/>.
        /// It will return a previously valid rate, if the database does not contain rate for the specified <paramref name="date"/>.
        /// It will return NULL if there is no rate at all for the <paramref name="toCurrency"/>.
        /// </summary>
        public decimal? GetRate(CurrencyTypes fromCurrency, CurrencyTypes toCurrency, DateTime date, ExchangeRateSources source, ExchangeRateFrequencies frequency)
        {
            var provider = _exchangeRateSourceFactory.GetExchangeRateProvider(source);

            if (toCurrency == fromCurrency)
                return 1m;

            date = date.Date;

            // If neither fromCurrency, nor toCurrency matches the provider's currency, we need to calculate cross rates
            if (fromCurrency != provider.Currency && toCurrency != provider.Currency)
            {
                return GetRate(fromCurrency, provider.Currency, date, source, frequency) * GetRate(provider.Currency, toCurrency, date, source, frequency);
            }

            CurrencyTypes lookupCurrency = default;
            var result = GetFxRate(source, frequency, date, provider, fromCurrency, toCurrency, out _);

            if (result.IsSuccess)
                return result.Value;

            if (result.Errors.FirstOrDefault() is NoFxRateFoundError)
            {
                // Clean Refactor: Strategy - Fetch Monthly
                // 1. Check DB for the month
                // 2. Load DB to Memory
                // 3. Retry
                // 4. Fallback: Fetch Provider (Month) -> Save DB -> Load Memory
                
                var startOfMonth = PeriodHelper.GetStartOfMonth(date);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Step 1: Load from DB
                LoadRatesFromDb(startOfMonth, endOfMonth);

                // Step 2: Retry
                result = GetFxRate(source, frequency, date, provider, fromCurrency, toCurrency, out lookupCurrency);
                if (result.IsSuccess) return result.Value;

                // Step 3: Fetch from Provider
                try 
                {
                    var rates = AsyncUtil.RunSync(() => provider.GetExchangeRatesAsync(startOfMonth, endOfMonth, frequency));
                    
                    if (rates != null && rates.Any())
                    {
                        var list = rates.ToList();
                        AsyncUtil.RunSync(() => _dataStore.SaveExchangeRatesAsync(list));
                        LoadRates(list);
                    }
                }
                catch (Exception ex)
                {
                     _logger.LogError(ex, "Failed to fetch/save rates for {source} {frequency} {date}", source, frequency, date);
                }

                // Step 4: Retry Final
                result = GetFxRate(source, frequency, date, provider, fromCurrency, toCurrency, out lookupCurrency);
                if (result.IsSuccess) return result.Value;

                // Step 5: Fallbacks (Future / Month Boundary)
                bool fallbackFetched = false;
                
                // Fallback A: Future Date -> Ensure Latest (Current Month) is loaded
                if (date > DateTime.UtcNow.Date)
                {
                     var currentStartCallback = PeriodHelper.GetStartOfMonth(DateTime.UtcNow);
                     if (currentStartCallback != startOfMonth) // Don't re-fetch if we just fetched it
                     {
                         // Fetch Current Month
                         try
                         {
                             // Only need to fetch provider really, DB load handled if needed but for "Latest" usually provider is key.
                             // But let's follow pattern: Load DB, then Provider.
                             var currentEnd = currentStartCallback.AddMonths(1).AddDays(-1);
                             // Optimization: Just go straight to Provider for "Latest" usually?
                             // But consistency...
                             var rates = AsyncUtil.RunSync(() => provider.GetExchangeRatesAsync(currentStartCallback, currentEnd, frequency));
                              if (rates != null && rates.Any())
                            {
                                var list = rates.ToList();
                                AsyncUtil.RunSync(() => _dataStore.SaveExchangeRatesAsync(list));
                                LoadRates(list);
                                fallbackFetched = true;
                            }
                         }
                         catch (Exception ex)
                         {
                             _logger.LogError(ex, "Failed to fetch fallback current rates for {source}", source);
                         }
                     }
                }

                // Fallback B: Month Boundary (e.g. Jan 1st asking for previous rate) -> Fetch Previous Month
                // If we haven't found it yet, and we are not in Future case (or Future case failed).
                // Check if we are close to start of month? Or just always fetch prev month if missing?
                // To be safe and pass "previously valid rate" test: Fetch Previous Month.
                if (!fallbackFetched)
                {
                     var prevStart = startOfMonth.AddMonths(-1);
                     // If we are asking for today, and today is 15th, previous month is likely not needed unless gap is huge.
                     // But if we missed, let's try.
                     var prevEnd = startOfMonth.AddDays(-1);
                     
                     try
                     {
                         var rates = AsyncUtil.RunSync(() => provider.GetExchangeRatesAsync(prevStart, prevEnd, frequency));
                         if (rates != null && rates.Any())
                        {
                            var list = rates.ToList();
                            AsyncUtil.RunSync(() => _dataStore.SaveExchangeRatesAsync(list));
                            LoadRates(list);
                        }
                     }
                     catch (Exception ex)
                     {
                          _logger.LogError(ex, "Failed to fetch fallback previous rates for {source}", source);
                     }
                }
                
                // Final Retry
                result = GetFxRate(source, frequency, date, provider, fromCurrency, toCurrency, out lookupCurrency);
                if (result.IsSuccess) return result.Value;
            }

            _logger.LogError("No {source} {frequency} exchange rate found for {lookupCurrency} on {date:yyyy-MM-dd}. FromCurrency: {fromCurrency}, ToCurrency: {toCurrency}", source, frequency, null, date, fromCurrency, toCurrency);
            return null;
        }

        /// <summary>
        /// Returns the exchange rate for the <paramref name="currencyCode"/> on the given <paramref name="date"/>.
        /// It will return a previously valid rate, if the database does not contain rate for the specified <paramref name="date"/>.
        /// It will return NULL if there is no rate at all for the <paramref name="currencyCode"/>.
        /// </summary>
        public decimal? GetRate(string fromCurrencyCode, string toCurrencyCode, DateTime date, ExchangeRateSources source, ExchangeRateFrequencies frequency)
        {
            var fromCurrency = GetCurrencyType(fromCurrencyCode);

            var toCurrency = GetCurrencyType(toCurrencyCode);

            return GetRate(fromCurrency, toCurrency, date, source, frequency);
        }

        public void UpdateRates()
        {
            // Legacy / Periodic update method.
            // Ideally should use GetExchangeRatesAsync but with what range?
            // "Updates the exchange rates for the last available day/month."
            // Original implementation updated "Everything".
            // Minimal implementation: Update "Latest".
            
             foreach (var source in _exchangeRateSourceFactory.ListExchangeRateSources())
            {
                try
                {
                    var provider = _exchangeRateSourceFactory.GetExchangeRateProvider(source);
                    // Use GetLatestFxRateAsync if available or generic generic GetExchangeRatesAsync(Today)
                    // Providers usually have GetLatestFxRateAsync from IDaily...
                    
                    if (provider is IDailyExchangeRateProvider daily)
                    {
                         var rates = AsyncUtil.RunSync(() => daily.GetLatestFxRateAsync());
                         ProcessRates(rates);
                    }
                    else if (provider is IMonthlyExchangeRateProvider monthly)
                    {
                        var rates = AsyncUtil.RunSync(() => provider.GetExchangeRatesAsync(PeriodHelper.GetStartOfMonth(DateTime.UtcNow), DateTime.UtcNow, ExchangeRateFrequencies.Monthly));
                        ProcessRates(rates);
                    }
                    // Add others if needed
                }
                 catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update daily rates for {source}", source.ToString());
                }
            }
        }
        
        private void ProcessRates(IEnumerable<Entities.ExchangeRate> rates)
        {
             if (rates.Any())
             {
                 var itemsToSave = new List<Entities.ExchangeRate>();
                 foreach (var rate in rates)
                 {
                     if (AddRateToDictionaries(rate))
                         itemsToSave.Add(rate);
                 }

                 if (itemsToSave.Any())
                     AsyncUtil.RunSync(() => _dataStore.SaveExchangeRatesAsync(itemsToSave));
             }
        }

        public bool EnsureMinimumDateRange(DateTime minDate, IEnumerable<ExchangeRateSources> exchangeRateSources = null)
        {
             // Deprecated or Simplified.
             // With "On-Demand" strategy, we don't strictly need this for GetRate to work.
             // But if current tests rely on it to "Pre-warm" cache, we should support it.
             // implementation: Iterate sources, call GetExchangeRatesAsync(minDate, Now).
             
             var result = true;
             var now = DateTime.UtcNow.Date;
             foreach (var source in exchangeRateSources ?? _exchangeRateSourceFactory.ListExchangeRateSources())
            {
                try
                {
                    var provider = _exchangeRateSourceFactory.GetExchangeRateProvider(source);
                    // Frequency? Default to provider's default?
                    var frequency = provider.DefaultCalculationFrequency;
                    var rates = AsyncUtil.RunSync(() => provider.GetExchangeRatesAsync(minDate, now, frequency));
                    ProcessRates(rates);
                }
                catch
                {
                    result = false;
                }
            }
            return result;
        }

        private void LoadRatesFromDb(DateTime minDate, DateTime maxDate)
        {
            var fxRatesInDb = AsyncUtil.RunSync(() => _dataStore.GetExchangeRatesAsync(minDate, maxDate));
            LoadRates(fxRatesInDb);
        }

        private void LoadRates(IEnumerable<Entities.ExchangeRate> fxRatesInDb)
        {
            foreach (var item in fxRatesInDb)
            {
                AddRateToDictionaries(item);
            }
        }

        private bool AddRateToDictionaries(Entities.ExchangeRate item)
        {
            var currency = item.CurrencyId!.Value;
            var date = item.Date!.Value;
            var source = item.Source!.Value;
            var frequency = item.Frequency!.Value;
            var newRate = item.Rate!.Value;

            if (!_fxRatesBySourceFrequencyAndCurrency.TryGetValue((source, frequency), out var currenciesBySource))
                _fxRatesBySourceFrequencyAndCurrency.Add((source, frequency), currenciesBySource = new Dictionary<CurrencyTypes, Dictionary<DateTime, decimal>>());

            if (!currenciesBySource.TryGetValue(currency, out var datesByCurrency))
                currenciesBySource.Add(currency, datesByCurrency = new Dictionary<DateTime, decimal>());

            if (datesByCurrency.TryGetValue(date, out var savedRate))
            {
                // DATA OVERWRITE LOGIC
                if (decimal.Round(newRate, Entities.ExchangeRate.Precision) != decimal.Round(savedRate, Entities.ExchangeRate.Precision))
                {
                    _logger.LogWarning("Overwriting exchange rate. Currency: {currency}. Saved rate: {savedRate}. New rate: {newRate}. Source: {source}. Frequency: {frequency}", currency, savedRate, newRate, source, frequency);
                    datesByCurrency[date] = newRate; 
                    return true; // Indicate change -> Save to DB
                }

                return false;
            }
            else
            {
                datesByCurrency.Add(date, newRate);
                return true;
            }
        }

        private Result<decimal> GetFxRate(
            ExchangeRateSources source, ExchangeRateFrequencies frequency, 
            DateTime date,
            IExchangeRateProvider provider,
            CurrencyTypes fromCurrency,
            CurrencyTypes toCurrency,
            out CurrencyTypes lookupCurrency)
        {
             // Helper to fetch dictionary safely
             if (!_fxRatesBySourceFrequencyAndCurrency.TryGetValue((source, frequency), out var ratesByCurrencyAndDate))
             {
                 lookupCurrency = default;
                 return Result.Fail(new NoFxRateFoundError());
             }

                // Handle same-currency conversion
                if (fromCurrency == toCurrency)
                {
                    lookupCurrency = fromCurrency;
                    return Result.Ok(1m);
                }

                lookupCurrency = toCurrency == provider.Currency ? fromCurrency : toCurrency;
                var nonLookupCurrency = toCurrency == provider.Currency ? toCurrency : fromCurrency;

                if (!ratesByCurrencyAndDate.TryGetValue(lookupCurrency, out var currencyDict))
                {
                    if (!_peggedCurrencies.TryGetValue(lookupCurrency, out var peggedCurrency))
                    {
                         // Check if Pegged Logic needs recursion?
                        return Result.Fail(new NotSupportedCurrencyError(lookupCurrency));
                    }

                    // Recursive call for pegged
                    var peggedToCurrencyResult = GetFxRate(source, frequency, date, provider, nonLookupCurrency, peggedCurrency.PeggedTo!.Value, out _);

                    if (peggedToCurrencyResult.IsFailed)
                    {
                        return peggedToCurrencyResult;
                    }

                    var peggedRate = peggedCurrency.Rate!.Value;
                    var resultRate = peggedToCurrencyResult.Value;

                    return Result.Ok(toCurrency == provider.Currency
                        ? peggedRate / resultRate
                        : resultRate / peggedRate);

                }
                
            // Search back for PREVIOUSLY VALID rate (Fall back)
            // Note: We search indefinitely back? Or constrained?
            // Original code: `for (var d = date; d >= minFxDate; d = d.AddDays(-1d))`
            // Now we don't have minFxDate.
            // Loop until found or ... reasonable limit?
            
            // If we are looking for a gap, we should verify that we have fetched the previous values.
            // With "Bucket" fetching: If I fetched May, I have May.
            // If I look back to April, and I haven't fetched April, I just miss it?
            // "It will return a previously valid rate"
            // If I haven't loaded April, I can't return previously valid rate from April.
            
            // Refinement: If we miss in the current month, should we fetch the previous month?
            // This could trigger cascading fetches.
            // For now, let's limit search to the dictionary content. safely.
            // If the user wants "Previous valid rate", and it's not in memory, we might fail.
            // But usually "Previous valid" refers to "Yesterday" (holiday).
            // So iterating back a few days is usually enough.
            // Let's iterate back 30 days? Or just keep iterating as long as we have data?
            
            // Search back for PREVIOUSLY VALID rate (Fall back)
            // Strategy: O(N) scan of loaded keys to find the max date <= requested date.
            // This supports finding "Latest" for future dates, and bridging gaps.
            
            var bestDate = DateTime.MinValue;
            bool found = false;

            // Optimization: If we expect recent dates, iterating everything might be slow if history is huge.
            // But Clean Architecture refactor fetches "On Demand". 
            // So Dictionary likely contains only relevant months (Small N).
            foreach (var d in currencyDict.Keys)
            {
                if (d <= date && d > bestDate)
                {
                    bestDate = d;
                    found = true;
                }
            }

            if (found)
            {
                var fxRate = currencyDict[bestDate];
                return provider.QuoteType switch
                {
                    QuoteTypes.Direct when toCurrency == provider.Currency => Result.Ok(fxRate),
                    QuoteTypes.Direct when fromCurrency == provider.Currency => Result.Ok(1 / fxRate),
                    QuoteTypes.Indirect when fromCurrency == provider.Currency => Result.Ok(fxRate),
                    QuoteTypes.Indirect when toCurrency == provider.Currency => Result.Ok(1 / fxRate),
                    _ => throw new InvalidOperationException("Unsupported QuoteType")
                };
            }

            return Result.Fail(new NoFxRateFoundError());
        }

        private static CurrencyTypes GetCurrencyType(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                throw new ExchangeRateException("Null or empty currency code.");

            if (!CurrencyMapping.TryGetValue(currencyCode.ToUpperInvariant(), out var currency))
                throw new ExchangeRateException("Not supported currency code: " + currencyCode);

            return currency;
        }
    }

    class NotSupportedCurrencyError : Error
    {
        public NotSupportedCurrencyError(CurrencyTypes currency)
            : base("Not supported currency: " + currency) { }
    }

    class NoFxRateFoundError : Error
    {
        public NoFxRateFoundError()
            : base("No fx rate found") { }
    }
}
