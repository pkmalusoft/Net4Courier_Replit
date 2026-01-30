using Net4Courier.Web.DTOs;

namespace Net4Courier.Web.Services.CashBank;

public class CashBankTransactionService : ICashBankTransactionService
{
    public Task<List<CashBankTransactionDto>> GetAllAsync(Guid tenantId, int? transactionType = null, int? recPayType = null, int? status = null, DateTime? fromDate = null, DateTime? toDate = null, bool includeVoided = false)
        => Task.FromResult(new List<CashBankTransactionDto>());

    public Task<CashBankTransactionDto?> GetByIdAsync(Guid tenantId, Guid id)
        => Task.FromResult<CashBankTransactionDto?>(null);

    public Task<string> GenerateVoucherNoAsync(Guid tenantId, int transactionType, int recPayType)
    {
        var prefix = transactionType == 0 ? "BK" : "CH";
        var suffix = recPayType == 0 ? "R" : "P";
        return Task.FromResult($"{prefix}{suffix}-{DateTime.Now:yyyyMMdd}-001");
    }

    public Task<Guid> CreateAsync(Guid tenantId, CreateCashBankTransactionRequest request)
        => Task.FromResult(Guid.NewGuid());

    public Task UpdateAsync(Guid tenantId, Guid id, UpdateCashBankTransactionRequest request)
        => Task.CompletedTask;

    public Task DeleteAsync(Guid tenantId, Guid id)
        => Task.CompletedTask;

    public Task PostAsync(Guid tenantId, Guid id)
        => Task.CompletedTask;

    public Task VoidAsync(Guid tenantId, Guid id, string voidReason)
        => Task.CompletedTask;

    public Task<bool> VoucherNoExistsAsync(Guid tenantId, string voucherNo, Guid? excludeId = null)
        => Task.FromResult(false);

    public Task<bool> ReferenceNoExistsAsync(Guid tenantId, string referenceNo, Guid? excludeId = null)
        => Task.FromResult(false);

    public Task<bool> ChequeNoExistsAsync(Guid tenantId, string chequeNo, Guid? excludeId = null)
        => Task.FromResult(false);

    public Task<Dictionary<Guid, int>> GetAttachmentCountsAsync(Guid tenantId, List<Guid> transactionIds)
        => Task.FromResult(new Dictionary<Guid, int>());
}
