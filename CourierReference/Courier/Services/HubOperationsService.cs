using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface IHubOperationsService
{
    Task<Shipment?> ScanArrivalAsync(string awbNumber, Guid hubId, string hubName, Guid? userId = null);
    Task<Shipment?> ScanDepartureAsync(string awbNumber, Guid manifestId, Guid? userId = null);
    Task<List<Shipment>> ProcessInboundManifestAsync(Guid manifestId, Guid hubId, string hubName, Guid? userId = null);
    Task<List<Shipment>> GetShipmentsAtHubAsync(Guid hubId);
    Task<List<Shipment>> GetPendingInscanAsync();
    Task<List<Shipment>> GetPendingOutscanAsync(Guid hubId);
    Task<HubOperationsSummary> GetHubSummaryAsync(Guid hubId, DateTime date);
}

public class HubOperationsService : IHubOperationsService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IShipmentService _shipmentService;
    private readonly ILogger<HubOperationsService> _logger;

    public HubOperationsService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IShipmentService shipmentService,
        ILogger<HubOperationsService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _shipmentService = shipmentService;
        _logger = logger;
    }

    public async Task<Shipment?> ScanArrivalAsync(string awbNumber, Guid hubId, string hubName, Guid? userId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var shipment = await _shipmentService.GetByAWBAsync(awbNumber);
        if (shipment == null)
        {
            _logger.LogWarning("Shipment not found for AWB: {AWB}", awbNumber);
            return null;
        }

        if (shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Cancelled)
        {
            _logger.LogWarning("Cannot inscan shipment in status {Status}: {AWB}", shipment.Status, awbNumber);
            return null;
        }

        shipment.CurrentHubId = hubId;
        shipment.CurrentHubName = hubName;
        shipment.LastScanTime = DateTime.UtcNow;
        shipment.Status = ShipmentStatus.InScan;

        await _context.SaveChangesAsync();

        await _shipmentService.AddTrackingEventAsync(
            shipment.Id,
            ShipmentStatus.InScan,
            $"Arrived at hub: {hubName}",
            hubName,
            userId.HasValue ? userId : null);

        _logger.LogInformation("InScan completed for AWB: {AWB} at hub: {Hub}", awbNumber, hubName);

        return shipment;
    }

    public async Task<Shipment?> ScanDepartureAsync(string awbNumber, Guid manifestId, Guid? userId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var shipment = await _shipmentService.GetByAWBAsync(awbNumber);
        if (shipment == null)
        {
            _logger.LogWarning("Shipment not found for AWB: {AWB}", awbNumber);
            return null;
        }

        var manifest = await _context.Manifests
            .FirstOrDefaultAsync(m => m.Id == manifestId && m.TenantId == tenantId.Value);

        if (manifest == null)
        {
            _logger.LogWarning("Manifest not found: {ManifestId}", manifestId);
            return null;
        }

        shipment.OutboundManifestId = manifestId;
        shipment.LastScanTime = DateTime.UtcNow;
        shipment.Status = ShipmentStatus.InTransit;

        var manifestItem = new ManifestItem
        {
            ManifestId = manifestId,
            ShipmentId = shipment.Id,
            AWBNumber = shipment.AWBNumber,
            ScanTime = DateTime.UtcNow,
            ScannedByUserId = userId,
            Weight = shipment.ChargeableWeight,
            IsCOD = shipment.PaymentMode == PaymentMode.COD,
            CODAmount = shipment.PaymentMode == PaymentMode.COD ? shipment.CODAmount : 0
        };

        _context.ManifestItems.Add(manifestItem);
        
        manifest.TotalPieces++;
        manifest.TotalWeight += shipment.ChargeableWeight;
        if (shipment.PaymentMode == PaymentMode.COD)
        {
            manifest.TotalCODAmount += shipment.CODAmount;
        }

        await _context.SaveChangesAsync();

        await _shipmentService.AddTrackingEventAsync(
            shipment.Id,
            ShipmentStatus.InTransit,
            $"Dispatched via manifest: {manifest.ManifestNumber}",
            manifest.OriginHubName,
            userId);

        _logger.LogInformation("OutScan completed for AWB: {AWB} on manifest: {Manifest}", 
            awbNumber, manifest.ManifestNumber);

        return shipment;
    }

    public async Task<List<Shipment>> ProcessInboundManifestAsync(Guid manifestId, Guid hubId, string hubName, Guid? userId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        var manifest = await _context.Manifests
            .Include(m => m.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(m => m.Id == manifestId && m.TenantId == tenantId.Value);

        if (manifest == null)
        {
            _logger.LogWarning("Manifest not found: {ManifestId}", manifestId);
            return new List<Shipment>();
        }

        if (manifest.Status != ManifestStatus.Dispatched)
        {
            _logger.LogWarning("Manifest not in dispatched status: {ManifestId}, Status: {Status}", 
                manifestId, manifest.Status);
            return new List<Shipment>();
        }

        var processedShipments = new List<Shipment>();
        var receiveTime = DateTime.UtcNow;

        foreach (var item in manifest.Items)
        {
            if (item.Shipment != null)
            {
                item.Shipment.InboundManifestId = manifestId;
                item.Shipment.CurrentHubId = hubId;
                item.Shipment.CurrentHubName = hubName;
                item.Shipment.LastScanTime = receiveTime;
                item.Shipment.Status = ShipmentStatus.InScan;

                item.ReceiveTime = receiveTime;
                item.ReceivedByUserId = userId;

                await _shipmentService.AddTrackingEventAsync(
                    item.Shipment.Id,
                    ShipmentStatus.InScan,
                    $"Received at hub: {hubName} via manifest: {manifest.ManifestNumber}",
                    hubName,
                    userId);

                processedShipments.Add(item.Shipment);
            }
        }

        manifest.Status = ManifestStatus.Received;
        manifest.ReceivedAt = receiveTime;
        manifest.ReceivedByUserId = userId;
        manifest.ActualPiecesReceived = processedShipments.Count;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Inbound manifest processed: {Manifest}, Shipments: {Count}", 
            manifest.ManifestNumber, processedShipments.Count);

        return processedShipments;
    }

    public async Task<List<Shipment>> GetShipmentsAtHubAsync(Guid hubId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        return await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.DestinationZone)
            .Include(s => s.Customer)
            .Where(s => s.TenantId == tenantId.Value 
                && s.CurrentHubId == hubId 
                && !s.IsVoided
                && s.Status != ShipmentStatus.Delivered
                && s.Status != ShipmentStatus.Cancelled)
            .OrderByDescending(s => s.LastScanTime)
            .ToListAsync();
    }

    public async Task<List<Shipment>> GetPendingInscanAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        return await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.OriginZone)
            .Include(s => s.DestinationZone)
            .Where(s => s.TenantId == tenantId.Value 
                && !s.IsVoided
                && (s.Status == ShipmentStatus.PickedUp || s.Status == ShipmentStatus.Booked))
            .OrderBy(s => s.BookingDate)
            .ToListAsync();
    }

    public async Task<List<Shipment>> GetPendingOutscanAsync(Guid hubId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Shipment>();

        return await _context.Shipments
            .Include(s => s.CourierServiceType)
            .Include(s => s.DestinationZone)
            .Where(s => s.TenantId == tenantId.Value 
                && s.CurrentHubId == hubId
                && !s.IsVoided
                && s.Status == ShipmentStatus.InScan
                && s.OutboundManifestId == null)
            .OrderBy(s => s.LastScanTime)
            .ToListAsync();
    }

    public async Task<HubOperationsSummary> GetHubSummaryAsync(Guid hubId, DateTime date)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new HubOperationsSummary();

        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var shipmentsAtHub = await _context.Shipments
            .Where(s => s.TenantId == tenantId.Value 
                && s.CurrentHubId == hubId 
                && !s.IsVoided)
            .ToListAsync();

        var todayInscans = await _context.ShipmentTrackings
            .Include(t => t.Shipment)
            .Where(t => t.Shipment.TenantId == tenantId.Value 
                && t.Status == ShipmentStatus.InScan
                && t.EventDateTime >= startOfDay 
                && t.EventDateTime < endOfDay
                && t.Location != null)
            .CountAsync();

        var todayOutscans = await _context.ManifestItems
            .Include(m => m.Manifest)
            .Where(m => m.Manifest.TenantId == tenantId.Value 
                && m.Manifest.OriginHubId == hubId
                && m.ScanTime >= startOfDay 
                && m.ScanTime < endOfDay)
            .CountAsync();

        return new HubOperationsSummary
        {
            HubId = hubId,
            Date = date,
            TotalShipmentsAtHub = shipmentsAtHub.Count,
            PendingInscan = shipmentsAtHub.Count(s => s.Status == ShipmentStatus.PickedUp),
            PendingOutscan = shipmentsAtHub.Count(s => s.Status == ShipmentStatus.InScan && s.OutboundManifestId == null),
            PendingDelivery = shipmentsAtHub.Count(s => s.Status == ShipmentStatus.InScan || s.Status == ShipmentStatus.OutForDelivery),
            TodayInscans = todayInscans,
            TodayOutscans = todayOutscans,
            TotalCODPending = shipmentsAtHub
                .Where(s => s.PaymentMode == PaymentMode.COD && s.Status != ShipmentStatus.Delivered)
                .Sum(s => s.CODAmount)
        };
    }
}

public class HubOperationsSummary
{
    public Guid HubId { get; set; }
    public DateTime Date { get; set; }
    public int TotalShipmentsAtHub { get; set; }
    public int PendingInscan { get; set; }
    public int PendingOutscan { get; set; }
    public int PendingDelivery { get; set; }
    public int TodayInscans { get; set; }
    public int TodayOutscans { get; set; }
    public decimal TotalCODPending { get; set; }
}
