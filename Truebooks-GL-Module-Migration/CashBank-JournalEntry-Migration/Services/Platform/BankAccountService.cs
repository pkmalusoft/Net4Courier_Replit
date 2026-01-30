using Truebooks.Platform.Contracts.DTOs.Finance;
using Truebooks.Platform.Contracts.Services;

namespace Truebooks.Platform.Host.Services;

public class BankAccountService : IBankAccountService
{
    public Task<List<BankAccountListDto>> GetAllAsync(Guid tenantId)
        => Task.FromResult(new List<BankAccountListDto>());

    public Task<BankAccountDto?> GetByIdAsync(Guid tenantId, Guid id)
        => Task.FromResult<BankAccountDto?>(null);

    public Task<BankAccountDto?> CreateAsync(Guid tenantId, CreateBankAccountRequest request)
        => Task.FromResult<BankAccountDto?>(null);

    public Task<bool> UpdateAsync(Guid tenantId, Guid id, UpdateBankAccountRequest request)
        => Task.FromResult(false);

    public Task<bool> DeleteAsync(Guid tenantId, Guid id)
        => Task.FromResult(false);

    public Task<List<BankAccountListDto>> GetActiveAsync(Guid tenantId)
        => Task.FromResult(new List<BankAccountListDto>());
}
