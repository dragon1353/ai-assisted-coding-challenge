using System;
using ExchangeRate.Core.Enums;

namespace ExchangeRate.Core.Entities;

public record ExchangeRate
{
    public DateTime? Date { get; set; }

    public CurrencyTypes? CurrencyId { get; set; }

    public ExchangeRateSources? Source { get; set; }

    public ExchangeRateFrequencies? Frequency { get; set; }

    /// <summary>
    /// The exchange rate of the currency on the given day. It is stored with 5 decimal precision.
    /// </summary>
    public decimal? Rate { get; set; }

    public override string ToString()
    {
        return $"{CurrencyId} - {Date:yyyy-MM-dd}: {Rate}";
    }

    public const int Precision = 10;
}
