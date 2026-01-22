using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface IDRSService
{
    Task<List<DeliveryRunSheet>> GetAllAsync(DateTime? date = null, DRSStatus? status = null);
    Task<DeliveryRunSheet?> GetByIdAsync(Guid id);
    Task<DeliveryRunSheet?> GetByNumberAsync(string drsNumber);
    Task<DeliveryRunSheet> GenerateDrsAsync(Guid hubId, string hubName, Guid driverId, string driverName, string? vehicleNumber = null, Guid? routeZoneId = null);
    Task<DRSItem?> AddShipmentToDrsAsync(Guid drsId, string awbNumber, int? sequenceNumber = null);
    Task<bool> RemoveShipmentFromDrsAsync(Guid drsId, Guid shipmentId);
    Task<DRSItem?> UpdateDeliveryStatusAsync(Guid drsItemId, DRSItemStatus status, string? remarks = null, string? podImageUrl = null, string? signatureImageUrl = null, decimal? freightCollected = null);
    Task<DeliveryRunSheet?> DispatchDrsAsync(Guid drsId, Guid? userId = null);
    Task<DeliveryRunSheet?> ReconcileDrsAsync(Guid drsId, decimal cashDeposited, decimal driverExpenses, string? notes = null, Guid? userId = null);
    Task<decimal> CalculateCodExpectedAsync(Guid drsId);
    Task<List<DeliveryRunSheet>> GetPendingReconciliationAsync();
    Task<DRSSummary> GetDrsSummaryAsync(Guid drsId);
    Task<string> GenerateDrsNumberAsync();
}

public class DRSService : IDRSService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IShipmentService _shipmentService;
    private readonly ILogger<DRSService> _logger;

    public DRSService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IShipmentService shipmentService,
        ILogger<DRSService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _shipmentService = shipmentService;
        _logger = logger;
    }

    public async Task<List<DeliveryRunSheet>> GetAllAsync(DateTime? date = null, DRSStatus? status = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<DeliveryRunSheet>();

        var query = _context.DeliveryRunSheets
            .Include(d => d.Items)
            .Where(d => d.TenantId == tenantId.Value);

        if (date.HasValue)
        {
            var startOfDay = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);
            query = query.Where(d => d.DrsDate >= startOfDay && d.DrsDate < endOfDay);
        }

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        return await query
            .OrderByDescending(d => d.DrsDate)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<DeliveryRunSheet?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.DeliveryRunSheets
            .Include(d => d.Items)
                .ThenInclude(i => i.Shipment)
                    .ThenInclude(s => s.Customer)
            .Include(d => d.Items)
                .ThenInclude(i => i.Shipment)
                    .ThenInclude(s => s.DestinationZone)
            .Include(d => d.RouteZone)
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId.Value);
    }

    public async Task<DeliveryRunSheet?> GetByNumberAsync(string drsNumber)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.DeliveryRunSheets
            .Include(d => d.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(d => d.DRSNumber == drsNumber && d.TenantId == tenantId.Value);
    }

    public async Task<DeliveryRunSheet> GenerateDrsAsync(
        Guid hubId, 
        string hubName, 
        Guid driverId, 
        string driverName, 
        string? vehicleNumber = null, 
        Guid? routeZoneId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var drs = new DeliveryRunSheet
        {
            DRSNumber = await GenerateDrsNumberAsync(),
            DrsDate = DateTime.UtcNow.Date,
            HubId = hubId,
            HubName = hubName,
            DriverId = driverId,
            DriverName = driverName,
            VehicleNumber = vehicleNumber,
            RouteZoneId = routeZoneId,
            Status = DRSStatus.Draft,
            TotalShipments = 0,
            DeliveredCount = 0,
            TotalCODExpected = 0,
            TotalCODCollected = 0,
            TotalCashDeposited = 0,
            DriverExpenses = 0,
            ShortageAmount = 0,
            IsReconciled = false
        };

        _context.DeliveryRunSheets.Add(drs);
        await _context.SaveChangesAsync();

        _logger.LogInformation("DRS created: {DRSNumber} for driver: {Driver}", drs.DRSNumber, driverName);

        return drs;
    }

    public async Task<DRSItem?> AddShipmentToDrsAsync(Guid drsId, string awbNumber, int? sequenceNumber = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var drs = await _context.DeliveryRunSheets
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.Id == drsId && d.TenantId == tenantId.Value);

        if (drs == null || (drs.Status != DRSStatus.Draft && drs.Status != DRSStatus.Open))
        {
            _logger.LogWarning("Cannot add to DRS: {DRSId}, Status: {Status}", drsId, drs?.Status);
            return null;
        }

        var shipment = await _shipmentService.GetByAWBAsync(awbNumber);
        if (shipment == null)
        {
            _logger.LogWarning("Shipment not found: {AWB}", awbNumber);
            return null;
        }

        var existingItem = drs.Items.Any(i => i.ShipmentId == shipment.Id);
        if (existingItem)
        {
            _logger.LogWarning("Shipment already in DRS: {AWB}", awbNumber);
            return null;
        }

        var drsItem = new DRSItem
        {
            DeliveryRunSheetId = drsId,
            ShipmentId = shipment.Id,
            SequenceNumber = sequenceNumber ?? (drs.Items.Count + 1),
            Status = DRSItemStatus.Pending,
            ReceiverName = shipment.ReceiverName,
            ReceiverAddress = shipment.ReceiverAddress ?? "",
            ReceiverPhone = shipment.ReceiverPhone ?? "",
            CODAmount = shipment.PaymentMode == PaymentMode.COD ? shipment.CODAmount : 0,
            FreightAmount = shipment.TotalCharge,
            AttemptNumber = 1
        };

        _context.DRSItems.Add(drsItem);

        drs.TotalShipments++;
        if (shipment.PaymentMode == PaymentMode.COD)
        {
            drs.TotalCODExpected += shipment.CODAmount;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Shipment added to DRS: {AWB} -> {DRS}", awbNumber, drs.DRSNumber);

        return drsItem;
    }

    public async Task<bool> RemoveShipmentFromDrsAsync(Guid drsId, Guid shipmentId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return false;

        var drs = await _context.DeliveryRunSheets
            .FirstOrDefaultAsync(d => d.Id == drsId && d.TenantId == tenantId.Value);

        if (drs == null || drs.Status != DRSStatus.Draft)
            return false;

        var drsItem = await _context.DRSItems
            .FirstOrDefaultAsync(i => i.DeliveryRunSheetId == drsId && i.ShipmentId == shipmentId);

        if (drsItem == null)
            return false;

        drs.TotalShipments--;
        drs.TotalCODExpected -= drsItem.CODAmount;

        _context.DRSItems.Remove(drsItem);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<DRSItem?> UpdateDeliveryStatusAsync(
        Guid drsItemId, 
        DRSItemStatus status, 
        string? remarks = null, 
        string? podImageUrl = null, 
        string? signatureImageUrl = null, 
        decimal? freightCollected = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var drsItem = await _context.DRSItems
            .Include(i => i.DeliveryRunSheet)
            .Include(i => i.Shipment)
            .FirstOrDefaultAsync(i => i.Id == drsItemId && i.DeliveryRunSheet.TenantId == tenantId.Value);

        if (drsItem == null)
            return null;

        var previousStatus = drsItem.Status;
        drsItem.Status = status;
        drsItem.StatusUpdateTime = DateTime.UtcNow;
        drsItem.Remarks = remarks;

        if (status == DRSItemStatus.Delivered)
        {
            drsItem.DeliveryTime = DateTime.UtcNow;
            drsItem.PODImageUrl = podImageUrl;
            drsItem.SignatureImageUrl = signatureImageUrl;
            drsItem.FreightCollected = freightCollected ?? 0;

            if (drsItem.Shipment != null)
            {
                drsItem.Shipment.Status = ShipmentStatus.Delivered;
                drsItem.Shipment.ActualDeliveryDate = DateTime.UtcNow;

                await _shipmentService.AddTrackingEventAsync(
                    drsItem.Shipment.Id,
                    ShipmentStatus.Delivered,
                    $"Delivered. {remarks ?? ""}".Trim(),
                    drsItem.Shipment.ReceiverCity);
            }

            if (previousStatus != DRSItemStatus.Delivered)
            {
                drsItem.DeliveryRunSheet.DeliveredCount++;
                drsItem.DeliveryRunSheet.TotalCODCollected += drsItem.CODAmount;
            }
        }
        else if (status == DRSItemStatus.Undelivered || status == DRSItemStatus.Refused)
        {
            if (drsItem.Shipment != null)
            {
                drsItem.Shipment.Status = ShipmentStatus.OnHold;

                await _shipmentService.AddTrackingEventAsync(
                    drsItem.Shipment.Id,
                    ShipmentStatus.OnHold,
                    $"Delivery attempt failed: {remarks ?? status.ToString()}",
                    drsItem.Shipment.ReceiverCity);
            }
        }
        else if (status == DRSItemStatus.Rescheduled)
        {
            drsItem.AttemptNumber++;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("DRS item status updated: {ItemId} -> {Status}", drsItemId, status);

        return drsItem;
    }

    public async Task<DeliveryRunSheet?> DispatchDrsAsync(Guid drsId, Guid? userId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var drs = await _context.DeliveryRunSheets
            .Include(d => d.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(d => d.Id == drsId && d.TenantId == tenantId.Value);

        if (drs == null || drs.Status != DRSStatus.Draft)
        {
            _logger.LogWarning("Cannot dispatch DRS: {DRSId}, Status: {Status}", drsId, drs?.Status);
            return null;
        }

        if (drs.Items.Count == 0)
        {
            _logger.LogWarning("Cannot dispatch empty DRS: {DRSId}", drsId);
            return null;
        }

        drs.Status = DRSStatus.Dispatched;
        drs.DispatchTime = DateTime.UtcNow;

        foreach (var item in drs.Items)
        {
            item.Status = DRSItemStatus.OutForDelivery;

            if (item.Shipment != null)
            {
                item.Shipment.Status = ShipmentStatus.OutForDelivery;
                item.Shipment.LastScanTime = DateTime.UtcNow;

                await _shipmentService.AddTrackingEventAsync(
                    item.Shipment.Id,
                    ShipmentStatus.OutForDelivery,
                    $"Out for delivery with {drs.DriverName}",
                    drs.HubName);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("DRS dispatched: {DRSNumber}, Items: {Count}", drs.DRSNumber, drs.TotalShipments);

        return drs;
    }

    public async Task<DeliveryRunSheet?> ReconcileDrsAsync(
        Guid drsId, 
        decimal cashDeposited, 
        decimal driverExpenses, 
        string? notes = null, 
        Guid? userId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var drs = await _context.DeliveryRunSheets
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.Id == drsId && d.TenantId == tenantId.Value);

        if (drs == null || drs.IsReconciled)
        {
            _logger.LogWarning("Cannot reconcile DRS: {DRSId}, Already reconciled: {Reconciled}", drsId, drs?.IsReconciled);
            return null;
        }

        var expectedCash = drs.TotalCODCollected - driverExpenses;
        var shortage = expectedCash - cashDeposited;

        drs.TotalCashDeposited = cashDeposited;
        drs.DriverExpenses = driverExpenses;
        drs.ShortageAmount = shortage > 0 ? shortage : 0;
        drs.ReconciliationNotes = notes;
        drs.IsReconciled = true;
        drs.ReconciledByUserId = userId;
        drs.ReconciledAt = DateTime.UtcNow;
        drs.Status = DRSStatus.Closed;
        drs.ReturnTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("DRS reconciled: {DRSNumber}, Cash: {Cash}, Expenses: {Expenses}, Shortage: {Shortage}", 
            drs.DRSNumber, cashDeposited, driverExpenses, drs.ShortageAmount);

        return drs;
    }

    public async Task<decimal> CalculateCodExpectedAsync(Guid drsId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return 0;

        var drs = await _context.DeliveryRunSheets
            .Include(d => d.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(d => d.Id == drsId && d.TenantId == tenantId.Value);

        if (drs == null)
            return 0;

        return drs.Items
            .Where(i => i.Status == DRSItemStatus.Delivered && i.Shipment?.PaymentMode == PaymentMode.COD)
            .Sum(i => i.CODAmount);
    }

    public async Task<List<DeliveryRunSheet>> GetPendingReconciliationAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<DeliveryRunSheet>();

        return await _context.DeliveryRunSheets
            .Include(d => d.Items)
            .Where(d => d.TenantId == tenantId.Value 
                && !d.IsReconciled 
                && d.Status == DRSStatus.Dispatched)
            .OrderBy(d => d.DrsDate)
            .ToListAsync();
    }

    public async Task<DRSSummary> GetDrsSummaryAsync(Guid drsId)
    {
        var drs = await GetByIdAsync(drsId);
        if (drs == null)
            return new DRSSummary();

        return new DRSSummary
        {
            DrsId = drsId,
            DrsNumber = drs.DRSNumber,
            TotalShipments = drs.TotalShipments,
            DeliveredCount = drs.DeliveredCount,
            PendingCount = drs.Items.Count(i => i.Status == DRSItemStatus.Pending || i.Status == DRSItemStatus.OutForDelivery),
            UndeliveredCount = drs.Items.Count(i => i.Status == DRSItemStatus.Undelivered || i.Status == DRSItemStatus.Refused),
            RescheduledCount = drs.Items.Count(i => i.Status == DRSItemStatus.Rescheduled),
            TotalCODExpected = drs.TotalCODExpected,
            TotalCODCollected = drs.TotalCODCollected,
            TotalFreightCollected = drs.Items.Sum(i => i.FreightCollected),
            DeliveryRate = drs.TotalShipments > 0 ? (decimal)drs.DeliveredCount / drs.TotalShipments * 100 : 0
        };
    }

    public async Task<string> GenerateDrsNumberAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var today = DateTime.UtcNow;
        var prefix = $"DRS{today:yyyyMMdd}";

        var lastDrs = await _context.DeliveryRunSheets
            .Where(d => d.TenantId == tenantId.Value && d.DRSNumber.StartsWith(prefix))
            .OrderByDescending(d => d.DRSNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastDrs != null)
        {
            var lastNumberStr = lastDrs.DRSNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }
}

public class DRSSummary
{
    public Guid DrsId { get; set; }
    public string DrsNumber { get; set; } = string.Empty;
    public int TotalShipments { get; set; }
    public int DeliveredCount { get; set; }
    public int PendingCount { get; set; }
    public int UndeliveredCount { get; set; }
    public int RescheduledCount { get; set; }
    public decimal TotalCODExpected { get; set; }
    public decimal TotalCODCollected { get; set; }
    public decimal TotalFreightCollected { get; set; }
    public decimal DeliveryRate { get; set; }
}
