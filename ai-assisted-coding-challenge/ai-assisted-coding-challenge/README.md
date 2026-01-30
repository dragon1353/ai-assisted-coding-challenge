# Exchange Rate Management System

## Task

You are given an Exchange Rate management system that needs refactoring.

### Rules
1. **GetRate must work** - The `GetRate` method must continue to return correct exchange rates
2. **All tests must pass** - Run `dotnet test` to verify
3. **Interface can change** - You may simplify, remove methods, change signatures
4. **AI assistance is required** - You must use AI tools (Claude Code, GitHub Copilot, ChatGPT, Cursor, or similar) during this exercise

### What We Evaluate
- **How you use AI** - Your prompts, follow-up questions, and critical evaluation of AI suggestions
- Your refactoring decisions and reasoning
- Understanding of the business domain
- Code quality improvements
- Whether you go beyond obvious/surface-level changes

### Deliverables
1. Refactored code with all tests passing
2. Brief explanation of your changes (can be comments or separate doc)
3. **Complete AI interaction history** - Submit your chat logs, Claude Code session export, conversation history, or equivalent record of your AI usage

### Business Scenario

The system must be able to handle these real-world situations:
1. A rate for a specific date was never provided by the source - the system should be able to fetch and store this missing rate on-demand
2. A rate was provided incorrectly and later corrected - the old rate must be replaced with the corrected one

Currently, neither scenario can be handled efficiently by the system.

### Time
Take as long as you need. We care about quality of thinking, not speed.

---

A .NET application for managing and retrieving currency exchange rates from various central bank sources. The system supports multiple rate frequencies, pegged currency conversions, and cross-currency calculations.

## Prerequisites

- .NET 8.0 SDK

## Building the Project

```bash
dotnet build
```

## Running Tests

```bash
dotnet test
```

## Project Structure

```
/
├── src/
│   └── ExchangeRate.Core/           # Core library
│       ├── Entities/                # Domain entities (ExchangeRate, PeggedCurrency, Country)
│       ├── Enums/                   # Enumerations (CurrencyTypes, ExchangeRateSources, etc.)
│       ├── Exceptions/              # Custom exception types
│       ├── Helpers/                 # Utility classes (PeriodHelper, AsyncUtil)
│       ├── Infrastructure/          # Data store interfaces
│       ├── Interfaces/              # Core interfaces and provider contracts
│       ├── Models/                  # Data transfer objects
│       └── Providers/               # Exchange rate provider implementations
├── tests/
│   └── ExchangeRate.Tests/          # Unit tests
│       └── TestDoubles/             # Test infrastructure (in-memory data store)
└── ExchangeRateRefactoring.sln      # Solution file
```

## Domain Overview

### Exchange Rate Sources

Exchange rates are fetched from various central banks and financial authorities worldwide. Some of the supported sources include:

- **ECB** - European Central Bank
- **HMRC** - Her Majesty's Revenue and Customs (UK)
- **MNB** - Magyar Nemzeti Bank (Hungary)
- **PLCB** - National Bank of Poland
- **SECB** - Sveriges Riksbank (Sweden)
- **UNTR** - United Nations Treasury (fallback source)

The system supports over 70 different exchange rate sources from central banks across Europe, Asia, Americas, Africa, and the Middle East.

### Rate Frequencies

Exchange rates can be published at different intervals depending on the source:

- **Daily** - Most common; rates published each business day
- **Weekly** - Rates published once per week
- **BiWeekly** - Rates published every two weeks
- **Monthly** - Monthly average rates

### Pegged Currencies

Some currencies maintain a fixed exchange rate (peg) to another currency. The system handles these automatically:

- **AED** (UAE Dirham) - pegged to USD
- **SAR** (Saudi Riyal) - pegged to USD
- **HKD** (Hong Kong Dollar) - pegged to USD
- **BAM** (Bosnia-Herzegovina Convertible Mark) - pegged to EUR
- **XOF** (West African CFA Franc) - pegged to EUR

When converting to or from a pegged currency, the system uses the fixed peg rate combined with the exchange rate of the anchor currency.

### Cross-Currency Conversion

When converting between two currencies where neither matches the provider's base currency, the system performs a cross-currency calculation. For example, converting GBP to PLN via ECB (which uses EUR as base):

1. Get GBP to EUR rate
2. Get EUR to PLN rate
3. Multiply the rates to get GBP to PLN

## Key Classes

### ExchangeRateRepository

The main data access component responsible for:

- Retrieving exchange rates for specific currency pairs and dates
- Caching rates in memory for performance
- Loading historical rates from the data store
- Fetching missing rates from external providers
- Handling pegged currency conversions
- Supporting both direct rate lookups and monthly averages

### IExchangeRateProvider / Provider Implementations

The provider interface defines the contract for exchange rate sources:

- `Currency` - The base currency of the provider (e.g., EUR for ECB)
- `QuoteType` - Whether rates are quoted directly or indirectly
- `Source` - The exchange rate source identifier

Concrete implementations fetch rates from specific central banks:

- `EUECBExchangeRateProvider` - European Central Bank
- `GBHMRCExchangeRateProvider` - UK HMRC
- `HUCBExchangeRateProvider` - Hungarian National Bank
- `PLCBExchangeRateProvider` - National Bank of Poland
- `SECBExchangeRateProvider` - Swedish Riksbank

### ExchangeRateProviderFactory

Creates and manages exchange rate provider instances:

- Resolves providers by source type
- Resolves providers by currency (to find the natural source for a given currency)
- Lists all available exchange rate sources

### Supporting Entities

- `ExchangeRate` - Represents a single exchange rate record with date, currency, source, frequency, and rate value
- `PeggedCurrency` - Defines fixed exchange rate relationships between currencies
- `Country` - Contains country information including currency associations and EU membership dates
