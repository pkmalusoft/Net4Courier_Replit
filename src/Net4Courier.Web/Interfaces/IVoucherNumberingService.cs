using Truebooks.Platform.Contracts.DTOs;

namespace Truebooks.Platform.Contracts.Services;

public interface IVoucherNumberingService
{
    Task<IEnumerable<VoucherNumberingDto>> GetAllAsync(Guid tenantId);
    Task<VoucherNumberingDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<VoucherNumberingDto> CreateAsync(Guid tenantId, CreateVoucherNumberingRequest request);
    Task<VoucherNumberingDto> UpdateAsync(Guid tenantId, Guid id, UpdateVoucherNumberingRequest request);
    Task DeleteAsync(Guid tenantId, Guid id);
}
