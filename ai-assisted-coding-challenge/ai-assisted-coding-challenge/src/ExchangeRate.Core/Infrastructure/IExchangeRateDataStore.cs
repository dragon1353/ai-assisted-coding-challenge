#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangeRate.Core.Entities;
using ExchangeRate.Core.Enums;

namespace ExchangeRate.Core.Infrastructure;

/// <summary>
/// Abstraction for exchange rate data persistence.
/// This interface abstracts the database context to allow the repository to be tested
/// and used without a concrete Entity Framework dependency.
/// </summary>
public interface IExchangeRateDataStore
{
    /// <summary>
    /// Gets all exchange rates from the data store for querying.
    /// Used for loading rates into the in-memory cache.
    /// </summary>
    IQueryable<Entities.ExchangeRate> ExchangeRates { get; }

    /// <summary>
    /// Gets exchange rates filtered by date range.
    /// </summary>
    /// <param name="minDate">Minimum date (inclusive)</param>
    /// <param name="maxDate">Maximum date (exclusive)</param>
    /// <returns>List of exchange rates within the date range</returns>
    Task<List<Entities.ExchangeRate>> GetExchangeRatesAsync(DateTime minDate, DateTime maxDate);

    /// <summary>
    /// Saves multiple exchange rates to the data store.
    /// If a rate already exists for the same date/currency/source/frequency combination,
    /// it should be updated or ignored based on implementation.
    /// </summary>
    /// <param name="rates">Exchange rates to save</param>
    Task SaveExchangeRatesAsync(IEnumerable<Entities.ExchangeRate> rates);

    /// <summary>
    /// Gets all pegged currency configurations.
    /// Pegged currencies have fixed exchange rates relative to another currency.
    /// </summary>
    /// <returns>List of pegged currency configurations</returns>
    List<PeggedCurrency> GetPeggedCurrencies();
}
