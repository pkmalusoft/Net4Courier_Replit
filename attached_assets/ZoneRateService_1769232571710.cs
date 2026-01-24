using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface IZoneRateService
{
    Task<List<ZoneRate>> GetAllAsync();
    Task<List<ZoneRate>> GetByZoneAsync(Guid zoneId);
    Task<List<ZoneRate>> GetByServiceTypeAsync(Guid serviceTypeId);
    Task<ZoneRate?> GetByIdAsync(Guid id);
    Task<ZoneRate?> GetRateAsync(Guid zoneId, Guid serviceTypeId, decimal weight);
    Task<ZoneRate> CreateAsync(ZoneRate rate);
    Task<ZoneRate> UpdateAsync(ZoneRate rate);
    Task<bool> DeleteAsync(Guid id);
    Task<decimal> CalculateFreightAsync(Guid zoneId, Guid serviceTypeId, decimal weight, decimal declaredValue, bool isCOD);
}

public class ZoneRateService : IZoneRateService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ZoneRateService(AppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<ZoneRate>> GetAllAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<ZoneRate>();

        return await _context.ZoneRates
            .Include(r => r.CourierZone)
            .Include(r => r.CourierServiceType)
            .Where(r => r.TenantId == tenantId.Value)
            .OrderBy(r => r.CourierZone.ZoneName)
            .ThenBy(r => r.CourierServiceType.Name)
            .ThenBy(r => r.MinWeight)
            .ToListAsync();
    }

    public async Task<List<ZoneRate>> GetByZoneAsync(Guid zoneId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<ZoneRate>();

        return await _context.ZoneRates
            .Include(r => r.CourierServiceType)
            .Where(r => r.TenantId == tenantId.Value && r.CourierZoneId == zoneId)
            .OrderBy(r => r.CourierServiceType.Name)
            .ThenBy(r => r.MinWeight)
            .ToListAsync();
    }

    public async Task<List<ZoneRate>> GetByServiceTypeAsync(Guid serviceTypeId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<ZoneRate>();

        return await _context.ZoneRates
            .Include(r => r.CourierZone)
            .Where(r => r.TenantId == tenantId.Value && r.CourierServiceTypeId == serviceTypeId)
            .OrderBy(r => r.CourierZone.ZoneName)
            .ThenBy(r => r.MinWeight)
            .ToListAsync();
    }

    public async Task<ZoneRate?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.ZoneRates
            .Include(r => r.CourierZone)
            .Include(r => r.CourierServiceType)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId.Value);
    }

    public async Task<ZoneRate?> GetRateAsync(Guid zoneId, Guid serviceTypeId, decimal weight)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var now = DateTime.UtcNow;

        return await _context.ZoneRates
            .Where(r => r.TenantId == tenantId.Value &&
                       r.CourierZoneId == zoneId &&
                       r.CourierServiceTypeId == serviceTypeId &&
                       r.IsActive &&
                       r.EffectiveFrom <= now &&
                       (r.EffectiveTo == null || r.EffectiveTo >= now) &&
                       weight >= r.MinWeight &&
                       weight <= r.MaxWeight)
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    public async Task<ZoneRate> CreateAsync(ZoneRate rate)
    {
        _context.ZoneRates.Add(rate);
        await _context.SaveChangesAsync();
        return rate;
    }

    public async Task<ZoneRate> UpdateAsync(ZoneRate rate)
    {
        rate.UpdatedAt = DateTime.UtcNow;
        _context.ZoneRates.Update(rate);
        await _context.SaveChangesAsync();
        return rate;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rate = await GetByIdAsync(id);
        if (rate == null)
            return false;

        _context.ZoneRates.Remove(rate);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> CalculateFreightAsync(Guid zoneId, Guid serviceTypeId, decimal weight, decimal declaredValue, bool isCOD)
    {
        var rate = await GetRateAsync(zoneId, serviceTypeId, weight);
        if (rate == null)
            return 0;

        decimal freight = 0;

        switch (rate.RateType)
        {
            case RateType.FlatRate:
                freight = rate.BaseRate;
                break;

            case RateType.PerKg:
                if (weight <= rate.MinWeight)
                {
                    freight = rate.BaseRate;
                }
                else
                {
                    var additionalWeight = weight - rate.MinWeight;
                    freight = rate.BaseRate + (additionalWeight * rate.AdditionalRatePerKg);
                }
                break;

            case RateType.PerPiece:
                freight = rate.BaseRate;
                break;

            case RateType.Slab:
                freight = rate.BaseRate;
                break;
        }

        if (freight < rate.MinCharge)
            freight = rate.MinCharge;

        if (rate.FuelSurchargePercent > 0)
            freight += freight * (rate.FuelSurchargePercent / 100);

        if (isCOD && rate.CODChargePercent > 0)
        {
            var codCharge = declaredValue * (rate.CODChargePercent / 100);
            if (codCharge < rate.CODMinCharge)
                codCharge = rate.CODMinCharge;
            freight += codCharge;
        }

        return Math.Round(freight, 2);
    }
}
