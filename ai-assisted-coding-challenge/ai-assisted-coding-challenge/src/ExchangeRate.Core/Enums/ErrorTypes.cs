namespace ExchangeRate.Core.Enums;

public enum ErrorTypes
{
    GenericError = 0,
    MissingInput = 1,
    InvalidValue = 2,
    IncorrectFormat = 3,
    OwnVatNumberProvided = 4,
    InvalidVatNumber = 5,
    DuplicateInvoiceNumber = 6,
    MissingOwnVatNumber = 7,
    VatNumberCountryMismatch = 8,
    InvalidVatRateForCountry = 9,
    InvalidVatRateForDate = 10,
    DateBelongsToPreviousPeriod = 11,
    DateBelongsToFuturePeriod = 12,
    InconsistentValue = 13,
    AdjustmentRuleApplied = 14,
    DynamicRuleExecutionError = 15,
}
