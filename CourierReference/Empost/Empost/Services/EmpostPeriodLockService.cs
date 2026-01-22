using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Empost.Models;

namespace Server.Modules.Empost.Services;

public interface IEmpostPeriodLockService
{
    Task<bool> IsQuarterLockedAsync(Guid quarterId);
    Task<bool> IsDateInLockedPeriodAsync(DateTime date);
    Task<EmpostQuarter> LockQuarterAsync(Guid quarterId, Guid userId, string userName);
    Task<EmpostQuarter> UnlockQuarterAsync(Guid quarterId, Guid userId, string userName, string reason);
    Task ValidateShipmentDateNotLockedAsync(DateTime shipmentDate, string operation = "modify");
    Task<List<EmpostQuarter>> GetLockedQuartersAsync();
    Task<List<EmpostQuarter>> GetQuartersPendingSubmissionAsync();
    Task<EmpostQuarter> MarkQuarterAsSubmittedAsync(Guid quarterId, Guid userId, string userName);
}

public class EmpostPeriodLockService : IEmpostPeriodLockService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmpostQuarterService _quarterService;
    private readonly ILogger<EmpostPeriodLockService> _logger;

    public EmpostPeriodLockService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IEmpostQuarterService quarterService,
        ILogger<EmpostPeriodLockService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _quarterService = quarterService;
        _logger = logger;
    }

    public async Task<bool> IsQuarterLockedAsync(Guid quarterId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return false;

        var quarter = await _context.EmpostQuarters
            .FirstOrDefaultAsync(q => q.Id == quarterId && q.TenantId == tenantId.Value);

        return quarter?.IsLocked ?? false;
    }

    public async Task<bool> IsDateInLockedPeriodAsync(DateTime date)
    {
        var quarter = await _quarterService.GetQuarterForDateAsync(date);
        if (quarter == null)
            return false;

        return quarter.IsLocked;
    }

    public async Task<EmpostQuarter> LockQuarterAsync(Guid quarterId, Guid userId, string userName)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var quarter = await _context.EmpostQuarters
            .FirstOrDefaultAsync(q => q.Id == quarterId && q.TenantId == tenantId.Value);

        if (quarter == null)
            throw new ArgumentException("Quarter not found");

        if (quarter.IsLocked)
            throw new InvalidOperationException("Quarter is already locked");

        quarter.IsLocked = true;
        quarter.LockedDate = DateTime.UtcNow;
        quarter.LockedBy = userId;
        quarter.LockedByName = userName;
        quarter.Status = QuarterStatus.Locked;

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.QuarterLocked,
            ActionDescription = $"Quarter {quarter.QuarterName} {quarter.Year} locked",
            EntityType = nameof(EmpostQuarter),
            EntityId = quarterId,
            EmpostQuarterId = quarterId,
            Year = quarter.Year,
            Quarter = quarter.Quarter,
            PerformedBy = userId,
            PerformedByName = userName,
            PerformedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Quarter {Quarter} {Year} locked by {User}",
            quarter.QuarterName, quarter.Year, userName);

        return quarter;
    }

    public async Task<EmpostQuarter> UnlockQuarterAsync(Guid quarterId, Guid userId, string userName, string reason)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var quarter = await _context.EmpostQuarters
            .FirstOrDefaultAsync(q => q.Id == quarterId && q.TenantId == tenantId.Value);

        if (quarter == null)
            throw new ArgumentException("Quarter not found");

        if (!quarter.IsLocked)
            throw new InvalidOperationException("Quarter is not locked");

        var previousLockedDate = quarter.LockedDate;
        var previousLockedBy = quarter.LockedByName;

        quarter.IsLocked = false;
        quarter.LockedDate = null;
        quarter.LockedBy = null;
        quarter.LockedByName = null;
        quarter.Status = QuarterStatus.Open;

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.QuarterUnlocked,
            ActionDescription = $"Quarter {quarter.QuarterName} {quarter.Year} unlocked. Reason: {reason}",
            EntityType = nameof(EmpostQuarter),
            EntityId = quarterId,
            EmpostQuarterId = quarterId,
            Year = quarter.Year,
            Quarter = quarter.Quarter,
            OldData = $"Locked by {previousLockedBy} on {previousLockedDate}",
            PerformedBy = userId,
            PerformedByName = userName,
            PerformedAt = DateTime.UtcNow,
            Notes = reason
        });

        await _context.SaveChangesAsync();

        _logger.LogWarning("Quarter {Quarter} {Year} unlocked by {User}. Reason: {Reason}",
            quarter.QuarterName, quarter.Year, userName, reason);

        return quarter;
    }

    public async Task ValidateShipmentDateNotLockedAsync(DateTime shipmentDate, string operation = "modify")
    {
        var isLocked = await IsDateInLockedPeriodAsync(shipmentDate);
        if (isLocked)
        {
            var quarterInfo = _quarterService.GetQuarterInfo(shipmentDate);
            throw new InvalidOperationException(
                $"Cannot {operation} shipment. The period {quarterInfo.QuarterName} {quarterInfo.Year} is locked for Empost audit. " +
                $"Please contact your administrator to unlock the period if changes are required.");
        }
    }

    public async Task<List<EmpostQuarter>> GetLockedQuartersAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostQuarter>();

        return await _context.EmpostQuarters
            .Where(q => q.TenantId == tenantId.Value && q.IsLocked)
            .OrderByDescending(q => q.Year)
            .ThenByDescending(q => q.Quarter)
            .ToListAsync();
    }

    public async Task<List<EmpostQuarter>> GetQuartersPendingSubmissionAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostQuarter>();

        var now = DateTime.UtcNow;

        return await _context.EmpostQuarters
            .Where(q => q.TenantId == tenantId.Value
                && q.Status != QuarterStatus.Submitted
                && q.PeriodEnd < now)
            .OrderBy(q => q.Year)
            .ThenBy(q => q.Quarter)
            .ToListAsync();
    }

    public async Task<EmpostQuarter> MarkQuarterAsSubmittedAsync(Guid quarterId, Guid userId, string userName)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var quarter = await _context.EmpostQuarters
            .FirstOrDefaultAsync(q => q.Id == quarterId && q.TenantId == tenantId.Value);

        if (quarter == null)
            throw new ArgumentException("Quarter not found");

        quarter.Status = QuarterStatus.Submitted;
        quarter.SubmittedDate = DateTime.UtcNow;
        quarter.SubmittedBy = userId;
        quarter.SubmittedByName = userName;
        quarter.IsLocked = true;
        quarter.LockedDate = DateTime.UtcNow;
        quarter.LockedBy = userId;
        quarter.LockedByName = userName;

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.QuarterSubmitted,
            ActionDescription = $"Quarter {quarter.QuarterName} {quarter.Year} marked as submitted",
            EntityType = nameof(EmpostQuarter),
            EntityId = quarterId,
            EmpostQuarterId = quarterId,
            Year = quarter.Year,
            Quarter = quarter.Quarter,
            PerformedBy = userId,
            PerformedByName = userName,
            PerformedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Quarter {Quarter} {Year} marked as submitted by {User}",
            quarter.QuarterName, quarter.Year, userName);

        return quarter;
    }
}
