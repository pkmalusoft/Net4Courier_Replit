using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/manifests")]
[Authorize]
public class ManifestController : ControllerBase
{
    private readonly IManifestService _manifestService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<ManifestController> _logger;

    public ManifestController(
        IManifestService manifestService,
        ITenantProvider tenantProvider,
        ILogger<ManifestController> logger)
    {
        _manifestService = manifestService;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ManifestListDto>>> GetManifests(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? status,
        [FromQuery] string? search)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            ManifestStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ManifestStatus>(status, out var s))
                statusEnum = s;

            var manifests = await _manifestService.GetAllAsync(fromDate, toDate, statusEnum, search);

            return Ok(manifests.Select(MapToListDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching manifests");
            return StatusCode(500, "An error occurred while fetching manifests.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ManifestDetailDto>> GetManifest(Guid id)
    {
        try
        {
            var manifest = await _manifestService.GetByIdAsync(id);
            if (manifest == null)
                return NotFound();

            return Ok(MapToDetailDto(manifest));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching manifest {ManifestId}", id);
            return StatusCode(500, "An error occurred while fetching the manifest.");
        }
    }

    [HttpGet("number/{manifestNumber}")]
    public async Task<ActionResult<ManifestDetailDto>> GetManifestByNumber(string manifestNumber)
    {
        try
        {
            var manifest = await _manifestService.GetByNumberAsync(manifestNumber);
            if (manifest == null)
                return NotFound();

            return Ok(MapToDetailDto(manifest));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching manifest by number {ManifestNumber}", manifestNumber);
            return StatusCode(500, "An error occurred while fetching the manifest.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ManifestDetailDto>> CreateManifest([FromBody] CreateManifestRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            var manifest = await _manifestService.CreateManifestAsync(
                request.OriginHubId,
                request.OriginHubName,
                request.DestinationHubId,
                request.DestinationHubName,
                request.VehicleNumber,
                request.DriverName);

            return CreatedAtAction(nameof(GetManifest), new { id = manifest.Id }, MapToDetailDto(manifest));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating manifest");
            return StatusCode(500, "An error occurred while creating the manifest.");
        }
    }

    [HttpPost("{id}/shipments")]
    public async Task<ActionResult<ManifestItemDto>> AddShipment(
        Guid id, 
        [FromBody] AddShipmentToManifestRequest request)
    {
        try
        {
            var manifest = await _manifestService.GetByIdAsync(id);
            if (manifest == null)
                return NotFound();

            if (manifest.Status != ManifestStatus.Open)
                return BadRequest("Can only add shipments to open manifests.");

            var item = await _manifestService.AddShipmentToManifestAsync(
                id, request.AWBNumber, request.UserId);

            if (item == null)
                return BadRequest("Failed to add shipment. Check AWB number and manifest status.");

            return Ok(new ManifestItemDto
            {
                Id = item.Id,
                ShipmentId = item.ShipmentId,
                AWBNumber = item.AWBNumber,
                Pieces = item.Pieces,
                Weight = item.Weight,
                CODAmount = item.CODAmount,
                IsCOD = item.IsCOD,
                ScanTime = item.ScanTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding shipment to manifest {ManifestId}", id);
            return StatusCode(500, "An error occurred while adding shipment to the manifest.");
        }
    }

    [HttpDelete("{manifestId}/shipments/{shipmentId}")]
    public async Task<ActionResult> RemoveShipment(Guid manifestId, Guid shipmentId)
    {
        try
        {
            var manifest = await _manifestService.GetByIdAsync(manifestId);
            if (manifest == null)
                return NotFound();

            if (manifest.Status != ManifestStatus.Open)
                return BadRequest("Can only remove shipments from open manifests.");

            var success = await _manifestService.RemoveShipmentFromManifestAsync(manifestId, shipmentId);
            if (!success)
                return NotFound("Shipment not found in manifest.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing shipment {ShipmentId} from manifest {ManifestId}", 
                shipmentId, manifestId);
            return StatusCode(500, "An error occurred while removing the shipment.");
        }
    }

    [HttpPost("{id}/dispatch")]
    public async Task<ActionResult<ManifestDetailDto>> DispatchManifest(
        Guid id, 
        [FromBody] DispatchManifestRequest request)
    {
        try
        {
            var manifest = await _manifestService.GetByIdAsync(id);
            if (manifest == null)
                return NotFound();

            if (manifest.Status != ManifestStatus.Open)
                return BadRequest("Manifest must be Open to dispatch.");

            if (manifest.Items == null || manifest.Items.Count == 0)
                return BadRequest("Cannot dispatch an empty manifest.");

            var dispatchedManifest = await _manifestService.DispatchManifestAsync(
                id,
                request.SealNumber,
                request.DispatchedByUserId);

            if (dispatchedManifest == null)
                return BadRequest("Failed to dispatch manifest.");

            return Ok(MapToDetailDto(dispatchedManifest));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching manifest {ManifestId}", id);
            return StatusCode(500, "An error occurred while dispatching the manifest.");
        }
    }

    [HttpPost("{id}/receive")]
    public async Task<ActionResult<ManifestReceiptDto>> ReceiveManifest(
        Guid id, 
        [FromBody] ReceiveManifestDetailRequest request)
    {
        try
        {
            var manifest = await _manifestService.GetByIdAsync(id);
            if (manifest == null)
                return NotFound();

            if (manifest.Status != ManifestStatus.InTransit)
                return BadRequest("Manifest must be In Transit to receive.");

            var receivedManifest = await _manifestService.ReceiveManifestAsync(
                id,
                request.HubId,
                request.HubName,
                request.ReceivedByUserId,
                request.Remarks);

            if (receivedManifest == null)
                return BadRequest("Failed to receive manifest.");

            return Ok(new ManifestReceiptDto
            {
                ManifestId = receivedManifest.Id,
                ManifestNumber = receivedManifest.ManifestNumber,
                Status = receivedManifest.Status.ToString(),
                ReceivedAt = receivedManifest.ReceivedAt,
                ShipmentsReceived = receivedManifest.ShipmentsReceived ?? 0,
                ShortShipments = receivedManifest.ShortShipments ?? 0,
                DamagedShipments = receivedManifest.DamagedShipments ?? 0,
                ReceiptNotes = receivedManifest.ReceiptNotes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving manifest {ManifestId}", id);
            return StatusCode(500, "An error occurred while receiving the manifest.");
        }
    }

    [HttpGet("pending-dispatch")]
    public async Task<ActionResult<List<ManifestListDto>>> GetPendingDispatch([FromQuery] Guid hubId)
    {
        try
        {
            var manifests = await _manifestService.GetPendingDispatchAsync(hubId);
            return Ok(manifests.Select(MapToListDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending dispatch for hub {HubId}", hubId);
            return StatusCode(500, "An error occurred while fetching pending manifests.");
        }
    }

    [HttpGet("pending-receive")]
    public async Task<ActionResult<List<ManifestListDto>>> GetPendingReceive([FromQuery] Guid hubId)
    {
        try
        {
            var manifests = await _manifestService.GetPendingReceiveAsync(hubId);
            return Ok(manifests.Select(MapToListDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending receive for hub {HubId}", hubId);
            return StatusCode(500, "An error occurred while fetching pending manifests.");
        }
    }

    [HttpGet("in-transit")]
    public async Task<ActionResult<List<ManifestListDto>>> GetInTransit()
    {
        try
        {
            var manifests = await _manifestService.GetInTransitAsync();
            return Ok(manifests.Select(MapToListDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching in-transit manifests");
            return StatusCode(500, "An error occurred while fetching in-transit manifests.");
        }
    }

    private static ManifestListDto MapToListDto(Manifest manifest) => new()
    {
        Id = manifest.Id,
        ManifestNumber = manifest.ManifestNumber,
        Type = manifest.Type.ToString(),
        ManifestDate = manifest.ManifestDate,
        OriginHubId = manifest.OriginHubId,
        OriginHubName = manifest.OriginHubName,
        DestinationHubId = manifest.DestinationHubId,
        DestinationHubName = manifest.DestinationHubName,
        Status = manifest.Status.ToString(),
        TotalShipments = manifest.TotalShipments,
        TotalWeight = manifest.TotalWeight,
        TotalPieces = manifest.TotalPieces,
        TotalCODAmount = manifest.TotalCODAmount,
        VehicleNumber = manifest.VehicleNumber,
        DriverName = manifest.DriverName,
        DispatchedAt = manifest.DispatchedAt,
        ExpectedArrival = manifest.ExpectedArrival,
        ReceivedAt = manifest.ReceivedAt
    };

    private static ManifestDetailDto MapToDetailDto(Manifest manifest) => new()
    {
        Id = manifest.Id,
        ManifestNumber = manifest.ManifestNumber,
        Type = manifest.Type.ToString(),
        ManifestDate = manifest.ManifestDate,
        OriginHubId = manifest.OriginHubId,
        OriginHubName = manifest.OriginHubName,
        DestinationHubId = manifest.DestinationHubId,
        DestinationHubName = manifest.DestinationHubName,
        Status = manifest.Status.ToString(),
        SealNumber = manifest.SealNumber,
        VehicleNumber = manifest.VehicleNumber,
        DriverName = manifest.DriverName,
        DriverPhone = manifest.DriverPhone,
        CoLoaderName = manifest.CoLoaderName,
        TotalShipments = manifest.TotalShipments,
        TotalWeight = manifest.TotalWeight,
        TotalPieces = manifest.TotalPieces,
        TotalCODAmount = manifest.TotalCODAmount,
        TotalDeclaredValue = manifest.TotalDeclaredValue,
        DispatchedAt = manifest.DispatchedAt,
        ExpectedArrival = manifest.ExpectedArrival,
        ReceivedAt = manifest.ReceivedAt,
        ShipmentsReceived = manifest.ShipmentsReceived,
        ShortShipments = manifest.ShortShipments,
        DamagedShipments = manifest.DamagedShipments,
        DispatchNotes = manifest.DispatchNotes,
        ReceiptNotes = manifest.ReceiptNotes,
        DiscrepancyNotes = manifest.DiscrepancyNotes,
        Items = manifest.Items?.Select(i => new ManifestItemDto
        {
            Id = i.Id,
            ShipmentId = i.ShipmentId,
            AWBNumber = i.AWBNumber,
            Pieces = i.Pieces,
            Weight = i.Weight,
            CODAmount = i.CODAmount,
            DestinationCity = i.DestinationCity,
            IsCOD = i.IsCOD,
            ScanTime = i.ScanTime,
            IsReceived = i.IsReceived,
            ReceivedAt = i.ReceivedAt
        }).ToList() ?? new()
    };
}

public class ManifestListDto
{
    public Guid Id { get; set; }
    public string ManifestNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime ManifestDate { get; set; }
    public Guid OriginHubId { get; set; }
    public string OriginHubName { get; set; } = string.Empty;
    public Guid DestinationHubId { get; set; }
    public string DestinationHubName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalShipments { get; set; }
    public decimal TotalWeight { get; set; }
    public int TotalPieces { get; set; }
    public decimal TotalCODAmount { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ExpectedArrival { get; set; }
    public DateTime? ReceivedAt { get; set; }
}

public class ManifestDetailDto
{
    public Guid Id { get; set; }
    public string ManifestNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime ManifestDate { get; set; }
    public Guid OriginHubId { get; set; }
    public string OriginHubName { get; set; } = string.Empty;
    public Guid DestinationHubId { get; set; }
    public string DestinationHubName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SealNumber { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? CoLoaderName { get; set; }
    public int TotalShipments { get; set; }
    public decimal TotalWeight { get; set; }
    public int TotalPieces { get; set; }
    public decimal TotalCODAmount { get; set; }
    public decimal TotalDeclaredValue { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ExpectedArrival { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public int? ShipmentsReceived { get; set; }
    public int? ShortShipments { get; set; }
    public int? DamagedShipments { get; set; }
    public string? DispatchNotes { get; set; }
    public string? ReceiptNotes { get; set; }
    public string? DiscrepancyNotes { get; set; }
    public List<ManifestItemDto> Items { get; set; } = new();
}

public class ManifestItemDto
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public int Pieces { get; set; }
    public decimal Weight { get; set; }
    public decimal CODAmount { get; set; }
    public string? DestinationCity { get; set; }
    public bool IsCOD { get; set; }
    public DateTime? ScanTime { get; set; }
    public bool IsReceived { get; set; }
    public DateTime? ReceivedAt { get; set; }
}

public class ManifestReceiptDto
{
    public Guid ManifestId { get; set; }
    public string ManifestNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ReceivedAt { get; set; }
    public int ShipmentsReceived { get; set; }
    public int ShortShipments { get; set; }
    public int DamagedShipments { get; set; }
    public string? ReceiptNotes { get; set; }
}

public class CreateManifestRequest
{
    public Guid OriginHubId { get; set; }
    public string OriginHubName { get; set; } = string.Empty;
    public Guid DestinationHubId { get; set; }
    public string DestinationHubName { get; set; } = string.Empty;
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
}

public class AddShipmentToManifestRequest
{
    public string AWBNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
}

public class DispatchManifestRequest
{
    public Guid? DispatchedByUserId { get; set; }
    public string? SealNumber { get; set; }
}

public class ReceiveManifestDetailRequest
{
    public Guid HubId { get; set; }
    public string HubName { get; set; } = string.Empty;
    public Guid? ReceivedByUserId { get; set; }
    public string? Remarks { get; set; }
}
