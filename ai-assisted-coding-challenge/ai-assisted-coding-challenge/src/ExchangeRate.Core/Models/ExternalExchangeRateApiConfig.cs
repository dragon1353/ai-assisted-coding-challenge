namespace ExchangeRate.Core.Models
{
    public class ExternalExchangeRateApiConfig
    {
        public string BaseAddress { get; set; }
        public string TokenEndpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
