using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface IShipmentService
{
    Task<List<Shipment>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null, ShipmentStatus? status = null);
    Task<Shipment?> GetByIdAsync(Guid id);
    Task<Shipment?> GetByAWBAsync(string awbNumber);
    Task<Shipment?> GetByAWBForTrackingAsync(string awbNumber);
    Task<Shipment> CreateAsync(Shipment shipment);
    Task<Shipment> UpdateAsync(Shipment shipment);
    Task<bool> UpdateStatusAsync(Guid id, ShipmentStatus newStatus, string? remarks = null, Guid? agentId = null);
    Task<bool> VoidAsync(Guid id, string reason, Guid userId);
    Task<string> GenerateAWBNumberAsync();
    Task<List<Shipment>> GetPendingDeliveriesAsync();
    Task<List<Shipment>> GetByCustomerAsync(Guid customerId);
    Task<List<Shipment>> GetByAgentAsync(Guid agentId);
    Task<List<Shipment>> GetByStatusAsync(ShipmentStatus status);
    Task<List<ShipmentTracking>> GetTrackingHistoryAsync(Guid shipmentId);
    Task<ShipmentTracking> AddTrackingEventAsync(Guid shipmentId, ShipmentStatus status, string description, string? location = null, Guid? agentId = null);
    Task<decimal> CalculateChargesAsync(Guid serviceTypeId, Guid originZoneId, Guid destinationZoneId, decimal weight, decimal declaredValue, PaymentMode paymentMode);
    Task<ShipmentChargeAudit> LogFreightChargeChangeAsync(Guid shipmentId, string awbNumber, string fieldName, decimal oldValue, decimal newValue, string? changeReason, Guid userId, string? userName, string? userEmail, string? ipAddress);
    Task<List<ShipmentChargeAudit>> GetChargeAuditHistoryAsync(Guid shipmentId);
}

public class ShipmentService : IShipmentService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IZoneRateService _zoneRateService;
    private readonly IVoucherNumberingService _voucherNumberingService;

    public ShipmentService(
        AppDbContext context, 
        ITenantProvider tenantProvider,
        IZoneRateService zoneRateService,
        IVoucherNumberingService voucherNumberingService)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _zoneRateService = zoneRateService;
        _voucherNumberingService = voucherNumberingService;
    }

    public async Task<List<Shipment>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null, ShipmentStatus? status = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        var query = _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.OriginZone)
            .Include(s => s.DestinationZone)
            .Include(s => s.Customer)
            .Include(s => s.AssignedAgent)
            .Where(s => s.TenantId == tenantId.Value && !s.IsVoided);

        if (fromDate.HasValue)
        {
            var utcFromDate = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
            query = query.Where(s => s.BookingDate >= utcFromDate);
        }

        if (toDate.HasValue)
        {
            var utcToDate = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
            query = query.Where(s => s.BookingDate <= utcToDate);
        }

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        return await query
            .OrderByDescending(s => s.BookingDate)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Shipment?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.OriginZone)
            .Include(s => s.DestinationZone)
            .Include(s => s.Customer)
            .Include(s => s.AssignedAgent)
            .Include(s => s.Items)
            .Include(s => s.Charges)
            .Include(s => s.TrackingHistory.OrderByDescending(t => t.EventDateTime))
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId.Value);
    }

    public async Task<Shipment?> GetByAWBAsync(string awbNumber)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var searchAwb = awbNumber.Trim().ToUpperInvariant();
        return await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.OriginZone)
            .Include(s => s.DestinationZone)
            .Include(s => s.Customer)
            .Include(s => s.AssignedAgent)
            .Include(s => s.Items)
            .Include(s => s.Charges)
            .Include(s => s.TrackingHistory.OrderByDescending(t => t.EventDateTime))
            .FirstOrDefaultAsync(s => s.AWBNumber.ToUpper() == searchAwb && s.TenantId == tenantId.Value);
    }

    public async Task<Shipment?> GetByAWBForTrackingAsync(string awbNumber)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var searchAwb = awbNumber.Trim().ToUpperInvariant();
        return await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.TrackingHistory.OrderByDescending(t => t.EventDateTime))
            .Include(s => s.PickupRequest)
            .FirstOrDefaultAsync(s => s.AWBNumber.ToUpper() == searchAwb && s.TenantId == tenantId.Value);
    }

    public async Task<Shipment> CreateAsync(Shipment shipment)
    {
        if (string.IsNullOrEmpty(shipment.AWBNumber))
        {
            shipment.AWBNumber = await GenerateAWBNumberAsync();
        }

        shipment.ChargeableWeight = Math.Max(shipment.ActualWeight, shipment.VolumetricWeight);
        
        // Auto-calculate ShipmentMode based on origin/destination vs company country
        await CalculateShipmentModeAsync(shipment);
        
        // Compute OtherCharges from Charges collection
        RecalculateOtherCharges(shipment);

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        await AddTrackingEventAsync(
            shipment.Id, 
            ShipmentStatus.Booked, 
            "Shipment booked",
            shipment.SenderCity);

        return shipment;
    }

    public async Task<Shipment> UpdateAsync(Shipment shipment)
    {
        shipment.UpdatedAt = DateTime.UtcNow;
        shipment.ChargeableWeight = Math.Max(shipment.ActualWeight, shipment.VolumetricWeight);
        
        // Recalculate ShipmentMode in case countries changed
        await CalculateShipmentModeAsync(shipment);
        
        // Compute OtherCharges from Charges collection
        RecalculateOtherCharges(shipment);
        
        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();
        return shipment;
    }
    
    private void RecalculateOtherCharges(Shipment shipment)
    {
        if (shipment.Charges != null && shipment.Charges.Any())
        {
            shipment.OtherCharges = shipment.Charges.Sum(c => c.Amount);
        }
        else
        {
            shipment.OtherCharges = 0;
        }
        
        // Recalculate TotalCharge to include updated OtherCharges
        shipment.TotalCharge = shipment.FreightCharge + shipment.InsuranceCharge + shipment.OtherCharges;
    }
    
    private async Task CalculateShipmentModeAsync(Shipment shipment)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue) return;
        
        // Get company country from tenant
        var tenant = await _context.Tenants.FindAsync(tenantId.Value);
        var companyCountry = tenant?.Country?.Trim().ToUpperInvariant() ?? string.Empty;
        
        var originCountry = shipment.SenderCountry?.Trim().ToUpperInvariant() ?? string.Empty;
        var destinationCountry = shipment.ReceiverCountry?.Trim().ToUpperInvariant() ?? string.Empty;
        
        bool originIsCompany = string.Equals(originCountry, companyCountry, StringComparison.OrdinalIgnoreCase) 
                               || string.IsNullOrEmpty(originCountry);
        bool destinationIsCompany = string.Equals(destinationCountry, companyCountry, StringComparison.OrdinalIgnoreCase) 
                                    || string.IsNullOrEmpty(destinationCountry);
        
        // Determine ShipmentMode:
        // - Domestic: Origin = Company AND Destination = Company
        // - Export: Origin = Company AND Destination <> Company
        // - Import: Origin <> Company AND Destination = Company
        // - Transhipment: Origin <> Company AND Destination <> Company
        if (originIsCompany && destinationIsCompany)
        {
            shipment.ShipmentMode = ShipmentMode.Domestic;
        }
        else if (originIsCompany && !destinationIsCompany)
        {
            shipment.ShipmentMode = ShipmentMode.Export;
        }
        else if (!originIsCompany && destinationIsCompany)
        {
            shipment.ShipmentMode = ShipmentMode.Import;
        }
        else
        {
            shipment.ShipmentMode = ShipmentMode.Transhipment;
        }
    }

    public async Task<bool> UpdateStatusAsync(Guid id, ShipmentStatus newStatus, string? remarks = null, Guid? agentId = null)
    {
        var shipment = await GetByIdAsync(id);
        if (shipment == null)
            return false;

        var oldStatus = shipment.Status;
        shipment.Status = newStatus;
        shipment.UpdatedAt = DateTime.UtcNow;

        if (newStatus == ShipmentStatus.Delivered)
        {
            shipment.ActualDeliveryDate = DateTime.UtcNow;
        }

        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();

        var description = GetStatusDescription(newStatus);
        if (!string.IsNullOrEmpty(remarks))
            description += $" - {remarks}";

        await AddTrackingEventAsync(id, newStatus, description, null, agentId);

        return true;
    }

    public async Task<bool> VoidAsync(Guid id, string reason, Guid userId)
    {
        var shipment = await GetByIdAsync(id);
        if (shipment == null)
            return false;

        shipment.IsVoided = true;
        shipment.VoidedBy = userId;
        shipment.VoidedDate = DateTime.UtcNow;
        shipment.VoidReason = reason;
        shipment.Status = ShipmentStatus.Cancelled;
        shipment.UpdatedAt = DateTime.UtcNow;

        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();

        await AddTrackingEventAsync(id, ShipmentStatus.Cancelled, $"Shipment voided: {reason}");

        return true;
    }

    public async Task<string> GenerateAWBNumberAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        return await _voucherNumberingService.GenerateNumberAsync(Shared.Enums.VoucherTransactionType.AWB, tenantId.Value);
    }

    public async Task<List<Shipment>> GetPendingDeliveriesAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        return await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.DestinationZone)
            .Include(s => s.Customer)
            .Include(s => s.AssignedAgent)
            .Where(s => s.TenantId == tenantId.Value && 
                       !s.IsVoided &&
                       (s.Status == ShipmentStatus.InTransit || 
                        s.Status == ShipmentStatus.OutForDelivery))
            .OrderBy(s => s.ExpectedDeliveryDate)
            .ToListAsync();
    }

    public async Task<List<Shipment>> GetByCustomerAsync(Guid customerId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        return await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.DestinationZone)
            .Where(s => s.TenantId == tenantId.Value && 
                       s.CustomerId == customerId && 
                       !s.IsVoided)
            .OrderByDescending(s => s.BookingDate)
            .ToListAsync();
    }

    public async Task<List<Shipment>> GetByAgentAsync(Guid agentId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        return await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.DestinationZone)
            .Include(s => s.Customer)
            .Where(s => s.TenantId == tenantId.Value && 
                       s.AssignedAgentId == agentId && 
                       !s.IsVoided)
            .OrderByDescending(s => s.BookingDate)
            .ToListAsync();
    }

    public async Task<List<Shipment>> GetByStatusAsync(ShipmentStatus status)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        return await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.DestinationZone)
            .Include(s => s.Customer)
            .Include(s => s.AssignedAgent)
            .Where(s => s.TenantId == tenantId.Value && 
                       s.Status == status && 
                       !s.IsVoided)
            .OrderByDescending(s => s.BookingDate)
            .ToListAsync();
    }

    public async Task<List<ShipmentTracking>> GetTrackingHistoryAsync(Guid shipmentId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<ShipmentTracking>();

        return await _context.ShipmentTrackings
            .Include(t => t.Agent)
            .Where(t => t.TenantId == tenantId.Value && t.ShipmentId == shipmentId)
            .OrderByDescending(t => t.EventDateTime)
            .ToListAsync();
    }

    public async Task<ShipmentTracking> AddTrackingEventAsync(Guid shipmentId, ShipmentStatus status, string description, string? location = null, Guid? agentId = null)
    {
        var tracking = new ShipmentTracking
        {
            ShipmentId = shipmentId,
            Status = status,
            StatusDescription = description,
            Location = location,
            EventDateTime = DateTime.UtcNow,
            AgentId = agentId,
            IsPublic = true
        };

        _context.ShipmentTrackings.Add(tracking);
        await _context.SaveChangesAsync();

        return tracking;
    }

    public async Task<decimal> CalculateChargesAsync(Guid serviceTypeId, Guid originZoneId, Guid destinationZoneId, decimal weight, decimal declaredValue, PaymentMode paymentMode)
    {
        var isCOD = paymentMode == PaymentMode.COD;
        return await _zoneRateService.CalculateFreightAsync(destinationZoneId, serviceTypeId, weight, declaredValue, isCOD);
    }

    private static string GetStatusDescription(ShipmentStatus status)
    {
        return status switch
        {
            ShipmentStatus.Draft => "Shipment created as draft",
            ShipmentStatus.Booked => "Shipment booked",
            ShipmentStatus.PickedUp => "Shipment picked up from sender",
            ShipmentStatus.InTransit => "Shipment in transit",
            ShipmentStatus.OutForDelivery => "Shipment out for delivery",
            ShipmentStatus.Delivered => "Shipment delivered",
            ShipmentStatus.ReturnedToOrigin => "Shipment returned to origin",
            ShipmentStatus.Cancelled => "Shipment cancelled",
            ShipmentStatus.OnHold => "Shipment on hold",
            _ => "Status updated"
        };
    }

    public async Task<ShipmentChargeAudit> LogFreightChargeChangeAsync(
        Guid shipmentId, 
        string awbNumber, 
        string fieldName, 
        decimal oldValue, 
        decimal newValue, 
        string? changeReason, 
        Guid userId, 
        string? userName, 
        string? userEmail, 
        string? ipAddress)
    {
        var tenantId = _tenantProvider.CurrentTenantId 
            ?? throw new InvalidOperationException("Tenant context not available");

        var audit = new ShipmentChargeAudit
        {
            TenantId = tenantId,
            ShipmentId = shipmentId,
            AWBNumber = awbNumber,
            Action = ChargeAuditAction.FreightChargeModified,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            ChangeReason = changeReason,
            ModifiedByUserId = userId,
            ModifiedByUserName = userName,
            ModifiedByUserEmail = userEmail,
            ModifiedAt = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        _context.ShipmentChargeAudits.Add(audit);
        await _context.SaveChangesAsync();

        return audit;
    }

    public async Task<List<ShipmentChargeAudit>> GetChargeAuditHistoryAsync(Guid shipmentId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<ShipmentChargeAudit>();

        return await _context.ShipmentChargeAudits
            .Where(a => a.TenantId == tenantId.Value && a.ShipmentId == shipmentId)
            .OrderByDescending(a => a.ModifiedAt)
            .ToListAsync();
    }
}
