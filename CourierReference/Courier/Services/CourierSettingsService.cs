using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface ICourierSettingsService
{
    Task<CourierSettings?> GetAsync();
    Task<CourierSettingsDto?> GetWithAccountNamesAsync();
    Task<CourierSettings> UpsertAsync(CourierSettingsUpdateDto dto);
}

public class CourierSettingsDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? PrepaidControlAccountId { get; set; }
    public string? PrepaidControlAccountName { get; set; }
    public string? PrepaidControlAccountCode { get; set; }
    public Guid? CODControlAccountId { get; set; }
    public string? CODControlAccountName { get; set; }
    public string? CODControlAccountCode { get; set; }
    public Guid? FreightRevenueAccountId { get; set; }
    public string? FreightRevenueAccountName { get; set; }
    public string? FreightRevenueAccountCode { get; set; }
    public Guid? CODPayableAccountId { get; set; }
    public string? CODPayableAccountName { get; set; }
    public string? CODPayableAccountCode { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CourierSettingsUpdateDto
{
    public Guid? PrepaidControlAccountId { get; set; }
    public Guid? CODControlAccountId { get; set; }
    public Guid? FreightRevenueAccountId { get; set; }
    public Guid? CODPayableAccountId { get; set; }
    public string? Notes { get; set; }
}

public class CourierSettingsService : ICourierSettingsService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CourierSettingsService(AppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<CourierSettings?> GetAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.CourierSettings
            .Include(s => s.PrepaidControlAccount)
            .Include(s => s.CODControlAccount)
            .Include(s => s.FreightRevenueAccount)
            .Include(s => s.CODPayableAccount)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId.Value);
    }

    public async Task<CourierSettingsDto?> GetWithAccountNamesAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var settings = await _context.CourierSettings
            .Include(s => s.PrepaidControlAccount)
            .Include(s => s.CODControlAccount)
            .Include(s => s.FreightRevenueAccount)
            .Include(s => s.CODPayableAccount)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId.Value);

        if (settings == null)
            return null;

        return new CourierSettingsDto
        {
            Id = settings.Id,
            TenantId = settings.TenantId,
            PrepaidControlAccountId = settings.PrepaidControlAccountId,
            PrepaidControlAccountName = settings.PrepaidControlAccount?.AccountName,
            PrepaidControlAccountCode = settings.PrepaidControlAccount?.AccountCode,
            CODControlAccountId = settings.CODControlAccountId,
            CODControlAccountName = settings.CODControlAccount?.AccountName,
            CODControlAccountCode = settings.CODControlAccount?.AccountCode,
            FreightRevenueAccountId = settings.FreightRevenueAccountId,
            FreightRevenueAccountName = settings.FreightRevenueAccount?.AccountName,
            FreightRevenueAccountCode = settings.FreightRevenueAccount?.AccountCode,
            CODPayableAccountId = settings.CODPayableAccountId,
            CODPayableAccountName = settings.CODPayableAccount?.AccountName,
            CODPayableAccountCode = settings.CODPayableAccount?.AccountCode,
            Notes = settings.Notes,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
    }

    public async Task<CourierSettings> UpsertAsync(CourierSettingsUpdateDto dto)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("No tenant context available");

        var existing = await _context.CourierSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId.Value);

        if (existing == null)
        {
            existing = new CourierSettings
            {
                TenantId = tenantId.Value,
                PrepaidControlAccountId = dto.PrepaidControlAccountId,
                CODControlAccountId = dto.CODControlAccountId,
                FreightRevenueAccountId = dto.FreightRevenueAccountId,
                CODPayableAccountId = dto.CODPayableAccountId,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };
            _context.CourierSettings.Add(existing);
        }
        else
        {
            existing.PrepaidControlAccountId = dto.PrepaidControlAccountId;
            existing.CODControlAccountId = dto.CODControlAccountId;
            existing.FreightRevenueAccountId = dto.FreightRevenueAccountId;
            existing.CODPayableAccountId = dto.CODPayableAccountId;
            existing.Notes = dto.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await _context.Entry(existing).Reference(s => s.PrepaidControlAccount).LoadAsync();
        await _context.Entry(existing).Reference(s => s.CODControlAccount).LoadAsync();
        await _context.Entry(existing).Reference(s => s.FreightRevenueAccount).LoadAsync();
        await _context.Entry(existing).Reference(s => s.CODPayableAccount).LoadAsync();

        return existing;
    }
}
