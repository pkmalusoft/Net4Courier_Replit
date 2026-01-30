using Truebooks.Platform.Contracts.DTOs;
using Truebooks.Platform.Contracts.DTOs.Finance;

namespace Net4Courier.Web.Interfaces;

public interface IAccountClassificationService
{
    Task<IEnumerable<AccountClassificationDto>> GetAllAsync(Guid tenantId);
    Task<AccountClassificationDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<AccountClassificationDto> CreateAsync(Guid tenantId, CreateAccountClassificationRequest request);
    Task<AccountClassificationDto> UpdateAsync(Guid tenantId, Guid id, UpdateAccountClassificationRequest request);
    Task DeleteAsync(Guid tenantId, Guid id);
}
