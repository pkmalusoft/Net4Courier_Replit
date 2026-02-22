using System.Text.Json;

namespace Net4Courier.Web.Services;

public class ExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExchangeRateService> _logger;

    public ExchangeRateService(HttpClient httpClient, ILogger<ExchangeRateService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal?> GetExchangeRateAsync(string baseCurrency, string targetCurrency)
    {
        if (string.Equals(baseCurrency, targetCurrency, StringComparison.OrdinalIgnoreCase))
            return 1m;

        try
        {
            var url = $"https://api.frankfurter.dev/v1/latest?base={baseCurrency.ToUpperInvariant()}&symbols={targetCurrency.ToUpperInvariant()}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("rates", out var rates) &&
                rates.TryGetProperty(targetCurrency.ToUpperInvariant(), out var rateValue))
            {
                return rateValue.GetDecimal();
            }

            _logger.LogWarning("Exchange rate not found for {Base} to {Target}", baseCurrency, targetCurrency);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rate from {Base} to {Target}", baseCurrency, targetCurrency);
            return null;
        }
    }
}
