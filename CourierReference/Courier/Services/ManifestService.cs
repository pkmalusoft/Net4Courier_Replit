using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Services;

public interface IManifestService
{
    Task<List<Manifest>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null, ManifestStatus? status = null, string? search = null);
    Task<Manifest?> GetByIdAsync(Guid id);
    Task<Manifest?> GetByNumberAsync(string manifestNumber);
    Task<Manifest> CreateManifestAsync(Guid originHubId, string originHubName, Guid destinationHubId, string destinationHubName, string? vehicleNumber = null, string? driverName = null);
    Task<ManifestItem?> AddShipmentToManifestAsync(Guid manifestId, string awbNumber, Guid? userId = null);
    Task<bool> RemoveShipmentFromManifestAsync(Guid manifestId, Guid shipmentId);
    Task<Manifest?> DispatchManifestAsync(Guid manifestId, string? sealNumber = null, Guid? userId = null);
    Task<Manifest?> ReceiveManifestAsync(Guid manifestId, Guid hubId, string hubName, Guid? userId = null, string? remarks = null);
    Task<List<Manifest>> GetPendingDispatchAsync(Guid hubId);
    Task<List<Manifest>> GetInTransitAsync();
    Task<List<Manifest>> GetPendingReceiveAsync(Guid hubId);
    Task<string> GenerateManifestNumberAsync();
}

public class ManifestService : IManifestService
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IShipmentService _shipmentService;
    private readonly IVoucherNumberingService _voucherNumberingService;
    private readonly ILogger<ManifestService> _logger;

    public ManifestService(
        AppDbContext context,
        ITenantProvider tenantProvider,
        IShipmentService shipmentService,
        IVoucherNumberingService voucherNumberingService,
        ILogger<ManifestService> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _shipmentService = shipmentService;
        _voucherNumberingService = voucherNumberingService;
        _logger = logger;
    }

    public async Task<List<Manifest>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null, ManifestStatus? status = null, string? search = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Manifest>();

        var query = _context.Manifests
            .Include(m => m.Items)
            .Where(m => m.TenantId == tenantId.Value);

        if (fromDate.HasValue)
        {
            var utcFromDate = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
            query = query.Where(m => m.CreatedAt >= utcFromDate);
        }

        if (toDate.HasValue)
        {
            var utcToDate = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
            query = query.Where(m => m.CreatedAt <= utcToDate);
        }

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.ManifestNumber.Contains(search));

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Manifest?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.Manifests
            .Include(m => m.Items)
                .ThenInclude(i => i.Shipment)
            .Include(m => m.Department)
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId.Value);
    }

    public async Task<Manifest?> GetByNumberAsync(string manifestNumber)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        return await _context.Manifests
            .Include(m => m.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(m => m.ManifestNumber == manifestNumber && m.TenantId == tenantId.Value);
    }

    public async Task<Manifest> CreateManifestAsync(
        Guid originHubId, 
        string originHubName, 
        Guid destinationHubId, 
        string destinationHubName, 
        string? vehicleNumber = null, 
        string? driverName = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var manifest = new Manifest
        {
            ManifestNumber = await GenerateManifestNumberAsync(),
            ManifestDate = DateTime.UtcNow,
            OriginHubId = originHubId,
            OriginHubName = originHubName,
            DestinationHubId = destinationHubId,
            DestinationHubName = destinationHubName,
            VehicleNumber = vehicleNumber,
            DriverName = driverName,
            Status = ManifestStatus.Open,
            TotalPieces = 0,
            TotalWeight = 0,
            TotalCODAmount = 0
        };

        _context.Manifests.Add(manifest);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Manifest created: {ManifestNumber} from {Origin} to {Destination}", 
            manifest.ManifestNumber, originHubName, destinationHubName);

        return manifest;
    }

    public async Task<ManifestItem?> AddShipmentToManifestAsync(Guid manifestId, string awbNumber, Guid? userId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var manifest = await _context.Manifests
            .FirstOrDefaultAsync(m => m.Id == manifestId && m.TenantId == tenantId.Value);

        if (manifest == null || manifest.Status != ManifestStatus.Open)
        {
            _logger.LogWarning("Cannot add to manifest: {ManifestId}, Status: {Status}", manifestId, manifest?.Status);
            return null;
        }

        var shipment = await _shipmentService.GetByAWBAsync(awbNumber);
        if (shipment == null)
        {
            _logger.LogWarning("Shipment not found: {AWB}", awbNumber);
            return null;
        }

        var existingItem = await _context.ManifestItems
            .AnyAsync(m => m.ManifestId == manifestId && m.ShipmentId == shipment.Id);

        if (existingItem)
        {
            _logger.LogWarning("Shipment already in manifest: {AWB}", awbNumber);
            return null;
        }

        var manifestItem = new ManifestItem
        {
            ManifestId = manifestId,
            ShipmentId = shipment.Id,
            AWBNumber = awbNumber,
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

        shipment.OutboundManifestId = manifestId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Shipment added to manifest: {AWB} -> {Manifest}", awbNumber, manifest.ManifestNumber);

        return manifestItem;
    }

    public async Task<bool> RemoveShipmentFromManifestAsync(Guid manifestId, Guid shipmentId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return false;

        var manifest = await _context.Manifests
            .FirstOrDefaultAsync(m => m.Id == manifestId && m.TenantId == tenantId.Value);

        if (manifest == null || manifest.Status != ManifestStatus.Open)
            return false;

        var manifestItem = await _context.ManifestItems
            .Include(m => m.Shipment)
            .FirstOrDefaultAsync(m => m.ManifestId == manifestId && m.ShipmentId == shipmentId);

        if (manifestItem == null)
            return false;

        manifest.TotalPieces--;
        manifest.TotalWeight -= manifestItem.Weight;
        if (manifestItem.IsCOD)
        {
            manifest.TotalCODAmount -= manifestItem.CODAmount;
        }

        if (manifestItem.Shipment != null)
        {
            manifestItem.Shipment.OutboundManifestId = null;
        }

        _context.ManifestItems.Remove(manifestItem);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Manifest?> DispatchManifestAsync(Guid manifestId, string? sealNumber = null, Guid? userId = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var manifest = await _context.Manifests
            .Include(m => m.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(m => m.Id == manifestId && m.TenantId == tenantId.Value);

        if (manifest == null)
        {
            _logger.LogWarning("Manifest not found: {ManifestId}", manifestId);
            return null;
        }

        if (manifest.Status != ManifestStatus.Open)
        {
            _logger.LogWarning("Manifest not in open status: {ManifestId}, Status: {Status}", manifestId, manifest.Status);
            return null;
        }

        if (manifest.Items.Count == 0)
        {
            _logger.LogWarning("Cannot dispatch empty manifest: {ManifestId}", manifestId);
            return null;
        }

        manifest.Status = ManifestStatus.Dispatched;
        manifest.SealNumber = sealNumber;
        manifest.DispatchedAt = DateTime.UtcNow;
        manifest.DispatchedByUserId = userId;

        foreach (var item in manifest.Items)
        {
            if (item.Shipment != null)
            {
                item.Shipment.Status = ShipmentStatus.InTransit;
                item.Shipment.LastScanTime = DateTime.UtcNow;

                await _shipmentService.AddTrackingEventAsync(
                    item.Shipment.Id,
                    ShipmentStatus.InTransit,
                    $"Dispatched via manifest {manifest.ManifestNumber} to {manifest.DestinationHubName}",
                    manifest.OriginHubName,
                    userId);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Manifest dispatched: {ManifestNumber}, Pieces: {Pieces}", 
            manifest.ManifestNumber, manifest.TotalPieces);

        return manifest;
    }

    public async Task<Manifest?> ReceiveManifestAsync(Guid manifestId, Guid hubId, string hubName, Guid? userId = null, string? remarks = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return null;

        var manifest = await _context.Manifests
            .Include(m => m.Items)
                .ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(m => m.Id == manifestId && m.TenantId == tenantId.Value);

        if (manifest == null)
        {
            _logger.LogWarning("Manifest not found: {ManifestId}", manifestId);
            return null;
        }

        if (manifest.Status != ManifestStatus.Dispatched)
        {
            _logger.LogWarning("Manifest not in dispatched status: {ManifestId}, Status: {Status}", manifestId, manifest.Status);
            return null;
        }

        var receiveTime = DateTime.UtcNow;
        manifest.Status = ManifestStatus.Received;
        manifest.ReceivedAt = receiveTime;
        manifest.ReceivedByUserId = userId;
        manifest.ReceiveRemarks = remarks;
        manifest.ActualPiecesReceived = manifest.Items.Count;

        foreach (var item in manifest.Items)
        {
            item.ReceiveTime = receiveTime;
            item.ReceivedByUserId = userId;

            if (item.Shipment != null)
            {
                item.Shipment.InboundManifestId = manifestId;
                item.Shipment.CurrentHubId = hubId;
                item.Shipment.CurrentHubName = hubName;
                item.Shipment.LastScanTime = receiveTime;
                item.Shipment.Status = ShipmentStatus.InScan;

                await _shipmentService.AddTrackingEventAsync(
                    item.Shipment.Id,
                    ShipmentStatus.InScan,
                    $"Received at {hubName} via manifest {manifest.ManifestNumber}",
                    hubName,
                    userId);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Manifest received: {ManifestNumber} at {Hub}, Pieces: {Pieces}", 
            manifest.ManifestNumber, hubName, manifest.ActualPiecesReceived);

        return manifest;
    }

    public async Task<List<Manifest>> GetPendingDispatchAsync(Guid hubId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Manifest>();

        return await _context.Manifests
            .Include(m => m.Items)
            .Where(m => m.TenantId == tenantId.Value 
                && m.OriginHubId == hubId 
                && m.Status == ManifestStatus.Open)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Manifest>> GetInTransitAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Manifest>();

        return await _context.Manifests
            .Include(m => m.Items)
            .Where(m => m.TenantId == tenantId.Value 
                && m.Status == ManifestStatus.Dispatched)
            .OrderBy(m => m.DispatchedAt)
            .ToListAsync();
    }

    public async Task<List<Manifest>> GetPendingReceiveAsync(Guid hubId)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return new List<Manifest>();

        return await _context.Manifests
            .Include(m => m.Items)
            .Where(m => m.TenantId == tenantId.Value 
                && m.DestinationHubId == hubId 
                && m.Status == ManifestStatus.Dispatched)
            .OrderBy(m => m.DispatchedAt)
            .ToListAsync();
    }

    public async Task<string> GenerateManifestNumberAsync()
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            throw new InvalidOperationException("Tenant context required");

        var today = DateTime.UtcNow;
        var prefix = $"MAN{today:yyyyMMdd}";
        
        var lastManifest = await _context.Manifests
            .Where(m => m.TenantId == tenantId.Value && m.ManifestNumber.StartsWith(prefix))
            .OrderByDescending(m => m.ManifestNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastManifest != null)
        {
            var lastNumberStr = lastManifest.ManifestNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }
}
