using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/drs")]
[Authorize]
public class DRSController : ControllerBase
{
    private readonly IDRSService _drsService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<DRSController> _logger;

    public DRSController(
        IDRSService drsService,
        ITenantProvider tenantProvider,
        ILogger<DRSController> logger)
    {
        _drsService = drsService;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<DRSListDto>>> GetDeliveryRunSheets(
        [FromQuery] DateTime? date,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? status,
        [FromQuery] Guid? deliveryAgentId,
        [FromQuery] string? search)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            DRSStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<DRSStatus>(status, out var s))
                statusEnum = s;

            var sheets = await _drsService.GetAllAsync(date, statusEnum);

            if (fromDate.HasValue)
                sheets = sheets.Where(d => d.DrsDate.Date >= fromDate.Value.Date).ToList();
            if (toDate.HasValue)
                sheets = sheets.Where(d => d.DrsDate.Date <= toDate.Value.Date).ToList();
            if (deliveryAgentId.HasValue)
                sheets = sheets.Where(d => d.AgentId == deliveryAgentId.Value).ToList();
            if (!string.IsNullOrEmpty(search))
            {
                var searchTerm = search.Trim();
                sheets = sheets.Where(d => 
                    d.DRSNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (d.Agent != null && d.Agent.Name != null && d.Agent.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            return Ok(sheets.Select(MapToListDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching delivery run sheets");
            return StatusCode(500, "An error occurred while fetching delivery run sheets.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DRSDetailDto>> GetDeliveryRunSheet(Guid id)
    {
        try
        {
            var drs = await _drsService.GetByIdAsync(id);
            if (drs == null)
                return NotFound();

            return Ok(MapToDetailDto(drs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching DRS {DRSId}", id);
            return StatusCode(500, "An error occurred while fetching the delivery run sheet.");
        }
    }

    [HttpGet("number/{drsNumber}")]
    public async Task<ActionResult<DRSDetailDto>> GetDRSByNumber(string drsNumber)
    {
        try
        {
            var drs = await _drsService.GetByNumberAsync(drsNumber);
            if (drs == null)
                return NotFound();

            return Ok(MapToDetailDto(drs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching DRS by number {DRSNumber}", drsNumber);
            return StatusCode(500, "An error occurred while fetching the delivery run sheet.");
        }
    }

    [HttpPost("generate")]
    public async Task<ActionResult<DRSDetailDto>> GenerateDRS([FromBody] GenerateDRSRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            var drs = await _drsService.GenerateDrsAsync(
                request.HubId,
                request.HubName,
                request.DriverId,
                request.DriverName,
                request.VehicleNumber,
                request.RouteZoneId);

            return CreatedAtAction(nameof(GetDeliveryRunSheet), new { id = drs.Id }, MapToDetailDto(drs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating DRS");
            return StatusCode(500, "An error occurred while generating the delivery run sheet.");
        }
    }

    [HttpPost("{id}/shipments")]
    public async Task<ActionResult<DRSItemDto>> AddShipment(Guid id, [FromBody] AddShipmentToDrsRequest request)
    {
        try
        {
            var drs = await _drsService.GetByIdAsync(id);
            if (drs == null)
                return NotFound();

            if (drs.Status != DRSStatus.Draft && drs.Status != DRSStatus.Open)
                return BadRequest("Can only add shipments to Draft or Open DRS.");

            var item = await _drsService.AddShipmentToDrsAsync(id, request.AWBNumber, request.SequenceNumber);

            if (item == null)
                return BadRequest("Failed to add shipment. Check AWB number.");

            return Ok(MapToItemDto(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding shipment to DRS {DRSId}", id);
            return StatusCode(500, "An error occurred while adding shipment to DRS.");
        }
    }

    [HttpDelete("{drsId}/shipments/{shipmentId}")]
    public async Task<ActionResult> RemoveShipment(Guid drsId, Guid shipmentId)
    {
        try
        {
            var drs = await _drsService.GetByIdAsync(drsId);
            if (drs == null)
                return NotFound();

            if (drs.Status != DRSStatus.Draft && drs.Status != DRSStatus.Open)
                return BadRequest("Can only remove shipments from Draft or Open DRS.");

            var success = await _drsService.RemoveShipmentFromDrsAsync(drsId, shipmentId);
            if (!success)
                return NotFound("Shipment not found in DRS.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing shipment {ShipmentId} from DRS {DRSId}", 
                shipmentId, drsId);
            return StatusCode(500, "An error occurred while removing the shipment.");
        }
    }

    [HttpPost("{id}/dispatch")]
    public async Task<ActionResult<DRSDetailDto>> DispatchDRS(Guid id, [FromBody] DispatchDrsRequest? request = null)
    {
        try
        {
            var drs = await _drsService.GetByIdAsync(id);
            if (drs == null)
                return NotFound();

            if (drs.Status != DRSStatus.Draft && drs.Status != DRSStatus.Open)
                return BadRequest("DRS must be in Draft or Open status to dispatch.");

            var dispatchedDrs = await _drsService.DispatchDrsAsync(id, request?.UserId);
            if (dispatchedDrs == null)
                return BadRequest("Failed to dispatch DRS.");

            return Ok(MapToDetailDto(dispatchedDrs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching DRS {DRSId}", id);
            return StatusCode(500, "An error occurred while dispatching the DRS.");
        }
    }

    [HttpPut("items/{itemId}/status")]
    public async Task<ActionResult<DRSItemDto>> UpdateDeliveryStatus(
        Guid itemId, 
        [FromBody] UpdateDeliveryStatusRequest request)
    {
        try
        {
            if (!Enum.TryParse<DRSItemStatus>(request.Status, out var status))
                return BadRequest("Invalid status.");

            var updatedItem = await _drsService.UpdateDeliveryStatusAsync(
                itemId,
                status,
                request.Remarks,
                request.PODImageUrl,
                request.SignatureImageUrl,
                request.FreightCollected);

            if (updatedItem == null)
                return NotFound("DRS item not found.");

            return Ok(MapToItemDto(updatedItem));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating delivery status for item {ItemId}", itemId);
            return StatusCode(500, "An error occurred while updating the delivery status.");
        }
    }

    [HttpPost("{id}/reconcile")]
    public async Task<ActionResult<DRSDetailDto>> ReconcileDRS(
        Guid id, 
        [FromBody] ReconcileDRSRequest request)
    {
        try
        {
            var drs = await _drsService.GetByIdAsync(id);
            if (drs == null)
                return NotFound();

            if (drs.Status != DRSStatus.Completed && drs.Status != DRSStatus.InProgress)
                return BadRequest("DRS must be In Progress or Completed to reconcile.");

            var reconciledDrs = await _drsService.ReconcileDrsAsync(
                id,
                request.CashDeposited,
                request.DriverExpenses,
                request.ReconciliationNotes,
                request.ReconciledByUserId);

            if (reconciledDrs == null)
                return BadRequest("Failed to reconcile DRS.");

            return Ok(MapToDetailDto(reconciledDrs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling DRS {DRSId}", id);
            return StatusCode(500, "An error occurred while reconciling the DRS.");
        }
    }

    [HttpGet("{id}/cod-expected")]
    public async Task<ActionResult<decimal>> GetCODExpected(Guid id)
    {
        try
        {
            var expected = await _drsService.CalculateCodExpectedAsync(id);
            return Ok(expected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating COD expected for DRS {DRSId}", id);
            return StatusCode(500, "An error occurred while calculating COD.");
        }
    }

    [HttpGet("{id}/summary")]
    public async Task<ActionResult<DRSSummaryDto>> GetDRSSummary(Guid id)
    {
        try
        {
            var summary = await _drsService.GetDrsSummaryAsync(id);

            return Ok(new DRSSummaryDto
            {
                DRSId = summary.DrsId,
                DRSNumber = summary.DrsNumber,
                TotalShipments = summary.TotalShipments,
                DeliveredCount = summary.DeliveredCount,
                PendingCount = summary.PendingCount,
                UndeliveredCount = summary.UndeliveredCount,
                RescheduledCount = summary.RescheduledCount,
                TotalCODExpected = summary.TotalCODExpected,
                TotalCODCollected = summary.TotalCODCollected,
                TotalFreightCollected = summary.TotalFreightCollected,
                DeliveryRate = summary.DeliveryRate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summary for DRS {DRSId}", id);
            return StatusCode(500, "An error occurred while getting DRS summary.");
        }
    }

    [HttpGet("pending-reconciliation")]
    public async Task<ActionResult<List<DRSListDto>>> GetPendingReconciliation()
    {
        try
        {
            var sheets = await _drsService.GetPendingReconciliationAsync();
            return Ok(sheets.Select(MapToListDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending reconciliation DRS");
            return StatusCode(500, "An error occurred while fetching pending reconciliation.");
        }
    }

    private static DRSListDto MapToListDto(DeliveryRunSheet drs) => new()
    {
        Id = drs.Id,
        DRSNumber = drs.DRSNumber,
        DRSDate = drs.DRSDate,
        AgentId = drs.AgentId,
        AgentName = drs.Agent?.Name,
        HubId = drs.HubId,
        HubName = drs.HubName,
        DriverName = drs.DriverName,
        Status = drs.Status.ToString(),
        TotalShipments = drs.TotalShipments,
        DeliveredCount = drs.DeliveredCount,
        FailedCount = drs.FailedCount,
        PendingCount = drs.PendingCount,
        TotalCODExpected = drs.TotalCODExpected,
        TotalCODCollected = drs.TotalCODCollected,
        VehicleNumber = drs.VehicleNumber,
        DispatchTime = drs.DispatchTime,
        ReturnTime = drs.ReturnTime,
        IsReconciled = drs.IsReconciled
    };

    private static DRSDetailDto MapToDetailDto(DeliveryRunSheet drs) => new()
    {
        Id = drs.Id,
        DRSNumber = drs.DRSNumber,
        DRSDate = drs.DRSDate,
        AgentId = drs.AgentId,
        AgentName = drs.Agent?.Name,
        DriverId = drs.DriverId,
        DriverName = drs.DriverName,
        HubId = drs.HubId,
        HubName = drs.HubName,
        Status = drs.Status.ToString(),
        TotalShipments = drs.TotalShipments,
        DeliveredCount = drs.DeliveredCount,
        FailedCount = drs.FailedCount,
        PendingCount = drs.PendingCount,
        TotalCODExpected = drs.TotalCODExpected,
        TotalCODCollected = drs.TotalCODCollected,
        TotalFreightToCollect = drs.TotalFreightToCollect,
        FreightCollected = drs.FreightCollected,
        TotalCashDeposited = drs.TotalCashDeposited,
        DriverExpenses = drs.DriverExpenses,
        ShortageAmount = drs.ShortageAmount,
        VehicleNumber = drs.VehicleNumber,
        RouteZoneId = drs.RouteZoneId,
        RouteZoneName = drs.RouteZone?.ZoneName,
        StartTime = drs.StartTime,
        DispatchTime = drs.DispatchTime,
        ReturnTime = drs.ReturnTime,
        EndTime = drs.EndTime,
        IsReconciled = drs.IsReconciled,
        ReconciledAt = drs.ReconciledAt,
        ReconciliationNotes = drs.ReconciliationNotes,
        Notes = drs.Notes,
        ExpenseNotes = drs.ExpenseNotes,
        Items = drs.Items?.Select(MapToItemDto).ToList() ?? new()
    };

    private static DRSItemDto MapToItemDto(DRSItem item) => new()
    {
        Id = item.Id,
        ShipmentId = item.ShipmentId,
        Sequence = item.Sequence,
        ReceiverName = item.ReceiverName,
        ReceiverAddress = item.ReceiverAddress,
        ReceiverPhone = item.ReceiverPhone,
        CODAmount = item.CODAmount,
        FreightAmount = item.FreightAmount,
        Status = item.Status.ToString(),
        CODCollected = item.CODCollected ?? 0,
        FreightCollected = item.FreightCollected,
        ReceivedBy = item.ReceivedBy,
        Relationship = item.Relationship,
        DeliveryTime = item.DeliveryTime,
        FailureReason = item.FailureReason,
        Remarks = item.Remarks
    };
}

public class DRSListDto
{
    public Guid Id { get; set; }
    public string DRSNumber { get; set; } = string.Empty;
    public DateTime DRSDate { get; set; }
    public Guid? AgentId { get; set; }
    public string? AgentName { get; set; }
    public Guid? HubId { get; set; }
    public string? HubName { get; set; }
    public string? DriverName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalShipments { get; set; }
    public int DeliveredCount { get; set; }
    public int FailedCount { get; set; }
    public int PendingCount { get; set; }
    public decimal TotalCODExpected { get; set; }
    public decimal TotalCODCollected { get; set; }
    public string? VehicleNumber { get; set; }
    public DateTime? DispatchTime { get; set; }
    public DateTime? ReturnTime { get; set; }
    public bool IsReconciled { get; set; }
}

public class DRSDetailDto
{
    public Guid Id { get; set; }
    public string DRSNumber { get; set; } = string.Empty;
    public DateTime DRSDate { get; set; }
    public Guid? AgentId { get; set; }
    public string? AgentName { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public Guid? HubId { get; set; }
    public string? HubName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalShipments { get; set; }
    public int DeliveredCount { get; set; }
    public int FailedCount { get; set; }
    public int PendingCount { get; set; }
    public decimal TotalCODExpected { get; set; }
    public decimal TotalCODCollected { get; set; }
    public decimal TotalFreightToCollect { get; set; }
    public decimal FreightCollected { get; set; }
    public decimal TotalCashDeposited { get; set; }
    public decimal DriverExpenses { get; set; }
    public decimal ShortageAmount { get; set; }
    public string? VehicleNumber { get; set; }
    public Guid? RouteZoneId { get; set; }
    public string? RouteZoneName { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? DispatchTime { get; set; }
    public DateTime? ReturnTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public string? ReconciliationNotes { get; set; }
    public string? Notes { get; set; }
    public string? ExpenseNotes { get; set; }
    public List<DRSItemDto> Items { get; set; } = new();
}

public class DRSItemDto
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public int Sequence { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverAddress { get; set; }
    public string? ReceiverPhone { get; set; }
    public decimal CODAmount { get; set; }
    public decimal FreightAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal CODCollected { get; set; }
    public decimal FreightCollected { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Relationship { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public string? FailureReason { get; set; }
    public string? Remarks { get; set; }
}

public class DRSSummaryDto
{
    public Guid DRSId { get; set; }
    public string DRSNumber { get; set; } = string.Empty;
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

public class GenerateDRSRequest
{
    public Guid HubId { get; set; }
    public string HubName { get; set; } = string.Empty;
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string? VehicleNumber { get; set; }
    public Guid? RouteZoneId { get; set; }
}

public class AddShipmentToDrsRequest
{
    public string AWBNumber { get; set; } = string.Empty;
    public int? SequenceNumber { get; set; }
}

public class DispatchDrsRequest
{
    public Guid? UserId { get; set; }
}

public class UpdateDeliveryStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? PODImageUrl { get; set; }
    public string? SignatureImageUrl { get; set; }
    public decimal? FreightCollected { get; set; }
}

public class ReconcileDRSRequest
{
    public Guid? ReconciledByUserId { get; set; }
    public decimal CashDeposited { get; set; }
    public decimal DriverExpenses { get; set; }
    public string? ReconciliationNotes { get; set; }
}
