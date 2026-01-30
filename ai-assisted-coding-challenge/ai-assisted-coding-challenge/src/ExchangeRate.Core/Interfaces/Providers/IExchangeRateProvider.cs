using ExchangeRate.Core.Enums;

namespace ExchangeRate.Core.Interfaces.Providers;

public interface IExchangeRateProvider
{
    CurrencyTypes Currency { get; }

    QuoteTypes QuoteType { get; }

    ExchangeRateSources Source { get; }

    ExchangeRateCalculationMethods DefaultCalculationMethod => this switch
    {
        IDailyExchangeRateProvider => ExchangeRateCalculationMethods.Daily,
        IMonthlyExchangeRateProvider => ExchangeRateCalculationMethods.Monthly,
        IBiWeeklyExchangeRateProvider => ExchangeRateCalculationMethods.BiWeekly,
        IWeeklyExchangeRateProvider => ExchangeRateCalculationMethods.Weekly,
        _ => ExchangeRateCalculationMethods.Daily
    };

    ExchangeRateFrequencies DefaultCalculationFrequency => this switch
    {
        IDailyExchangeRateProvider => ExchangeRateFrequencies.Daily,
        IMonthlyExchangeRateProvider => ExchangeRateFrequencies.Monthly,
        IBiWeeklyExchangeRateProvider => ExchangeRateFrequencies.BiWeekly,
        IWeeklyExchangeRateProvider => ExchangeRateFrequencies.Weekly,
        _ => ExchangeRateFrequencies.Daily
    };

    Task<IEnumerable<Entities.ExchangeRate>> GetExchangeRatesAsync(DateTime startDate, DateTime endDate, ExchangeRateFrequencies frequency, CancellationToken ct = default);
}
