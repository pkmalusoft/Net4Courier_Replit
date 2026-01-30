using Truebooks.Platform.Contracts.DTOs;

namespace Truebooks.Platform.Contracts.Services;

public interface ICashBankTransactionService
{
    Task<List<CashBankTransactionDto>> GetAllAsync(Guid tenantId, int? transactionType = null, int? recPayType = null, int? status = null, DateTime? fromDate = null, DateTime? toDate = null, bool includeVoided = false);
    Task<CashBankTransactionDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<string> GenerateVoucherNoAsync(Guid tenantId, int transactionType, int recPayType);
    Task<Guid> CreateAsync(Guid tenantId, CreateCashBankTransactionRequest request);
    Task UpdateAsync(Guid tenantId, Guid id, UpdateCashBankTransactionRequest request);
    Task DeleteAsync(Guid tenantId, Guid id);
    Task PostAsync(Guid tenantId, Guid id);
    Task VoidAsync(Guid tenantId, Guid id, string voidReason);
    Task<bool> VoucherNoExistsAsync(Guid tenantId, string voucherNo, Guid? excludeId = null);
    Task<bool> ReferenceNoExistsAsync(Guid tenantId, string referenceNo, Guid? excludeId = null);
    Task<bool> ChequeNoExistsAsync(Guid tenantId, string chequeNo, Guid? excludeId = null);
    Task<Dictionary<Guid, int>> GetAttachmentCountsAsync(Guid tenantId, List<Guid> transactionIds);
}
