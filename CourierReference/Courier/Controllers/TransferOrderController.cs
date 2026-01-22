using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;
using TransferStatus = Server.Modules.Courier.Models.TransferStatus;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransferOrderController : ControllerBase
{
    private readonly ITransferOrderService _transferService;

    public TransferOrderController(ITransferOrderService transferService)
    {
        _transferService = transferService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TransferOrderDto>>> GetAll(
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        TransferType? transferType = null;
        TransferStatus? transferStatus = null;

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<TransferType>(type, true, out var parsedType))
            transferType = parsedType;

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TransferStatus>(status, true, out var parsedStatus))
            transferStatus = parsedStatus;

        var transfers = await _transferService.GetAllTransfersAsync(transferType, transferStatus);
        return Ok(transfers.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransferOrderDto>> GetById(Guid id)
    {
        var transfer = await _transferService.GetTransferByIdAsync(id);
        if (transfer == null) return NotFound();
        return Ok(MapToDto(transfer));
    }

    [HttpGet("by-number/{transferNumber}")]
    public async Task<ActionResult<TransferOrderDto>> GetByNumber(string transferNumber)
    {
        var transfer = await _transferService.GetTransferByNumberAsync(transferNumber);
        if (transfer == null) return NotFound();
        return Ok(MapToDto(transfer));
    }

    [HttpPost]
    public async Task<ActionResult<TransferOrderDto>> Create([FromBody] CreateTransferOrderDto dto)
    {
        var transferType = TransferType.DeliveryOutscan;
        if (!string.IsNullOrEmpty(dto.TransferType) && Enum.TryParse<TransferType>(dto.TransferType, true, out var parsedType))
            transferType = parsedType;

        var transfer = new TransferOrder
        {
            TransferType = transferType,
            SourceWarehouseId = dto.SourceWarehouseId,
            DestinationWarehouseId = dto.DestinationWarehouseId,
            SourceCourierId = dto.SourceCourierId,
            DestinationCourierId = dto.DestinationCourierId,
            VehicleId = dto.VehicleId,
            ScheduledAt = dto.ScheduledAt,
            Remarks = dto.Remarks,
            CreatedBy = User.Identity?.Name
        };

        var result = await _transferService.CreateTransferAsync(transfer);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, MapToDto(result));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TransferOrderDto>> Update(Guid id, [FromBody] UpdateTransferOrderDto dto)
    {
        var existing = await _transferService.GetTransferByIdAsync(id);
        if (existing == null) return NotFound();

        if (existing.Status != TransferStatus.Draft)
            return BadRequest("Cannot update a transfer that is not in draft status");

        existing.SourceWarehouseId = dto.SourceWarehouseId;
        existing.DestinationWarehouseId = dto.DestinationWarehouseId;
        existing.SourceCourierId = dto.SourceCourierId;
        existing.DestinationCourierId = dto.DestinationCourierId;
        existing.VehicleId = dto.VehicleId;
        existing.ScheduledAt = dto.ScheduledAt;
        existing.Remarks = dto.Remarks;
        existing.UpdatedBy = User.Identity?.Name;

        var result = await _transferService.UpdateTransferAsync(existing);
        return Ok(MapToDto(result));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _transferService.DeleteTransferAsync(id);
        if (!success) return BadRequest("Cannot delete transfer or transfer not found");
        return NoContent();
    }

    [HttpPost("{id}/items")]
    public async Task<ActionResult<TransferOrderItemDto>> AddItem(Guid id, [FromBody] AddTransferItemDto dto)
    {
        try
        {
            var item = await _transferService.AddItemToTransferAsync(id, dto.ShipmentId);
            return Ok(MapItemToDto(item));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}/items/{itemId}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
    {
        var success = await _transferService.RemoveItemFromTransferAsync(id, itemId);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/scan")]
    public async Task<ActionResult<TransferOrderItemDto>> ScanItem(Guid id, [FromBody] ScanItemDto dto)
    {
        var item = await _transferService.ScanItemAsync(id, dto.AWBNumber, User.Identity?.Name ?? "System");
        if (item == null) return NotFound("AWB not found or transfer not found");
        return Ok(MapItemToDto(item));
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult<TransferOrderDto>> StartTransfer(Guid id)
    {
        try
        {
            var result = await _transferService.StartTransferAsync(id, User.Identity?.Name ?? "System");
            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<TransferOrderDto>> CompleteTransfer(Guid id)
    {
        try
        {
            var result = await _transferService.CompleteTransferAsync(id, User.Identity?.Name ?? "System");
            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<TransferOrderDto>> RejectTransfer(Guid id, [FromBody] RejectTransferDto dto)
    {
        try
        {
            var result = await _transferService.RejectTransferAsync(id, User.Identity?.Name ?? "System", dto.Reason);
            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/events")]
    public async Task<ActionResult<List<TransferScanEventDto>>> GetScanEvents(Guid id)
    {
        var events = await _transferService.GetScanEventsAsync(id);
        return Ok(events.Select(MapEventToDto));
    }

    [HttpGet("generate-number")]
    public async Task<ActionResult<string>> GenerateTransferNumber([FromQuery] string type = "DeliveryOutscan")
    {
        var transferType = TransferType.DeliveryOutscan;
        if (Enum.TryParse<TransferType>(type, true, out var parsedType))
            transferType = parsedType;

        var number = await _transferService.GenerateTransferNumberAsync(transferType);
        return Ok(new { transferNumber = number });
    }

    private static TransferOrderDto MapToDto(TransferOrder t) => new()
    {
        Id = t.Id,
        TransferNumber = t.TransferNumber,
        TransferType = t.TransferType.ToString(),
        Status = t.Status.ToString(),
        SourceWarehouseId = t.SourceWarehouseId,
        DestinationWarehouseId = t.DestinationWarehouseId,
        SourceCourierId = t.SourceCourierId,
        DestinationCourierId = t.DestinationCourierId,
        VehicleId = t.VehicleId,
        ScheduledAt = t.ScheduledAt,
        ExecutedAt = t.ExecutedAt,
        CompletedAt = t.CompletedAt,
        Remarks = t.Remarks,
        TotalItems = t.TotalItems,
        ScannedItems = t.ScannedItems,
        ExceptionItems = t.ExceptionItems,
        CreatedAt = t.CreatedAt,
        CreatedBy = t.CreatedBy,
        Items = t.Items?.Select(MapItemToDto).ToList() ?? new()
    };

    private static TransferOrderItemDto MapItemToDto(TransferOrderItem i) => new()
    {
        Id = i.Id,
        ShipmentId = i.ShipmentId,
        AWBNumber = i.AWBNumber,
        Status = i.Status.ToString(),
        ScannedAt = i.ScannedAt,
        LoadedAt = i.LoadedAt,
        ReceivedAt = i.ReceivedAt,
        ExceptionReason = i.ExceptionReason,
        ScannedBy = i.ScannedBy,
        ReceivedBy = i.ReceivedBy
    };

    private static TransferScanEventDto MapEventToDto(TransferScanEvent e) => new()
    {
        Id = e.Id,
        ScanType = e.ScanType.ToString(),
        AWBNumber = e.AWBNumber,
        ScanTimestamp = e.ScanTimestamp,
        ScannedByName = e.ScannedByName,
        ScanLocationName = e.ScanLocationName,
        DeviceName = e.DeviceName,
        Notes = e.Notes,
        IsException = e.IsException,
        ExceptionDetails = e.ExceptionDetails
    };
}

public class TransferOrderDto
{
    public Guid Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public string TransferType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? SourceWarehouseId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public Guid? SourceCourierId { get; set; }
    public Guid? DestinationCourierId { get; set; }
    public Guid? VehicleId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Remarks { get; set; }
    public int TotalItems { get; set; }
    public int ScannedItems { get; set; }
    public int ExceptionItems { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public List<TransferOrderItemDto> Items { get; set; } = new();
}

public class TransferOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ScannedAt { get; set; }
    public DateTime? LoadedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? ExceptionReason { get; set; }
    public string? ScannedBy { get; set; }
    public string? ReceivedBy { get; set; }
}

public class TransferScanEventDto
{
    public Guid Id { get; set; }
    public string ScanType { get; set; } = string.Empty;
    public string? AWBNumber { get; set; }
    public DateTime ScanTimestamp { get; set; }
    public string? ScannedByName { get; set; }
    public string? ScanLocationName { get; set; }
    public string? DeviceName { get; set; }
    public string? Notes { get; set; }
    public bool IsException { get; set; }
    public string? ExceptionDetails { get; set; }
}

public class CreateTransferOrderDto
{
    public string TransferType { get; set; } = "DeliveryOutscan";
    public Guid? SourceWarehouseId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public Guid? SourceCourierId { get; set; }
    public Guid? DestinationCourierId { get; set; }
    public Guid? VehicleId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateTransferOrderDto
{
    public Guid? SourceWarehouseId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public Guid? SourceCourierId { get; set; }
    public Guid? DestinationCourierId { get; set; }
    public Guid? VehicleId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? Remarks { get; set; }
}

public class AddTransferItemDto
{
    public Guid ShipmentId { get; set; }
}

public class ScanItemDto
{
    public string AWBNumber { get; set; } = string.Empty;
}

public class RejectTransferDto
{
    public string Reason { get; set; } = string.Empty;
}
