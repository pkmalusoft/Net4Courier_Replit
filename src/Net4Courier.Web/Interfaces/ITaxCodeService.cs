using Truebooks.Platform.Contracts.DTOs;
using Truebooks.Platform.Contracts.DTOs.Finance;

namespace Net4Courier.Web.Interfaces;

public interface ITaxCodeService
{
    Task<IEnumerable<TaxCodeDto>> GetAllAsync(Guid tenantId);
    Task<TaxCodeDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<TaxCodeDto> CreateAsync(Guid tenantId, CreateTaxCodeRequest request);
    Task<TaxCodeDto> UpdateAsync(Guid tenantId, Guid id, UpdateTaxCodeRequest request);
    Task DeleteAsync(Guid tenantId, Guid id);
}
