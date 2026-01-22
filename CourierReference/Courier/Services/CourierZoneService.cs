using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface ICourierZoneService
{
    Task<List<CourierZone>> GetAllAsync();
    Task<CourierZone?> GetByIdAsync(Guid id);
    Task<CourierZone?> GetByCodeAsync(string zoneCode);
    Task<CourierZone> CreateAsync(CourierZone zone);
    Task<CourierZone> UpdateAsync(CourierZone zone);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string zoneCode, Guid? excludeId = null);
    Task<bool> ExistsAsync(Guid zoneCategoryId, string zoneCode, Guid? excludeId = null);
    Task<CourierZone?> FindZoneByPostalCodeAsync(string postalCode, string? country = null);
}

public class CourierZoneService : ICourierZoneService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CourierZoneService(AppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<CourierZone>> GetAllAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<CourierZone>();

        return await _context.CourierZones
            .Include(z => z.ZoneCategory)
            .Include(z => z.ZoneCountries)
            .Include(z => z.ZoneStates)
            .Where(z => z.TenantId == tenantId.Value)
            .OrderBy(z => z.ZoneCategory!.Name)
            .ThenBy(z => z.ZoneName)
            .ToListAsync();
    }

    public async Task<CourierZone?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.CourierZones
            .Include(z => z.ZoneCategory)
            .Include(z => z.ZoneCountries)
            .Include(z => z.ZoneStates)
            .FirstOrDefaultAsync(z => z.Id == id && z.TenantId == tenantId.Value);
    }

    public async Task<CourierZone?> GetByCodeAsync(string zoneCode)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.CourierZones
            .FirstOrDefaultAsync(z => z.ZoneCode == zoneCode && z.TenantId == tenantId.Value);
    }

    public async Task<CourierZone> CreateAsync(CourierZone zone)
    {
        _context.CourierZones.Add(zone);
        await _context.SaveChangesAsync();
        return zone;
    }

    public async Task<CourierZone> UpdateAsync(CourierZone zone)
    {
        zone.UpdatedAt = DateTime.UtcNow;
        _context.CourierZones.Update(zone);
        await _context.SaveChangesAsync();
        return zone;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var zone = await GetByIdAsync(id);
        if (zone == null)
            return false;

        _context.CourierZones.Remove(zone);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string zoneCode, Guid? excludeId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return false;

        var query = _context.CourierZones
            .Where(z => z.ZoneCode == zoneCode && z.TenantId == tenantId.Value);

        if (excludeId.HasValue)
            query = query.Where(z => z.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsAsync(Guid zoneCategoryId, string zoneCode, Guid? excludeId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return false;

        var query = _context.CourierZones
            .Where(z => z.ZoneCategoryId == zoneCategoryId && z.ZoneCode == zoneCode && z.TenantId == tenantId.Value);

        if (excludeId.HasValue)
            query = query.Where(z => z.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<CourierZone?> FindZoneByPostalCodeAsync(string postalCode, string? country = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var query = _context.CourierZones
            .Where(z => z.TenantId == tenantId.Value && z.IsActive);

        if (!string.IsNullOrEmpty(country))
            query = query.Where(z => z.Country == country);

        var zones = await query.ToListAsync();

        foreach (var zone in zones)
        {
            if (!string.IsNullOrEmpty(zone.PostalCodeFrom) && !string.IsNullOrEmpty(zone.PostalCodeTo))
            {
                if (string.Compare(postalCode, zone.PostalCodeFrom, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    string.Compare(postalCode, zone.PostalCodeTo, StringComparison.OrdinalIgnoreCase) <= 0)
                {
                    return zone;
                }
            }
            else if (!string.IsNullOrEmpty(zone.City) && zone.City.Equals(postalCode, StringComparison.OrdinalIgnoreCase))
            {
                return zone;
            }
        }

        return zones.FirstOrDefault(z => z.ZoneCode == "DEFAULT" || z.ZoneName.Contains("Default", StringComparison.OrdinalIgnoreCase));
    }
}
