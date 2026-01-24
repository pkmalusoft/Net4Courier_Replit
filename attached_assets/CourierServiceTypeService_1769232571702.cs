using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface ICourierServiceTypeService
{
    Task<List<CourierServiceType>> GetAllAsync();
    Task<CourierServiceType?> GetByIdAsync(Guid id);
    Task<CourierServiceType?> GetByCodeAsync(string code);
    Task<CourierServiceType> CreateAsync(CourierServiceType serviceType);
    Task<CourierServiceType> UpdateAsync(CourierServiceType serviceType);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string code, Guid? excludeId = null);
}

public class CourierServiceTypeService : ICourierServiceTypeService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CourierServiceTypeService(AppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<CourierServiceType>> GetAllAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CourierServiceType>();

        return await _context.CourierServiceTypes
            .Where(s => s.TenantId == tenantId.Value)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<CourierServiceType?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.CourierServiceTypes
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId.Value);
    }

    public async Task<CourierServiceType?> GetByCodeAsync(string code)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.CourierServiceTypes
            .FirstOrDefaultAsync(s => s.Code == code && s.TenantId == tenantId.Value);
    }

    public async Task<CourierServiceType> CreateAsync(CourierServiceType serviceType)
    {
        _context.CourierServiceTypes.Add(serviceType);
        await _context.SaveChangesAsync();
        return serviceType;
    }

    public async Task<CourierServiceType> UpdateAsync(CourierServiceType serviceType)
    {
        serviceType.UpdatedAt = DateTime.UtcNow;
        _context.CourierServiceTypes.Update(serviceType);
        await _context.SaveChangesAsync();
        return serviceType;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var serviceType = await GetByIdAsync(id);
        if (serviceType == null)
            return false;

        _context.CourierServiceTypes.Remove(serviceType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string code, Guid? excludeId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return false;

        var query = _context.CourierServiceTypes
            .Where(s => s.Code == code && s.TenantId == tenantId.Value);

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
