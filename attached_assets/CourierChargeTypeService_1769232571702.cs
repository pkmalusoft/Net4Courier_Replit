using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public class CourierChargeTypeService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CourierChargeTypeService(AppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    private Guid GetTenantId()
    {
        return _tenantProvider.CurrentTenantId ?? throw new UnauthorizedAccessException("No tenant context");
    }

    public async Task<List<CourierChargeType>> GetAllAsync()
    {
        var tenantId = GetTenantId();
        return await _context.CourierChargeTypes
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<CourierChargeType>> GetActiveAsync()
    {
        var tenantId = GetTenantId();
        return await _context.CourierChargeTypes
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<CourierChargeType?> GetByIdAsync(Guid id)
    {
        var tenantId = GetTenantId();
        return await _context.CourierChargeTypes
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
    }

    public async Task<CourierChargeType?> GetByCodeAsync(string code)
    {
        var tenantId = GetTenantId();
        return await _context.CourierChargeTypes
            .FirstOrDefaultAsync(c => c.Code == code && c.TenantId == tenantId);
    }

    public async Task<CourierChargeType> CreateAsync(CourierChargeType chargeType)
    {
        var tenantId = GetTenantId();
        
        var existing = await _context.CourierChargeTypes
            .AnyAsync(c => c.Code == chargeType.Code && c.TenantId == tenantId);
        if (existing)
        {
            throw new InvalidOperationException($"Charge type with code '{chargeType.Code}' already exists");
        }

        chargeType.Id = Guid.NewGuid();
        chargeType.TenantId = tenantId;
        chargeType.CreatedAt = DateTime.UtcNow;

        _context.CourierChargeTypes.Add(chargeType);
        await _context.SaveChangesAsync();
        return chargeType;
    }

    public async Task<CourierChargeType> UpdateAsync(CourierChargeType chargeType)
    {
        var tenantId = GetTenantId();
        var existing = await _context.CourierChargeTypes
            .FirstOrDefaultAsync(c => c.Id == chargeType.Id && c.TenantId == tenantId);
        
        if (existing == null)
        {
            throw new KeyNotFoundException($"Charge type with ID {chargeType.Id} not found");
        }

        var duplicateCode = await _context.CourierChargeTypes
            .AnyAsync(c => c.Code == chargeType.Code && c.TenantId == tenantId && c.Id != chargeType.Id);
        if (duplicateCode)
        {
            throw new InvalidOperationException($"Another charge type with code '{chargeType.Code}' already exists");
        }

        existing.Code = chargeType.Code;
        existing.Name = chargeType.Name;
        existing.Description = chargeType.Description;
        existing.DefaultAmount = chargeType.DefaultAmount;
        existing.IsActive = chargeType.IsActive;
        existing.SortOrder = chargeType.SortOrder;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tenantId = GetTenantId();
        var chargeType = await _context.CourierChargeTypes
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        
        if (chargeType == null)
        {
            return false;
        }

        if (chargeType.IsSystemDefault)
        {
            throw new InvalidOperationException("Cannot delete system default charge type");
        }

        var hasShipmentCharges = await _context.ShipmentCharges
            .AnyAsync(sc => sc.ChargeTypeId == id && sc.TenantId == tenantId);
        if (hasShipmentCharges)
        {
            throw new InvalidOperationException("Cannot delete charge type that is in use by shipments");
        }

        _context.CourierChargeTypes.Remove(chargeType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task EnsureDefaultChargeTypesAsync()
    {
        var tenantId = GetTenantId();
        var hasAny = await _context.CourierChargeTypes.AnyAsync(c => c.TenantId == tenantId);
        if (hasAny) return;

        var defaults = new List<CourierChargeType>
        {
            new CourierChargeType
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "HANDLING",
                Name = "Handling Charges",
                Description = "Charges for handling and processing the shipment",
                DefaultAmount = 0,
                IsActive = true,
                SortOrder = 1,
                IsSystemDefault = true,
                CreatedAt = DateTime.UtcNow
            },
            new CourierChargeType
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "PACKING",
                Name = "Packing Charges",
                Description = "Charges for special packing materials and services",
                DefaultAmount = 0,
                IsActive = true,
                SortOrder = 2,
                IsSystemDefault = true,
                CreatedAt = DateTime.UtcNow
            },
            new CourierChargeType
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "SPECIAL_DELIVERY",
                Name = "Special Delivery",
                Description = "Additional charges for special delivery requirements",
                DefaultAmount = 0,
                IsActive = true,
                SortOrder = 3,
                IsSystemDefault = false,
                CreatedAt = DateTime.UtcNow
            },
            new CourierChargeType
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "WEEKEND",
                Name = "Weekend Delivery",
                Description = "Surcharge for weekend delivery",
                DefaultAmount = 0,
                IsActive = true,
                SortOrder = 4,
                IsSystemDefault = false,
                CreatedAt = DateTime.UtcNow
            },
            new CourierChargeType
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "MISC",
                Name = "Miscellaneous Charges",
                Description = "Other miscellaneous charges",
                DefaultAmount = 0,
                IsActive = true,
                SortOrder = 99,
                IsSystemDefault = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.CourierChargeTypes.AddRangeAsync(defaults);
        await _context.SaveChangesAsync();
    }
}
