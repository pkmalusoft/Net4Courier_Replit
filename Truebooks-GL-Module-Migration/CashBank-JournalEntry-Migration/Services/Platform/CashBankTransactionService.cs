using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Truebooks.Platform.Contracts.DTOs;
using Truebooks.Platform.Contracts.Services;
using LegacyDTOs = Truebooks.Platform.Contracts.Legacy.DTOs;

namespace Truebooks.Platform.Host.Services;

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

    private HttpRequestMessage CreateAuthenticatedRequest(HttpMethod method, string requestUri, Guid tenantId)
    {
        var request = new HttpRequestMessage(method, requestUri);
        var token = _authService.GetAccessToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return request;
    }

    private static string GetTransactionTypeText(LegacyDTOs.TransactionType type) => type switch
    {
        LegacyDTOs.TransactionType.Bank => "Bank",
        LegacyDTOs.TransactionType.Cash => "Cash",
        _ => "Unknown"
    };

    private static string GetRecPayTypeText(LegacyDTOs.RecPayType type) => type switch
    {
        LegacyDTOs.RecPayType.Receipt => "Receipt",
        LegacyDTOs.RecPayType.Payment => "Payment",
        _ => "Unknown"
    };

    private static string GetTransactionCategoryText(LegacyDTOs.TransactionCategory category) => category switch
    {
        LegacyDTOs.TransactionCategory.GL => "General Ledger",
        LegacyDTOs.TransactionCategory.PartyReceipt => "Party Receipt",
        LegacyDTOs.TransactionCategory.PartyPayment => "Party Payment",
        _ => "Unknown"
    };

    private static string GetStatusText(LegacyDTOs.CashBankStatus status) => status switch
    {
        LegacyDTOs.CashBankStatus.Draft => "Draft",
        LegacyDTOs.CashBankStatus.Posted => "Posted",
        LegacyDTOs.CashBankStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };

    private static CashBankTransactionDto MapToDto(LegacyDTOs.CashBankTransaction tx, Guid tenantId)
    {
        return new CashBankTransactionDto(
            tx.Id,
            tenantId,
            DateTime.UtcNow,
            null,
            tx.VoucherNo,
            tx.VoucherDate,
            (int)tx.TransactionType,
            GetTransactionTypeText(tx.TransactionType),
            (int)tx.RecPayType,
            GetRecPayTypeText(tx.RecPayType),
            (int)tx.TransactionCategory,
            GetTransactionCategoryText(tx.TransactionCategory),
            tx.SourceAccountId,
            tx.SourceAccount?.AccountCode ?? "",
            tx.SourceAccount?.AccountName ?? "",
            tx.BankAccountId,
            null,
            tx.TotalAmount,
            tx.ChequeNo,
            tx.ChequeDate,
            tx.IsPDC,
            tx.BankName,
            tx.BranchName,
            tx.ReferenceNo,
            (int)tx.Status,
            GetStatusText(tx.Status),
            (int)tx.ReceiptType,
            tx.CustomerId,
            tx.Customer?.Name,
            tx.VendorId,
            tx.Vendor?.Name,
            (int)tx.DepositStatus,
            tx.ActualDepositDate,
            (int)tx.ClearanceStatus,
            tx.ClearanceDate,
            tx.BouncedReason,
            tx.PostedDate,
            tx.PostedByUserId,
            tx.IsVoided,
            tx.VoidedDate,
            tx.VoidedByUserId,
            tx.VoidReason,
            tx.JournalEntryId,
            tx.BranchId,
            tx.OrganizationBranchName,
            tx.DepartmentId,
            tx.OrganizationDepartmentName,
            tx.Lines.Select(l => new CashBankTransactionLineDto(
                l.Id,
                tenantId,
                DateTime.UtcNow,
                l.CashBankTransactionId,
                l.DestinationAccountId,
                l.DestinationAccount?.AccountCode ?? "",
                l.DestinationAccount?.AccountName ?? "",
                l.GrossAmount,
                l.TaxRate,
                l.IsTaxInclusive,
                l.NetAmount,
                l.TaxAmount,
                l.ProjectId,
                l.Project?.Name,
                l.LineNumber,
                l.Description
            )).ToList()
        );
    }

    public async Task<List<CashBankTransactionDto>> GetAllAsync(Guid tenantId, int? transactionType = null, int? recPayType = null, int? status = null, DateTime? fromDate = null, DateTime? toDate = null, bool includeVoided = false)
    {
        var queryParams = new List<string>();
        if (transactionType.HasValue)
            queryParams.Add($"transactionType={(LegacyDTOs.TransactionType)transactionType.Value}");
        if (recPayType.HasValue)
            queryParams.Add($"recPayType={(LegacyDTOs.RecPayType)recPayType.Value}");
        if (status.HasValue)
            queryParams.Add($"status={(LegacyDTOs.CashBankStatus)status.Value}");
        if (fromDate.HasValue)
            queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue)
            queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (includeVoided)
            queryParams.Add("includeVoided=true");

        var url = "api/CashBankTransaction";
        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        var request = CreateAuthenticatedRequest(HttpMethod.Get, url, tenantId);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent}");
        }
        var legacyList = await response.Content.ReadFromJsonAsync<List<LegacyDTOs.CashBankTransaction>>(_jsonOptions) ?? new();
        return legacyList.Select(tx => MapToDto(tx, tenantId)).ToList();
    }

    public async Task<CashBankTransactionDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        var request = CreateAuthenticatedRequest(HttpMethod.Get, $"api/CashBankTransaction/{id}", tenantId);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        var legacy = await response.Content.ReadFromJsonAsync<LegacyDTOs.CashBankTransaction>(_jsonOptions);
        return legacy != null ? MapToDto(legacy, tenantId) : null;
    }

    public async Task<string> GenerateVoucherNoAsync(Guid tenantId, int transactionType, int recPayType)
    {
        var url = $"api/CashBankTransaction/generate-voucher-no?transactionType={(LegacyDTOs.TransactionType)transactionType}&recPayType={(LegacyDTOs.RecPayType)recPayType}";
        var request = CreateAuthenticatedRequest(HttpMethod.Get, url, tenantId);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return string.Empty;
        var result = await response.Content.ReadFromJsonAsync<string>(_jsonOptions);
        return result ?? string.Empty;
    }

    public async Task<Guid> CreateAsync(Guid tenantId, CreateCashBankTransactionRequest createRequest)
    {
        var legacyDto = new LegacyDTOs.CashBankTransaction
        {
            VoucherNo = createRequest.VoucherNo ?? "",
            VoucherDate = createRequest.VoucherDate,
            TransactionType = (LegacyDTOs.TransactionType)createRequest.TransactionType,
            RecPayType = (LegacyDTOs.RecPayType)createRequest.RecPayType,
            TransactionCategory = (LegacyDTOs.TransactionCategory)createRequest.TransactionCategory,
            SourceAccountId = createRequest.SourceAccountId,
            BankAccountId = createRequest.BankAccountId,
            TotalAmount = createRequest.TotalAmount,
            ChequeNo = createRequest.ChequeNo,
            ChequeDate = createRequest.ChequeDate,
            IsPDC = createRequest.IsPDC,
            BankName = createRequest.BankName,
            BranchName = createRequest.BranchName,
            ReferenceNo = createRequest.ReferenceNo,
            ReceiptType = (LegacyDTOs.ReceiptType)createRequest.ReceiptType,
            CustomerId = createRequest.CustomerId,
            VendorId = createRequest.VendorId,
            BranchId = createRequest.BranchId,
            DepartmentId = createRequest.DepartmentId,
            Lines = createRequest.Lines.Select(l => new LegacyDTOs.CashBankTransactionLine
            {
                DestinationAccountId = l.DestinationAccountId,
                GrossAmount = l.GrossAmount,
                TaxRate = l.TaxRate,
                IsTaxInclusive = l.IsTaxInclusive,
                NetAmount = l.NetAmount,
                TaxAmount = l.TaxAmount,
                ProjectId = l.ProjectId,
                LineNumber = l.LineNumber,
                Description = l.Description
            }).ToList()
        };

        var request = CreateAuthenticatedRequest(HttpMethod.Post, "api/CashBankTransaction", tenantId);
        request.Content = new StringContent(JsonSerializer.Serialize(legacyDto, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent}");
        }
        var result = await response.Content.ReadFromJsonAsync<LegacyDTOs.CashBankTransaction>(_jsonOptions);
        return result?.Id ?? Guid.Empty;
    }

    public async Task UpdateAsync(Guid tenantId, Guid id, UpdateCashBankTransactionRequest updateRequest)
    {
        var legacyDto = new LegacyDTOs.CashBankTransaction
        {
            Id = id,
            VoucherDate = updateRequest.VoucherDate,
            TransactionType = (LegacyDTOs.TransactionType)updateRequest.TransactionType,
            RecPayType = (LegacyDTOs.RecPayType)updateRequest.RecPayType,
            TransactionCategory = (LegacyDTOs.TransactionCategory)updateRequest.TransactionCategory,
            SourceAccountId = updateRequest.SourceAccountId,
            BankAccountId = updateRequest.BankAccountId,
            TotalAmount = updateRequest.TotalAmount,
            ChequeNo = updateRequest.ChequeNo,
            ChequeDate = updateRequest.ChequeDate,
            IsPDC = updateRequest.IsPDC,
            BankName = updateRequest.BankName,
            BranchName = updateRequest.BranchName,
            ReferenceNo = updateRequest.ReferenceNo,
            ReceiptType = (LegacyDTOs.ReceiptType)updateRequest.ReceiptType,
            CustomerId = updateRequest.CustomerId,
            VendorId = updateRequest.VendorId,
            BranchId = updateRequest.BranchId,
            DepartmentId = updateRequest.DepartmentId,
            Lines = updateRequest.Lines.Select(l => new LegacyDTOs.CashBankTransactionLine
            {
                DestinationAccountId = l.DestinationAccountId,
                GrossAmount = l.GrossAmount,
                TaxRate = l.TaxRate,
                IsTaxInclusive = l.IsTaxInclusive,
                NetAmount = l.NetAmount,
                TaxAmount = l.TaxAmount,
                ProjectId = l.ProjectId,
                LineNumber = l.LineNumber,
                Description = l.Description
            }).ToList()
        };

        var request = CreateAuthenticatedRequest(HttpMethod.Put, $"api/CashBankTransaction/{id}", tenantId);
        request.Content = new StringContent(JsonSerializer.Serialize(legacyDto, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent}");
        }
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        var request = CreateAuthenticatedRequest(HttpMethod.Delete, $"api/CashBankTransaction/{id}", tenantId);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent}");
        }
    }

    public async Task PostAsync(Guid tenantId, Guid id)
    {
        var request = CreateAuthenticatedRequest(HttpMethod.Post, $"api/CashBankTransaction/{id}/post", tenantId);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent}");
        }
    }

    public async Task<bool> VoucherNoExistsAsync(Guid tenantId, string voucherNo, Guid? excludeId = null)
    {
        var url = $"api/CashBankTransaction/voucher-exists?voucherNo={Uri.EscapeDataString(voucherNo)}";
        if (excludeId.HasValue)
            url += $"&excludeId={excludeId.Value}";
        var request = CreateAuthenticatedRequest(HttpMethod.Get, url, tenantId);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return false;
        return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
    }

    public async Task<bool> ReferenceNoExistsAsync(Guid tenantId, string referenceNo, Guid? excludeId = null)
    {
        var url = $"api/CashBankTransaction/reference-exists?referenceNo={Uri.EscapeDataString(referenceNo)}";
        if (excludeId.HasValue)
            url += $"&excludeId={excludeId.Value}";
        var request = CreateAuthenticatedRequest(HttpMethod.Get, url, tenantId);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return false;
        return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
    }

    public async Task<bool> ChequeNoExistsAsync(Guid tenantId, string chequeNo, Guid? excludeId = null)
    {
        var url = $"api/CashBankTransaction/cheque-exists?chequeNo={Uri.EscapeDataString(chequeNo)}";
        if (excludeId.HasValue)
            url += $"&excludeId={excludeId.Value}";
        var request = CreateAuthenticatedRequest(HttpMethod.Get, url, tenantId);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return false;
        return await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
    }

    public async Task VoidAsync(Guid tenantId, Guid id, string voidReason)
    {
        var request = CreateAuthenticatedRequest(HttpMethod.Post, $"api/CashBankTransaction/{id}/void", tenantId);
        var payload = new { voidReason };
        request.Content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {errorContent}");
        }
    }

    public async Task<Dictionary<Guid, int>> GetAttachmentCountsAsync(Guid tenantId, List<Guid> transactionIds)
    {
        if (transactionIds.Count == 0) return new Dictionary<Guid, int>();
        
        var request = CreateAuthenticatedRequest(HttpMethod.Post, "api/VoucherAttachment/counts/bulk", tenantId);
        request.Content = new StringContent(JsonSerializer.Serialize(transactionIds, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return new Dictionary<Guid, int>();
        return await response.Content.ReadFromJsonAsync<Dictionary<Guid, int>>(_jsonOptions) ?? new Dictionary<Guid, int>();
    }
}
