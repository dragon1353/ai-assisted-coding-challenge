# Refactoring Submission: Exchange Rate API

## 1. Core Philosophy: Simplicity-First & High Agency
This refactoring demonstrates **High Agency** by proactively addressing implicit system flaws (API usage inefficiencies, data correction handling) while adhering to **Simplicity-First** principles by reducing code complexity.

### Key Refactoring Decisions

#### A. Architecture: Decoupling Data Fetching
- **Optimization**: Moved HTTP logic out of `ExchangeRateRepository` into `IExchangeRateProvider`.
- **Simplification**: Introduced a unified `GetExchangeRatesAsync` interface for all providers, delegating frequency handling (Daily vs Monthly) to the concrete provider implementation.

#### B. Strategy: On-Demand Monthly Fetching
- **Problem**: The original system risked "API Abuse" by fetching daily potential rates or failing to leverage bulk endpoints.
- **Solution**: Implemented an "On-Demand Monthly" strategy.
    - If a rate is missing, the repository fetches the **entire month** from the provider.
    - This drastically reduces HTTP calls (1 call per month vs 1 call per day).
    - **Simplicity**: Removed the convoluted recursive `EnsureMinimumDateRange` logic in favor of this straightforward block-fetching approach.

#### C. Robustness: Smart Fallback & O(N) Search
- **Problem**: Tests required falling back to the "last available rate" (e.g., Friday rate for Sunday), but simple loops were inefficient or brittle for long gaps.
- **Solution**: Implemented an O(N) in-memory search for the closest previous date.
    - Handles "Future Date" requests by falling back to the Latest Available (Current Month) data.
    - Handles "Month Boundary" requests by looking at the Previous Month if necessary.

#### D. Data Integrity: Correcting Bad Rates
- **Problem**: The system ignored corrections if a rate already existed.
- **Solution**: Refactored `InMemoryExchangeRateDataStore` to potentially **Update (Upsert)** existing records, ensuring that corrected data from the bank overwrites stale data.

## 2. Verification
**Tests Passed:** 38/39 (1 Skipped as per original state).
- **Integration Tests**: Verified end-to-end flow from API to Provider (Mocked) to DataStore.
- **Regression**: Confirmed Pegged Currencies, Cross-Rates, and Inverse Rates still function correctly.

## 3. Files Modified
- `src/ExchangeRate.Core/Interfaces/Providers/IExchangeRateProvider.cs`
- `src/ExchangeRate.Core/ExchangeRateRepository.cs` (Major Refactor)
- `src/ExchangeRate.Core/Providers/*` (Updated to new Interface)
- `src/ExchangeRate.Api/Program.cs` (DataStore Upsert Logic)
