#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ExchangeRate.Core.Enums;

namespace ExchangeRate.Core.Entities;

/// <summary>
/// Simplified Country entity containing only exchange-rate-related properties.
/// Original: Taxually.Domain.Entities.Country - contains static data for country lookup.
/// </summary>
public class Country
{
    #region Country Code Constants

    public const string BELARUS_COUNTRYCODE = "BY";
    public const string COLOMBIA_COUNRTYCODE = "CO";
    public const string KAZAKHSTAN_COUNRTYCODE = "KZ";
    public const string SOUTHKOREA_COUNTRYCODE = "KR";
    public const string THAILAND_COUNTRYCODE = "TH";
    public const string EU_IOSS_COUNTRYCODE = "EU";
    public const string EU_OSS_COUNTRYCODE = "XU";
    public const string EU_OSS_NON_UNION_COUNTRYCODE = "XN";

    #endregion

    /// <summary>
    /// Country identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Two-letter ISO country code (e.g., "DE", "US")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Country name in English
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Default currency for the country used for tax reporting.
    /// </summary>
    public CurrencyTypes? CurrencyId { get; set; }

    /// <summary>
    /// Secondary currency for countries that changed their currency (e.g., Croatia from 2023)
    /// </summary>
    public CurrencyTypes? CurrencyId2 { get; set; }

    /// <summary>
    /// Date from which the secondary currency is valid
    /// </summary>
    public DateTime? CurrencyId2ValidFrom { get; set; }

    /// <summary>
    /// The date from which the country is considered an EU member
    /// </summary>
    public DateTime? IsEuCountryFrom { get; set; }

    /// <summary>
    /// The date until which the country is considered an EU member (null = still member)
    /// </summary>
    public DateTime? IsEuCountryTo { get; set; }

    /// <summary>
    /// Checks if the country is an EU member at the specified date.
    /// </summary>
    public bool IsEuCountry(DateTime date)
    {
        return IsEuCountryFrom <= date && (IsEuCountryTo == null || IsEuCountryTo >= date);
    }

    /// <summary>
    /// Gets the country currency applicable at the specified date.
    /// Handles currency transitions (e.g., Croatia HRK to EUR).
    /// </summary>
    public CurrencyTypes? GetCountryCurrency(DateTime date)
    {
        if (CurrencyId2ValidFrom.HasValue &&
            CurrencyId2.HasValue &&
            CurrencyId2ValidFrom <= date)
        {
            return CurrencyId2.Value;
        }

        return CurrencyId;
    }

    /// <summary>
    /// Tries to get a country by its country code.
    /// </summary>
    public static bool TryGetCountry(string countryCode, [NotNullWhen(true)] out Country? country)
    {
        country = InitialData.SingleOrDefault(c => c.Code.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
        return country != null;
    }

    /// <summary>
    /// Static initial data - simplified subset of countries for exchange rate testing.
    /// In production, this would be loaded from database.
    /// </summary>
    public static readonly IReadOnlyList<Country> InitialData = new List<Country>
    {
        // EU Countries
        new() { Id = 1, Code = "AT", Name = "Austria", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1995, 01, 01) },
        new() { Id = 2, Code = "BE", Name = "Belgium", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1958, 01, 01) },
        new() { Id = 3, Code = "BG", Name = "Bulgaria", CurrencyId = CurrencyTypes.BGN, IsEuCountryFrom = new DateTime(2007, 01, 01) },
        new() { Id = 4, Code = "HR", Name = "Croatia", CurrencyId = CurrencyTypes.HRK, CurrencyId2 = CurrencyTypes.EUR, CurrencyId2ValidFrom = new DateTime(2023, 01, 01), IsEuCountryFrom = new DateTime(2013, 07, 01) },
        new() { Id = 5, Code = "CY", Name = "Cyprus", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(2004, 05, 01) },
        new() { Id = 6, Code = "CZ", Name = "Czech Republic", CurrencyId = CurrencyTypes.CZK, IsEuCountryFrom = new DateTime(2004, 05, 01) },
        new() { Id = 7, Code = "DK", Name = "Denmark", CurrencyId = CurrencyTypes.DKK, IsEuCountryFrom = new DateTime(1973, 01, 01) },
        new() { Id = 8, Code = "EE", Name = "Estonia", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(2004, 05, 01) },
        new() { Id = 9, Code = "FI", Name = "Finland", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1995, 01, 01) },
        new() { Id = 10, Code = "FR", Name = "France", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1958, 01, 01) },
        new() { Id = 11, Code = "DE", Name = "Germany", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1958, 01, 01) },
        new() { Id = 12, Code = "GR", Name = "Greece", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1981, 01, 01) },
        new() { Id = 13, Code = "HU", Name = "Hungary", CurrencyId = CurrencyTypes.HUF, IsEuCountryFrom = new DateTime(2004, 05, 01) },
        new() { Id = 14, Code = "IE", Name = "Ireland", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1973, 01, 01) },
        new() { Id = 15, Code = "IT", Name = "Italy", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1958, 01, 01) },
        new() { Id = 16, Code = "LV", Name = "Latvia", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(2004, 05, 01) },
        new() { Id = 17, Code = "LT", Name = "Lithuania", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(2004, 05, 01) },
        new() { Id = 18, Code = "LU", Name = "Luxembourg", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1958, 01, 01) },
        new() { Id = 19, Code = "MT", Name = "Malta", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(2004, 05, 01) },
        new() { Id = 20, Code = "NL", Name = "Netherlands", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1958, 01, 01) },
        new() { Id = 21, Code = "PL", Name = "Poland", CurrencyId = CurrencyTypes.PLN, IsEuCountryFrom = new DateTime(2004, 05, 01) },
        new() { Id = 22, Code = "PT", Name = "Portugal", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1986, 01, 01) },
        new() { Id = 23, Code = "RO", Name = "Romania", CurrencyId = CurrencyTypes.RON, IsEuCountryFrom = new DateTime(2007, 01, 01) },
        new() { Id = 24, Code = "SK", Name = "Slovakia", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(2004, 05, 01) },
        new() { Id = 25, Code = "SI", Name = "Slovenia", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(2004, 05, 01) },
        new() { Id = 26, Code = "ES", Name = "Spain", CurrencyId = CurrencyTypes.EUR, IsEuCountryFrom = new DateTime(1986, 01, 01) },
        new() { Id = 27, Code = "SE", Name = "Sweden", CurrencyId = CurrencyTypes.SEK, IsEuCountryFrom = new DateTime(1995, 01, 01) },

        // UK (former EU member)
        new() { Id = 28, Code = "GB", Name = "United Kingdom", CurrencyId = CurrencyTypes.GBP, IsEuCountryFrom = new DateTime(1973, 01, 01), IsEuCountryTo = new DateTime(2020, 12, 31) },

        // Non-EU countries used in exchange rate calculations
        new() { Id = 29, Code = "CH", Name = "Switzerland", CurrencyId = CurrencyTypes.CHF },
        new() { Id = 30, Code = "NO", Name = "Norway", CurrencyId = CurrencyTypes.NOK },
        new() { Id = 31, Code = "US", Name = "United States", CurrencyId = CurrencyTypes.USD },

        // Special countries referenced in ExchangeRateService
        new() { Id = 32, Code = BELARUS_COUNTRYCODE, Name = "Belarus", CurrencyId = CurrencyTypes.BYN },
        new() { Id = 33, Code = COLOMBIA_COUNRTYCODE, Name = "Colombia", CurrencyId = CurrencyTypes.COP },
        new() { Id = 34, Code = KAZAKHSTAN_COUNRTYCODE, Name = "Kazakhstan", CurrencyId = CurrencyTypes.KZT },
        new() { Id = 35, Code = SOUTHKOREA_COUNTRYCODE, Name = "South Korea", CurrencyId = CurrencyTypes.KRW },
        new() { Id = 36, Code = THAILAND_COUNTRYCODE, Name = "Thailand", CurrencyId = CurrencyTypes.THB },

        // Technical EU scheme countries
        new() { Id = 100, Code = EU_IOSS_COUNTRYCODE, Name = "EU IOSS", CurrencyId = CurrencyTypes.EUR },
        new() { Id = 101, Code = EU_OSS_COUNTRYCODE, Name = "EU OSS", CurrencyId = CurrencyTypes.EUR },
        new() { Id = 102, Code = EU_OSS_NON_UNION_COUNTRYCODE, Name = "EU OSS Non-Union", CurrencyId = CurrencyTypes.EUR },
    };
}
