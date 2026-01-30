#nullable enable
namespace ExchangeRate.Core.Helpers;

/// <summary>
/// Helper class for period-based date calculations used in exchange rate lookups.
/// Simplified from Taxually.Common.Helpers.PeriodHelper - contains only methods needed for exchange rates.
/// Original: Taxually.Common.Helpers.PeriodHelper
/// </summary>
public static class PeriodHelper
{
    /// <summary>
    /// Returns a date with the first day of the year and month of the given date.
    /// Used for monthly exchange rate lookups and period alignment.
    /// </summary>
    public static DateTime GetStartOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    /// <summary>
    /// Checks whether the given date is valid within the specified validity interval.
    /// Used to validate VAT number validity for exchange rate selection.
    /// </summary>
    public static bool IsValidAt(DateTime? validFrom, DateTime? validTo, DateTime date)
    {
        if (validFrom > validTo)
            throw new ArgumentOutOfRangeException($"Validity interval is invalid. ValidFrom: {validFrom} ValidTo: {validTo}");

        return (validFrom == null || validFrom <= date) && (validTo == null || validTo >= date.Date);
    }
}
