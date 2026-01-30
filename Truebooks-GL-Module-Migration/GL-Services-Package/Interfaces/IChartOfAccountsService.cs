using Truebooks.Platform.Contracts.DTOs;

namespace Truebooks.Platform.Contracts.Services;

public interface IChartOfAccountsService
{
    Task<IEnumerable<ChartOfAccountDto>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<ChartOfAccountHierarchyDto>> GetHierarchyAsync(Guid tenantId);
    Task<IEnumerable<ChartOfAccountDto>> GetByTypeAsync(Guid tenantId, Truebooks.Platform.Contracts.DTOs.AccountType accountType);
    Task<IEnumerable<ChartOfAccountDto>> GetPostableAccountsAsync(Guid tenantId);
    Task<ChartOfAccountDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<ChartOfAccountDto> CreateAsync(Guid tenantId, CreateChartOfAccountRequest request);
    Task<ChartOfAccountDto> UpdateAsync(Guid tenantId, Guid id, UpdateChartOfAccountRequest request);
    Task DeleteAsync(Guid tenantId, Guid id);
    Task<bool> HasTransactionsAsync(Guid tenantId, Guid accountId);
    Task<bool> HasAccountsAsync(Guid tenantId);
    Task<bool> CodeExistsAsync(Guid tenantId, string code, Guid? excludeId = null);
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId = null);
    Task SeedFromTemplateAsync(Guid tenantId, string industryType);
    Task ResetFromTemplateAsync(Guid tenantId, string industryType);
    Task<byte[]?> ExportPdfAsync(Guid tenantId);
    Task<byte[]?> ExportExcelAsync(Guid tenantId);
    Task<Dictionary<string, ControlAccountAssignmentDto?>> GetControlAccountsAsync(Guid tenantId);
    Task SetControlAccountAsync(Guid tenantId, Guid accountId, int controlAccountType);
    Task RemoveControlAccountAsync(Guid tenantId, int controlAccountType);
}
