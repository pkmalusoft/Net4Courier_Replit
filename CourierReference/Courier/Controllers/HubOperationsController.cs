using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/hub")]
[Authorize]
public class HubOperationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubOperationsService _hubOperationsService;
    private readonly IManifestService _manifestService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<HubOperationsController> _logger;

    public HubOperationsController(
        AppDbContext context,
        IHubOperationsService hubOperationsService,
        IManifestService manifestService,
        ITenantProvider tenantProvider,
        ILogger<HubOperationsController> logger)
    {
        _context = context;
        _hubOperationsService = hubOperationsService;
        _manifestService = manifestService;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpGet("hubs")]
    public async Task<ActionResult<List<HubItemDto>>> GetHubs()
    {
        try
        {
            var hubs = await _context.Branches
                .Where(b => b.IsHub && b.IsActive)
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.Name)
                .Select(b => new HubItemDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Code = b.Code,
                    City = b.City,
                    IsHeadOffice = b.IsHeadOffice
                })
                .ToListAsync();

            return Ok(hubs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching hubs");
            return StatusCode(500, "An error occurred while fetching hubs.");
        }
    }

    [HttpPost("scan-arrival")]
    public async Task<ActionResult<ScanResultDto>> ScanArrival([FromBody] ScanArrivalRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            var shipment = await _hubOperationsService.ScanArrivalAsync(
                request.AWBNumber,
                request.HubId,
                request.HubName,
                request.ScannedByUserId);

            if (shipment == null)
            {
                return Ok(new ScanResultDto
                {
                    Success = false,
                    Message = "Shipment not found or scan failed",
                    AWBNumber = request.AWBNumber,
                    ScanTime = DateTime.UtcNow
                });
            }

            return Ok(new ScanResultDto
            {
                Success = true,
                Message = "Arrival scan successful",
                ShipmentId = shipment.Id,
                AWBNumber = shipment.AWBNumber,
                CurrentStatus = shipment.Status.ToString(),
                NewStatus = shipment.Status.ToString(),
                ScanTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning arrival for AWB {AWBNumber}", request.AWBNumber);
            return StatusCode(500, "An error occurred while processing the scan.");
        }
    }

    [HttpPost("scan-departure")]
    public async Task<ActionResult<ScanResultDto>> ScanDeparture([FromBody] ScanDepartureRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            if (!request.ManifestId.HasValue)
                return BadRequest("Manifest ID is required for departure scan.");

            var shipment = await _hubOperationsService.ScanDepartureAsync(
                request.AWBNumber,
                request.ManifestId.Value,
                request.ScannedByUserId);

            if (shipment == null)
            {
                return Ok(new ScanResultDto
                {
                    Success = false,
                    Message = "Shipment not found or scan failed",
                    AWBNumber = request.AWBNumber,
                    ScanTime = DateTime.UtcNow
                });
            }

            return Ok(new ScanResultDto
            {
                Success = true,
                Message = "Departure scan successful",
                ShipmentId = shipment.Id,
                AWBNumber = shipment.AWBNumber,
                CurrentStatus = shipment.Status.ToString(),
                NewStatus = shipment.Status.ToString(),
                ScanTime = DateTime.UtcNow,
                ManifestId = request.ManifestId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning departure for AWB {AWBNumber}", request.AWBNumber);
            return StatusCode(500, "An error occurred while processing the scan.");
        }
    }

    [HttpPost("scan-batch")]
    public async Task<ActionResult<BatchScanResultDto>> ScanBatch([FromBody] BatchScanRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            var results = new List<ScanResultDto>();
            int successCount = 0;
            int failureCount = 0;

            foreach (var awb in request.AWBNumbers)
            {
                Shipment? shipment;
                if (request.ScanType == "Arrival")
                {
                    shipment = await _hubOperationsService.ScanArrivalAsync(
                        awb, request.HubId, request.HubName, request.ScannedByUserId);
                }
                else
                {
                    if (!request.ManifestId.HasValue)
                    {
                        results.Add(new ScanResultDto
                        {
                            Success = false,
                            Message = "Manifest ID required for departure scan",
                            AWBNumber = awb,
                            ScanTime = DateTime.UtcNow
                        });
                        failureCount++;
                        continue;
                    }
                    shipment = await _hubOperationsService.ScanDepartureAsync(
                        awb, request.ManifestId.Value, request.ScannedByUserId);
                }

                if (shipment != null)
                {
                    successCount++;
                    results.Add(new ScanResultDto
                    {
                        Success = true,
                        Message = $"{request.ScanType} scan successful",
                        ShipmentId = shipment.Id,
                        AWBNumber = shipment.AWBNumber,
                        CurrentStatus = shipment.Status.ToString(),
                        NewStatus = shipment.Status.ToString(),
                        ScanTime = DateTime.UtcNow
                    });
                }
                else
                {
                    failureCount++;
                    results.Add(new ScanResultDto
                    {
                        Success = false,
                        Message = "Scan failed",
                        AWBNumber = awb,
                        ScanTime = DateTime.UtcNow
                    });
                }
            }

            return Ok(new BatchScanResultDto
            {
                TotalScanned = request.AWBNumbers.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Results = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch scan");
            return StatusCode(500, "An error occurred while processing the batch scan.");
        }
    }

    [HttpPost("manifests/{manifestId}/process")]
    public async Task<ActionResult<ManifestReceiveResultDto>> ProcessInboundManifest(
        Guid manifestId, 
        [FromBody] ProcessManifestRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            var shipments = await _hubOperationsService.ProcessInboundManifestAsync(
                manifestId,
                request.HubId,
                request.HubName,
                request.ProcessedByUserId);

            return Ok(new ManifestReceiveResultDto
            {
                Success = true,
                Message = "Manifest processed successfully",
                ManifestId = manifestId,
                ReceivedShipments = shipments.Count,
                ShortShipments = 0,
                OverShipments = 0,
                DamagedShipments = 0,
                Discrepancies = new List<DiscrepancyDto>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing manifest {ManifestId}", manifestId);
            return StatusCode(500, "An error occurred while processing the manifest.");
        }
    }

    [HttpGet("pending-inscan")]
    public async Task<ActionResult<List<ShipmentSummaryDto>>> GetPendingInScan()
    {
        try
        {
            var shipments = await _hubOperationsService.GetPendingInscanAsync();
            return Ok(shipments.Select(s => new ShipmentSummaryDto
            {
                Id = s.Id,
                AWBNumber = s.AWBNumber,
                Status = s.Status.ToString(),
                ReceiverName = s.ReceiverName,
                ReceiverCity = s.ReceiverCity,
                Pieces = s.NumberOfPieces,
                Weight = s.ChargeableWeight
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending inscan shipments");
            return StatusCode(500, "An error occurred while fetching pending shipments.");
        }
    }

    [HttpGet("pending-outscan")]
    public async Task<ActionResult<List<ShipmentSummaryDto>>> GetPendingOutScan([FromQuery] Guid hubId)
    {
        try
        {
            var shipments = await _hubOperationsService.GetPendingOutscanAsync(hubId);
            return Ok(shipments.Select(s => new ShipmentSummaryDto
            {
                Id = s.Id,
                AWBNumber = s.AWBNumber,
                Status = s.Status.ToString(),
                ReceiverName = s.ReceiverName,
                ReceiverCity = s.ReceiverCity,
                Pieces = s.NumberOfPieces,
                Weight = s.ChargeableWeight
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending outscan shipments for hub {HubId}", hubId);
            return StatusCode(500, "An error occurred while fetching pending shipments.");
        }
    }

    [HttpGet("shipments")]
    public async Task<ActionResult<List<ShipmentSummaryDto>>> GetShipmentsAtHub([FromQuery] Guid hubId)
    {
        try
        {
            var shipments = await _hubOperationsService.GetShipmentsAtHubAsync(hubId);
            return Ok(shipments.Select(s => new ShipmentSummaryDto
            {
                Id = s.Id,
                AWBNumber = s.AWBNumber,
                Status = s.Status.ToString(),
                ReceiverName = s.ReceiverName,
                ReceiverCity = s.ReceiverCity,
                Pieces = s.NumberOfPieces,
                Weight = s.ChargeableWeight
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching shipments at hub {HubId}", hubId);
            return StatusCode(500, "An error occurred while fetching shipments.");
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<HubSummaryDto>> GetHubSummary([FromQuery] Guid hubId, [FromQuery] DateTime? date)
    {
        try
        {
            var summaryDate = date ?? DateTime.UtcNow.Date;
            var summary = await _hubOperationsService.GetHubSummaryAsync(hubId, summaryDate);

            return Ok(new HubSummaryDto
            {
                HubId = hubId,
                Date = summaryDate,
                TotalShipmentsAtHub = summary.TotalShipmentsAtHub,
                PendingInscan = summary.PendingInscan,
                PendingOutscan = summary.PendingOutscan,
                PendingDelivery = summary.PendingDelivery,
                TodayInscans = summary.TodayInscans,
                TodayOutscans = summary.TodayOutscans,
                TotalCODPending = summary.TotalCODPending
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching hub summary for {HubId}", hubId);
            return StatusCode(500, "An error occurred while fetching hub summary.");
        }
    }

    [HttpGet("pending-arrivals")]
    public async Task<ActionResult<List<PendingManifestDto>>> GetPendingArrivals([FromQuery] Guid hubId)
    {
        try
        {
            var manifests = await _manifestService.GetPendingReceiveAsync(hubId);
            
            return Ok(manifests.Select(m => new PendingManifestDto
            {
                ManifestId = m.Id,
                ManifestNumber = m.ManifestNumber,
                OriginHubName = m.OriginHubName,
                DispatchedAt = m.DispatchedAt,
                ExpectedArrival = m.ExpectedArrival,
                TotalShipments = m.TotalShipments,
                TotalPieces = m.TotalPieces,
                TotalWeight = m.TotalWeight,
                VehicleNumber = m.VehicleNumber,
                DriverName = m.DriverName
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending arrivals for hub {HubId}", hubId);
            return StatusCode(500, "An error occurred while fetching pending arrivals.");
        }
    }
}

public class ScanArrivalRequest
{
    public string AWBNumber { get; set; } = string.Empty;
    public Guid HubId { get; set; }
    public string HubName { get; set; } = string.Empty;
    public Guid? ScannedByUserId { get; set; }
}

public class ScanDepartureRequest
{
    public string AWBNumber { get; set; } = string.Empty;
    public Guid HubId { get; set; }
    public Guid? ManifestId { get; set; }
    public Guid? ScannedByUserId { get; set; }
}

public class BatchScanRequest
{
    public List<string> AWBNumbers { get; set; } = new();
    public string ScanType { get; set; } = "Arrival";
    public Guid HubId { get; set; }
    public string HubName { get; set; } = string.Empty;
    public Guid? ManifestId { get; set; }
    public Guid? ScannedByUserId { get; set; }
}

public class ProcessManifestRequest
{
    public Guid HubId { get; set; }
    public string HubName { get; set; } = string.Empty;
    public Guid? ProcessedByUserId { get; set; }
}

public class ScanResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? ShipmentId { get; set; }
    public string? AWBNumber { get; set; }
    public string? CurrentStatus { get; set; }
    public string? NewStatus { get; set; }
    public DateTime ScanTime { get; set; }
    public Guid? ManifestId { get; set; }
    public string? ManifestNumber { get; set; }
}

public class BatchScanResultDto
{
    public int TotalScanned { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<ScanResultDto> Results { get; set; } = new();
}

public class ManifestReceiveResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid ManifestId { get; set; }
    public string ManifestNumber { get; set; } = string.Empty;
    public int ExpectedShipments { get; set; }
    public int ReceivedShipments { get; set; }
    public int ShortShipments { get; set; }
    public int OverShipments { get; set; }
    public int DamagedShipments { get; set; }
    public List<DiscrepancyDto> Discrepancies { get; set; } = new();
}

public class DiscrepancyDto
{
    public string AWBNumber { get; set; } = string.Empty;
    public string DiscrepancyType { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class PendingManifestDto
{
    public Guid ManifestId { get; set; }
    public string ManifestNumber { get; set; } = string.Empty;
    public string OriginHubName { get; set; } = string.Empty;
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ExpectedArrival { get; set; }
    public int TotalShipments { get; set; }
    public int TotalPieces { get; set; }
    public decimal TotalWeight { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
}

public class ShipmentSummaryDto
{
    public Guid Id { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ReceiverName { get; set; }
    public string? ReceiverCity { get; set; }
    public int Pieces { get; set; }
    public decimal Weight { get; set; }
}

public class HubSummaryDto
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

public class HubItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? City { get; set; }
    public bool IsHeadOffice { get; set; }
}
