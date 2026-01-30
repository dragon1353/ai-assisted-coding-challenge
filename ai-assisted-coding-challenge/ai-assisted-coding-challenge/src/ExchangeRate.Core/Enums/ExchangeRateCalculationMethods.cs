namespace ExchangeRate.Core.Enums;

public enum ExchangeRateCalculationMethods
{
    /// <summary>
    /// Use the exchange rates effective on the given date.
    /// </summary>
    Daily = 1,

    /// <summary>
    /// Use the exchange rates effective on the previous day.
    /// </summary>
    PreviousDaily = 2,

    /// <summary>
    /// Use the monthly average exchange rates.
    /// </summary>
    Monthly = 3,

    /// <summary>
    /// Use the last available rate for the given tax period.
    /// </summary>
    LastDayOfTaxPeriod = 4,

    /// <summary>
    /// Use the weekly exchange rates.
    /// </summary>
    Weekly = 5,

    /// <summary>
    /// Use the biweekly exchange rates.
    /// </summary>
    BiWeekly = 6
}
