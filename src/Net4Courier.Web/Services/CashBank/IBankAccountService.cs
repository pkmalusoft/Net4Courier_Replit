using Net4Courier.Web.DTOs;

namespace Net4Courier.Web.Services.CashBank;

public interface IBankAccountService
{
    Task<List<BankAccountListDto>> GetAllAsync(Guid tenantId);
    Task<BankAccountDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<BankAccountDto?> CreateAsync(Guid tenantId, CreateBankAccountRequest request);
    Task<bool> UpdateAsync(Guid tenantId, Guid id, UpdateBankAccountRequest request);
    Task<bool> DeleteAsync(Guid tenantId, Guid id);
    Task<List<BankAccountListDto>> GetActiveAsync(Guid tenantId);
}
