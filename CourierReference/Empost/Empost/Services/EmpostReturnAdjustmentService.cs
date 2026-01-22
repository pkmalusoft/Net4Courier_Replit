using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using Server.Modules.Empost.Models;

namespace Server.Modules.Empost.Services;

public interface IEmpostReturnAdjustmentService
{
    Task<EmpostReturnAdjustment?> CreateReturnAdjustmentAsync(Guid shipmentId, string reason, Guid userId, string userName);
    Task<EmpostReturnAdjustment> ApplyAdjustmentAsync(Guid adjustmentId, Guid userId, string userName);
    Task<EmpostReturnAdjustment> RejectAdjustmentAsync(Guid adjustmentId, string reason, Guid userId, string userName);
    Task<List<EmpostReturnAdjustment>> GetPendingAdjustmentsAsync();
    Task<List<EmpostReturnAdjustment>> GetAllAdjustmentsAsync();
    Task<List<EmpostReturnAdjustment>> GetAdjustmentsForQuarterAsync(Guid quarterId);
    Task ProcessShipmentReturnAsync(Shipment shipment, Guid userId, string userName);
    Task<int> ProcessPendingAdjustmentsAsync(Guid userId, string userName);
}

public class EmpostReturnAdjustmentService : IEmpostReturnAdjustmentService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmpostPeriodLockService _periodLockService;
    private readonly ILogger<EmpostReturnAdjustmentService> _logger;

    public EmpostReturnAdjustmentService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IEmpostPeriodLockService periodLockService,
        ILogger<EmpostReturnAdjustmentService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _periodLockService = periodLockService;
        _logger = logger;
    }

    public async Task<EmpostReturnAdjustment?> CreateReturnAdjustmentAsync(
        Guid shipmentId, 
        string reason, 
        Guid userId, 
        string userName)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var shipmentFee = await _context.EmpostShipmentFees
            .Include(f => f.Shipment)
            .Include(f => f.EmpostQuarter)
            .FirstOrDefaultAsync(f => f.ShipmentId == shipmentId && f.TenantId == tenantId.Value);

        if (shipmentFee == null)
        {
            _logger.LogWarning("No Empost fee record found for shipment {ShipmentId}", shipmentId);
            return null;
        }

        if (shipmentFee.Classification != EmpostClassification.Taxable)
        {
            _logger.LogInformation("Shipment {AWB} was exempt, no adjustment needed", shipmentFee.AWBNumber);
            return null;
        }

        if (shipmentFee.IsReturnAdjusted)
        {
            _logger.LogWarning("Shipment {AWB} already has a return adjustment", shipmentFee.AWBNumber);
            return null;
        }

        if (await _periodLockService.IsQuarterLockedAsync(shipmentFee.EmpostQuarterId))
        {
            throw new InvalidOperationException(
                $"Cannot create return adjustment. Quarter {shipmentFee.EmpostQuarter.QuarterName} {shipmentFee.EmpostQuarter.Year} is locked.");
        }

        var adjustment = new EmpostReturnAdjustment
        {
            EmpostShipmentFeeId = shipmentFee.Id,
            EmpostQuarterId = shipmentFee.EmpostQuarterId,
            ShipmentId = shipmentId,
            AWBNumber = shipmentFee.AWBNumber,
            OriginalShipmentDate = shipmentFee.ShipmentDate,
            ReturnDate = DateTime.UtcNow,
            AdjustmentType = EmpostAdjustmentType.FullRefund,
            OriginalGrossAmount = shipmentFee.GrossAmount,
            OriginalFeeAmount = shipmentFee.EmpostFeeAmount,
            AdjustmentAmount = shipmentFee.EmpostFeeAmount,
            Status = AdjustmentStatus.Applied,
            Reason = reason,
            AppliedDate = DateTime.UtcNow,
            AppliedBy = userId,
            AppliedByName = userName
        };

        _context.EmpostReturnAdjustments.Add(adjustment);

        shipmentFee.IsReturnAdjusted = true;
        shipmentFee.AdjustedDate = DateTime.UtcNow;
        shipmentFee.AdjustmentReason = reason;
        shipmentFee.FeeStatus = EmpostFeeStatus.Credited;

        var shipment = shipmentFee.Shipment;
        shipment.EmpostFeeStatus = EmpostFeeStatus.Credited;

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.ReturnAdjustmentCreated,
            ActionDescription = $"Return adjustment created for {shipmentFee.AWBNumber}: AED {shipmentFee.EmpostFeeAmount:N2} credited",
            EntityType = nameof(EmpostReturnAdjustment),
            EmpostQuarterId = shipmentFee.EmpostQuarterId,
            AWBNumber = shipmentFee.AWBNumber,
            OldValue = shipmentFee.EmpostFeeAmount,
            NewValue = 0,
            PerformedBy = userId,
            PerformedByName = userName,
            PerformedAt = DateTime.UtcNow,
            Notes = reason
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created return adjustment for shipment {AWB}: AED {Amount} credited",
            shipmentFee.AWBNumber, shipmentFee.EmpostFeeAmount);

        return adjustment;
    }

    public async Task<EmpostReturnAdjustment> ApplyAdjustmentAsync(Guid adjustmentId, Guid userId, string userName)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var adjustment = await _context.EmpostReturnAdjustments
            .Include(a => a.EmpostShipmentFee)
            .Include(a => a.EmpostQuarter)
            .FirstOrDefaultAsync(a => a.Id == adjustmentId && a.TenantId == tenantId.Value);

        if (adjustment == null)
            throw new ArgumentException("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.Pending)
            throw new InvalidOperationException("Adjustment is not pending");

        if (await _periodLockService.IsQuarterLockedAsync(adjustment.EmpostQuarterId))
        {
            throw new InvalidOperationException(
                $"Cannot apply adjustment. Quarter {adjustment.EmpostQuarter.QuarterName} {adjustment.EmpostQuarter.Year} is locked.");
        }

        adjustment.Status = AdjustmentStatus.Applied;
        adjustment.AppliedDate = DateTime.UtcNow;
        adjustment.AppliedBy = userId;
        adjustment.AppliedByName = userName;

        adjustment.EmpostShipmentFee.IsReturnAdjusted = true;
        adjustment.EmpostShipmentFee.AdjustedDate = DateTime.UtcNow;
        adjustment.EmpostShipmentFee.FeeStatus = EmpostFeeStatus.Credited;

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.ReturnAdjustmentApplied,
            ActionDescription = $"Return adjustment applied for {adjustment.AWBNumber}",
            EntityType = nameof(EmpostReturnAdjustment),
            EntityId = adjustmentId,
            EmpostQuarterId = adjustment.EmpostQuarterId,
            AWBNumber = adjustment.AWBNumber,
            PerformedBy = userId,
            PerformedByName = userName,
            PerformedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return adjustment;
    }

    public async Task<EmpostReturnAdjustment> RejectAdjustmentAsync(
        Guid adjustmentId, 
        string reason, 
        Guid userId, 
        string userName)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var adjustment = await _context.EmpostReturnAdjustments
            .FirstOrDefaultAsync(a => a.Id == adjustmentId && a.TenantId == tenantId.Value);

        if (adjustment == null)
            throw new ArgumentException("Adjustment not found");

        if (adjustment.Status != AdjustmentStatus.Pending)
            throw new InvalidOperationException("Adjustment is not pending");

        adjustment.Status = AdjustmentStatus.Rejected;
        adjustment.Notes = $"Rejected: {reason}";

        await _context.EmpostAuditLogs.AddAsync(new EmpostAuditLog
        {
            Action = EmpostAuditAction.ReturnAdjustmentApplied,
            ActionDescription = $"Return adjustment rejected for {adjustment.AWBNumber}: {reason}",
            EntityType = nameof(EmpostReturnAdjustment),
            EntityId = adjustmentId,
            EmpostQuarterId = adjustment.EmpostQuarterId,
            AWBNumber = adjustment.AWBNumber,
            PerformedBy = userId,
            PerformedByName = userName,
            PerformedAt = DateTime.UtcNow,
            Notes = reason
        });

        await _context.SaveChangesAsync();

        return adjustment;
    }

    public async Task<List<EmpostReturnAdjustment>> GetPendingAdjustmentsAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostReturnAdjustment>();

        return await _context.EmpostReturnAdjustments
            .Include(a => a.EmpostQuarter)
            .Where(a => a.TenantId == tenantId.Value && a.Status == AdjustmentStatus.Pending)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EmpostReturnAdjustment>> GetAdjustmentsForQuarterAsync(Guid quarterId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostReturnAdjustment>();

        return await _context.EmpostReturnAdjustments
            .Include(a => a.EmpostShipmentFee)
            .Where(a => a.EmpostQuarterId == quarterId && a.TenantId == tenantId.Value)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task ProcessShipmentReturnAsync(Shipment shipment, Guid userId, string userName)
    {
        if (shipment.Status != ShipmentStatus.ReturnedToOrigin)
        {
            _logger.LogDebug("Shipment {AWB} is not RTO, skipping return adjustment", shipment.AWBNumber);
            return;
        }

        var adjustment = await CreateReturnAdjustmentAsync(
            shipment.Id, 
            "Shipment returned to origin (RTO)", 
            userId, 
            userName);

        if (adjustment != null)
        {
            _logger.LogInformation("Processed RTO for shipment {AWB}, adjustment created", shipment.AWBNumber);
        }
    }

    public async Task<List<EmpostReturnAdjustment>> GetAllAdjustmentsAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<EmpostReturnAdjustment>();

        return await _context.EmpostReturnAdjustments
            .Include(a => a.EmpostQuarter)
            .Where(a => a.TenantId == tenantId.Value)
            .OrderByDescending(a => a.ReturnDate)
            .ToListAsync();
    }

    public async Task<int> ProcessPendingAdjustmentsAsync(Guid userId, string userName)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var pendingAdjustments = await _context.EmpostReturnAdjustments
            .Include(a => a.EmpostShipmentFee)
            .Where(a => a.TenantId == tenantId.Value && a.Status == AdjustmentStatus.Pending)
            .ToListAsync();

        var processedCount = 0;
        foreach (var adjustment in pendingAdjustments)
        {
            try
            {
                await ApplyAdjustmentAsync(adjustment.Id, userId, userName);
                processedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process adjustment {Id}", adjustment.Id);
            }
        }

        return processedCount;
    }
}
