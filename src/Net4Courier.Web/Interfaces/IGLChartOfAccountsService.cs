using Net4Courier.Finance.Entities;

namespace Net4Courier.Web.Interfaces;

public interface IGLChartOfAccountsService
{
    Task<IEnumerable<GLChartOfAccountDto>> GetAllAsync(long? companyId = null);
    Task<IEnumerable<GLChartOfAccountDto>> GetByTypeAsync(long? companyId, string? accountType);
    Task<IEnumerable<GLChartOfAccountDto>> GetPostableAccountsAsync(long? companyId = null);
    Task<GLChartOfAccountDto?> GetByIdAsync(long id);
    Task<GLChartOfAccountDto> CreateAsync(CreateGLChartOfAccountRequest request);
    Task<GLChartOfAccountDto> UpdateAsync(long id, UpdateGLChartOfAccountRequest request);
    Task DeleteAsync(long id);
    Task<bool> HasTransactionsAsync(long accountId);
    Task<bool> HasAccountsAsync(long? companyId = null);
    Task<bool> CodeExistsAsync(string code, long? excludeId = null);
    Task<bool> NameExistsAsync(string name, long? excludeId = null);
    Task<IEnumerable<GLAccountClassificationDto>> GetClassificationsAsync(long? companyId = null);
}

public record GLChartOfAccountDto(
    long Id,
    long? CompanyId,
    string AccountCode,
    string AccountName,
    string? AccountType,
    long? ParentId,
    string? ParentName,
    bool IsActive,
    bool AllowPosting,
    long? AccountClassificationId,
    string? AccountClassificationName,
    int? ControlAccountType,
    bool IsSystemAccount,
    DateTime CreatedAt,
    DateTime? ModifiedAt
);

public record GLAccountClassificationDto(
    long Id,
    long? CompanyId,
    string Name,
    string? Description,
    bool IsActive
);

public record CreateGLChartOfAccountRequest(
    long? CompanyId,
    string AccountCode,
    string AccountName,
    string? AccountType,
    long? ParentId,
    bool AllowPosting,
    long? AccountClassificationId,
    int? ControlAccountType,
    bool IsActive
);

public record UpdateGLChartOfAccountRequest(
    string AccountCode,
    string AccountName,
    string? AccountType,
    long? ParentId,
    bool AllowPosting,
    long? AccountClassificationId,
    int? ControlAccountType,
    bool IsActive
);
