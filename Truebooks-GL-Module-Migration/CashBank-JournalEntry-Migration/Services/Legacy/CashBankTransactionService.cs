using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;


using Truebooks.Platform.Contracts.Legacy.DTOs;
using Truebooks.Platform.Contracts.Legacy.Enums;

namespace Truebooks.Shared.UI.Services.Legacy;

public interface ICashBankTransactionService
{
    Task<List<CashBankTransaction>> GetAllAsync(TransactionType? transactionType = null, RecPayType? recPayType = null, CashBankStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null, bool includeVoided = false);
    Task<CashBankTransaction?> GetByIdAsync(Guid id);
    Task<string> GenerateVoucherNoAsync(TransactionType transactionType, RecPayType recPayType);
    Task<Guid> CreateAsync(CashBankTransaction transaction);
    Task UpdateAsync(Guid id, CashBankTransaction transaction);
    Task DeleteAsync(Guid id);
    Task PostAsync(Guid id);
    Task<bool> VoucherNoExistsAsync(string voucherNo, Guid? excludeId = null);
    Task<bool> ReferenceNoExistsAsync(string referenceNo, Guid? excludeId = null);
    Task<bool> ChequeNoExistsAsync(string chequeNo, Guid? excludeId = null);
}

public class CashBankTransactionService : ICashBankTransactionService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CashBankTransactionService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    private void SetAuthHeader()
    {
        var token = _authService.GetAccessToken();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        var tenantId = _authService.GetTenantId();
        if (!string.IsNullOrEmpty(tenantId))
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        }
    }

    public async Task<List<CashBankTransaction>> GetAllAsync(
        TransactionType? transactionType = null,
        RecPayType? recPayType = null,
        CashBankStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool includeVoided = false)
    {
        SetAuthHeader();
        var queryParams = new List<string>();
        
        if (transactionType.HasValue)
            queryParams.Add($"transactionType={transactionType.Value}");
        if (recPayType.HasValue)
            queryParams.Add($"recPayType={recPayType.Value}");
        if (status.HasValue)
            queryParams.Add($"status={status.Value}");
        if (fromDate.HasValue)
            queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue)
            queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (includeVoided)
            queryParams.Add("includeVoided=true");

        var url = "api/CashBankTransaction";
        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        var result = await _httpClient.GetFromJsonAsync<List<CashBankTransaction>>(url, _jsonOptions);
        return result ?? new List<CashBankTransaction>();
    }

    public async Task<CashBankTransaction?> GetByIdAsync(Guid id)
    {
        SetAuthHeader();
        var result = await _httpClient.GetFromJsonAsync<CashBankTransaction>($"api/CashBankTransaction/{id}", _jsonOptions);
        return result;
    }

    public async Task<string> GenerateVoucherNoAsync(TransactionType transactionType, RecPayType recPayType)
    {
        SetAuthHeader();
        var url = $"api/CashBankTransaction/generate-voucher-no?transactionType={transactionType}&recPayType={recPayType}";
        var result = await _httpClient.GetFromJsonAsync<string>(url, _jsonOptions);
        return result ?? string.Empty;
    }

    public async Task<Guid> CreateAsync(CashBankTransaction transaction)
    {
        SetAuthHeader();
        var response = await _httpClient.PostAsJsonAsync("api/CashBankTransaction", transaction);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent}");
        }
        
        var created = await response.Content.ReadFromJsonAsync<CashBankTransaction>(_jsonOptions);
        return created?.Id ?? Guid.Empty;
    }

    public async Task UpdateAsync(Guid id, CashBankTransaction transaction)
    {
        SetAuthHeader();
        var response = await _httpClient.PutAsJsonAsync($"api/CashBankTransaction/{id}", transaction);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent}");
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _httpClient.DeleteAsync($"api/CashBankTransaction/{id}");
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent}");
        }
    }

    public async Task PostAsync(Guid id)
    {
        SetAuthHeader();
        var response = await _httpClient.PostAsync($"api/CashBankTransaction/{id}/post", null);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent}");
        }
    }

    public async Task<bool> VoucherNoExistsAsync(string voucherNo, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(voucherNo))
            return false;
            
        SetAuthHeader();
        var url = $"api/CashBankTransaction/validate/voucher-no-exists?voucherNo={Uri.EscapeDataString(voucherNo)}";
        if (excludeId.HasValue)
        {
            url += $"&excludeId={excludeId.Value}";
        }
        var result = await _httpClient.GetFromJsonAsync<bool>(url);
        return result;
    }

    public async Task<bool> ReferenceNoExistsAsync(string referenceNo, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(referenceNo))
            return false;
            
        SetAuthHeader();
        var url = $"api/CashBankTransaction/validate/reference-no-exists?referenceNo={Uri.EscapeDataString(referenceNo)}";
        if (excludeId.HasValue)
        {
            url += $"&excludeId={excludeId.Value}";
        }
        var result = await _httpClient.GetFromJsonAsync<bool>(url);
        return result;
    }

    public async Task<bool> ChequeNoExistsAsync(string chequeNo, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(chequeNo))
            return false;
            
        SetAuthHeader();
        var url = $"api/CashBankTransaction/validate/cheque-no-exists?chequeNo={Uri.EscapeDataString(chequeNo)}";
        if (excludeId.HasValue)
        {
            url += $"&excludeId={excludeId.Value}";
        }
        var result = await _httpClient.GetFromJsonAsync<bool>(url);
        return result;
    }
}
