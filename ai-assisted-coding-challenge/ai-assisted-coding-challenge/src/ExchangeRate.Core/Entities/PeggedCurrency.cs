using System.Collections.Generic;
using ExchangeRate.Core.Enums;

namespace ExchangeRate.Core.Entities;

public class PeggedCurrency
{
    public CurrencyTypes? CurrencyId { get; set; }

    public CurrencyTypes? PeggedTo { get; set; }

    public decimal? Rate { get; set; }

    internal static IEnumerable<PeggedCurrency> InitialData =>
        new List<PeggedCurrency>()
        {
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.XCD,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.37007m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.DJF,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.00562m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.HKD,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.12850m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.BAM,
                PeggedTo = CurrencyTypes.EUR,
                Rate = 0.60000m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.XPF,
                PeggedTo = CurrencyTypes.EUR,
                Rate = 0.00838m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.BND,
                PeggedTo = CurrencyTypes.SGD,
                Rate = 1.00000m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.MOP,
                PeggedTo = CurrencyTypes.HKD,
                Rate = 0.16890m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.AWG,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.55866m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.BSD,
                PeggedTo = CurrencyTypes.USD,
                Rate = 1.00000m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.BHD,
                PeggedTo = CurrencyTypes.USD,
                Rate = 2.65957m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.BBD,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.50000m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.BZD,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.49600m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.ANG,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.55900m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.ERN,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.06667m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.JOD,
                PeggedTo = CurrencyTypes.USD,
                Rate = 1.41044m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.OMR,
                PeggedTo = CurrencyTypes.USD,
                Rate = 2.60078m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.PAB,
                PeggedTo = CurrencyTypes.USD,
                Rate = 1.00000m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.QAR,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.27473m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.SAR,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.26667m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.TMT,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.29777m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.AED,
                PeggedTo = CurrencyTypes.USD,
                Rate = 0.27229m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.XOF,
                PeggedTo = CurrencyTypes.EUR,
                Rate = 0.00152m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.CVE,
                PeggedTo = CurrencyTypes.EUR,
                Rate = 0.00907m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.XAF,
                PeggedTo = CurrencyTypes.EUR,
                Rate = 0.00152m
            },
            new PeggedCurrency()
            {
                CurrencyId = CurrencyTypes.KMF,
                PeggedTo = CurrencyTypes.EUR,
                Rate = 0.00203m
            },
        };
}
