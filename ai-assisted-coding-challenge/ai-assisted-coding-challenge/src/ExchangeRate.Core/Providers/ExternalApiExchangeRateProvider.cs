using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRate.Core.Enums;
using ExchangeRate.Core.Interfaces.Providers;
using ExchangeRate.Core.Models;
using ExchangeRateEntity = ExchangeRate.Core.Entities.ExchangeRate;

namespace ExchangeRate.Core.Providers
{
    /// <summary>
    /// Base class for all external API exchange rate providers.
    /// </summary>
    public abstract class ExternalApiExchangeRateProvider : IExchangeRateProvider
    {
        private static readonly Dictionary<string, CurrencyTypes> CurrencyMapping;

        private readonly HttpClient _httpClient;
        private readonly ExternalExchangeRateApiConfig _externalExchangeRateApiConfig;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public abstract CurrencyTypes Currency { get; }

        public abstract QuoteTypes QuoteType { get; }

        public abstract ExchangeRateSources Source { get; }

        public abstract string BankId { get; }

        public abstract Task<IEnumerable<ExchangeRateEntity>> GetExchangeRatesAsync(DateTime startDate, DateTime endDate, ExchangeRateFrequencies frequency, CancellationToken ct = default);

        static ExternalApiExchangeRateProvider()
        {
            var currencies = Enum.GetValues(typeof(CurrencyTypes)).Cast<CurrencyTypes>().ToList();
            CurrencyMapping = currencies.ToDictionary(x => x.ToString().ToUpperInvariant());
        }

        public ExternalApiExchangeRateProvider(HttpClient httpClient, ExternalExchangeRateApiConfig externalExchangeRateApiConfig)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.BaseAddress = new Uri(externalExchangeRateApiConfig.BaseAddress);
            _externalExchangeRateApiConfig = externalExchangeRateApiConfig ?? throw new ArgumentNullException(nameof(externalExchangeRateApiConfig));

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        protected async Task<IEnumerable<ExchangeRateEntity>> GetDailyRatesAsync(string bankId, (DateTime startDate, DateTime endDate) period = default, CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder($"/v1/Banks/{bankId}/DailyRates/");

            if (period == default)
            {
                sb.Append("Latest");
            }
            else
            {
                sb.Append($"TimeSeries?startDate={period.startDate:yyyy-MM-dd}&endDate={period.endDate:yyyy-MM-dd}");
            }

            var exchangeRates = await GetExchangeRatesAsync(sb.ToString(), cancellationToken);

            return GetExchangeRates(exchangeRates, Source, ExchangeRateFrequencies.Daily);
        }

        protected async Task<IEnumerable<ExchangeRateEntity>> GetMonthlyRatesAsync(string bankId, (int year, int month) period = default, CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder($"/v1/Banks/{bankId}/MonthlyRates/");

            if (period == default)
            {
                sb.Append("Latest");
            }
            else
            {
                sb.Append($"{period.year}/{period.month}");
            }

            var exchangeRates = await GetExchangeRatesAsync(sb.ToString(), cancellationToken);

            return GetExchangeRates(exchangeRates, Source, ExchangeRateFrequencies.Monthly);
        }

        protected async Task<IEnumerable<ExchangeRateEntity>> GetWeeklyRatesAsync(string bankId, (int year, int month) period = default, CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder($"/v1/Banks/{bankId}/WeeklyRates/");

            if (period == default)
            {
                sb.Append("Latest");
            }
            else
            {
                sb.Append($"{period.year}/{period.month}");
            }

            var exchangeRates = await GetExchangeRatesAsync(sb.ToString(), cancellationToken);

            return GetExchangeRates(exchangeRates, Source, ExchangeRateFrequencies.Weekly);
        }

        protected async Task<IEnumerable<ExchangeRateEntity>> GetBiWeeklyRatesAsync(string bankId, (int year, int month) period = default, CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder($"/v1/Banks/{bankId}/BiweeklyRates/");

            if (period == default)
            {
                sb.Append("Latest");
            }
            else
            {
                sb.Append($"{period.year}/{period.month}");
            }

            var exchangeRates = await GetExchangeRatesAsync(sb.ToString(), cancellationToken);

            return GetExchangeRates(exchangeRates, Source, ExchangeRateFrequencies.BiWeekly);
        }

        private IEnumerable<ExchangeRateEntity> GetExchangeRates(ExchangeRates exchangeRates, ExchangeRateSources source, ExchangeRateFrequencies frequency)
        {
            return exchangeRates.Rates.SelectMany(
                pair => pair.Value.Select(
                    innerPair =>
                    {
                        ExchangeRateEntity exchangeRate;
                        try
                        {
                            exchangeRate = new ExchangeRateEntity
                            {
                                Date = pair.Key,
                                Frequency = frequency,
                                Rate = innerPair.Value.GetAbsoluteRate(),
                                Source = source
                            };
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to get the absolute rate for {innerPair.Key} {innerPair.Value} on {pair.Key}, {frequency} at {source}.", ex);
                        }

                        if (CurrencyMapping.TryGetValue(innerPair.Key, out var currencyId))
                            exchangeRate.CurrencyId = currencyId;

                        return exchangeRate;
                    }).Where(x => x.CurrencyId.HasValue));
        }

        private async Task<ExchangeRates> GetExchangeRatesAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            var token = await GetTokenAsync(cancellationToken);

            using var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(requestUri, UriKind.Relative);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Exchange rate API request failed. " +
                    $"BankId: {BankId}, " +
                    $"Source: {Source}, " +
                    $"RequestUri: {requestUri}, " +
                    $"StatusCode: {(int)response.StatusCode} ({response.StatusCode}), " +
                    $"ReasonPhrase: {response.ReasonPhrase}, " +
                    $"ResponseBody: {responseBody}");
            }

            return await response.Content.ReadFromJsonAsync<ExchangeRates>(_jsonSerializerOptions, cancellationToken);
        }

        private async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            using var tokenRequestContent = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", _externalExchangeRateApiConfig.ClientId },
                    { "client_secret", _externalExchangeRateApiConfig.ClientSecret },
                    { "scope", "fx_api" },
                });

            using var response = await _httpClient.PostAsync(_externalExchangeRateApiConfig.TokenEndpoint, tokenRequestContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Exchange rate API token request failed. " +
                    $"BankId: {BankId}, " +
                    $"Source: {Source}, " +
                    $"TokenEndpoint: {_externalExchangeRateApiConfig.TokenEndpoint}, " +
                    $"ClientId: {_externalExchangeRateApiConfig.ClientId}, " +
                    $"StatusCode: {(int)response.StatusCode} ({response.StatusCode}), " +
                    $"ReasonPhrase: {response.ReasonPhrase}, " +
                    $"ResponseBody: {responseBody}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
            return tokenResponse.access_token;
        }

        record TokenResponse
        {
            public string access_token { get; set; }
        }

        record ExchangeRates
        {
            public string BankId { get; set; }

            public string BaseCurrency { get; set; }

            public ExchangeRateQuoteType QuoteType { get; set; }

            public Dictionary<DateTime, Dictionary<string, Rate>> Rates { get; set; }
        }

        record Rate
        {
            [JsonPropertyName("rate")]
            public decimal Value { get; set; }

            public int? UnitMultiplier { get; set; }

            public decimal GetAbsoluteRate() => Value / (decimal)Math.Pow(10, UnitMultiplier ?? 0);
        }

        enum ExchangeRateQuoteType
        {
            Direct,
            Indirect
        }
    }
}
