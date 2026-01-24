using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/cod-remittances")]
[Authorize]
public class CODRemittanceController : ControllerBase
{
    private readonly ICODRemittanceService _codRemittanceService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CODRemittanceController> _logger;

    public CODRemittanceController(
        ICODRemittanceService codRemittanceService,
        ITenantProvider tenantProvider,
        ILogger<CODRemittanceController> logger)
    {
        _codRemittanceService = codRemittanceService;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<CODRemittanceListDto>>> GetRemittances(
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

            RemittanceStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RemittanceStatus>(status, out var s))
                statusEnum = s;

            var remittances = await _codRemittanceService.GetAllAsync(fromDate, toDate, statusEnum);
            
            // Apply search filter on remittance number
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim().ToUpperInvariant();
                remittances = remittances.Where(r => 
                    (!string.IsNullOrEmpty(r.RemittanceNumber) && r.RemittanceNumber.ToUpperInvariant().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.CustomerName) && r.CustomerName.ToUpperInvariant().Contains(searchTerm))
                ).ToList();
            }

            return Ok(remittances.Select(MapToListDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching COD remittances");
            return StatusCode(500, "An error occurred while fetching COD remittances.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CODRemittanceDetailDto>> GetRemittance(Guid id)
    {
        try
        {
            var remittance = await _codRemittanceService.GetByIdAsync(id);
            if (remittance == null)
                return NotFound();

            return Ok(MapToDetailDto(remittance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching COD remittance {RemittanceId}", id);
            return StatusCode(500, "An error occurred while fetching the COD remittance.");
        }
    }

    [HttpGet("number/{remittanceNumber}")]
    public async Task<ActionResult<CODRemittanceDetailDto>> GetRemittanceByNumber(string remittanceNumber)
    {
        try
        {
            var remittance = await _codRemittanceService.GetByNumberAsync(remittanceNumber);
            if (remittance == null)
                return NotFound();

            return Ok(MapToDetailDto(remittance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching COD remittance by number {RemittanceNumber}", remittanceNumber);
            return StatusCode(500, "An error occurred while fetching the COD remittance.");
        }
    }

    [HttpGet("pending-by-customer")]
    public async Task<ActionResult<List<CustomerCODSummaryDto>>> GetPendingByCustomer()
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            var pending = await _codRemittanceService.GetPendingRemittancesByCustomerAsync();

            return Ok(pending.Select(p => new CustomerCODSummaryDto
            {
                CustomerId = p.CustomerId,
                CustomerName = p.CustomerName,
                TotalCODAmount = p.TotalCODAmount,
                TotalShipments = p.TotalShipments,
                OldestDeliveryDate = p.OldestDeliveryDate
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending COD by customer");
            return StatusCode(500, "An error occurred while fetching pending remittances.");
        }
    }

    [HttpGet("customer/{customerId}/pending-shipments")]
    public async Task<ActionResult<List<PendingCODShipmentDto>>> GetPendingShipments(Guid customerId)
    {
        try
        {
            var shipments = await _codRemittanceService.GetPendingCODShipmentsAsync(customerId);

            return Ok(shipments.Select(s => new PendingCODShipmentDto
            {
                ShipmentId = s.Id,
                AWBNumber = s.AWBNumber,
                DeliveryDate = s.ActualDeliveryDate,
                CODAmount = s.CODAmount,
                ReceiverName = s.ReceiverName,
                ReceiverCity = s.ReceiverCity
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending COD shipments for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while fetching pending shipments.");
        }
    }

    [HttpGet("customer/{customerId}/pending-amount")]
    public async Task<ActionResult<decimal>> GetPendingAmount(Guid customerId)
    {
        try
        {
            var amount = await _codRemittanceService.CalculatePendingCODAsync(customerId);
            return Ok(amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating pending COD for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while calculating pending COD.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CODRemittanceDetailDto>> CreateRemittance([FromBody] CreateRemittanceRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            if (request.ShipmentIds == null || request.ShipmentIds.Count == 0)
                return BadRequest("At least one shipment is required.");

            var remittance = await _codRemittanceService.CreateRemittanceAsync(
                request.CustomerId,
                request.ShipmentIds,
                request.Remarks);

            return CreatedAtAction(nameof(GetRemittance), new { id = remittance.Id }, MapToDetailDto(remittance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating COD remittance");
            return StatusCode(500, "An error occurred while creating the COD remittance.");
        }
    }

    [HttpPost("{id}/process-payment")]
    public async Task<ActionResult<CODRemittanceDetailDto>> ProcessPayment(
        Guid id, 
        [FromBody] ProcessPaymentRequest request)
    {
        try
        {
            var remittance = await _codRemittanceService.GetByIdAsync(id);
            if (remittance == null)
                return NotFound();

            if (remittance.Status == RemittanceStatus.Paid)
                return BadRequest("Remittance is already paid.");

            if (remittance.Status == RemittanceStatus.Cancelled)
                return BadRequest("Cannot process payment for cancelled remittance.");

            var processedRemittance = await _codRemittanceService.ProcessPaymentAsync(
                id,
                request.PaymentReference,
                request.PaymentMethod,
                request.PaymentDate,
                request.ProcessedByUserId);

            if (processedRemittance == null)
                return BadRequest("Failed to process payment.");

            return Ok(MapToDetailDto(processedRemittance));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for COD remittance {RemittanceId}", id);
            return StatusCode(500, "An error occurred while processing the payment.");
        }
    }

    [HttpPost("{id}/void")]
    public async Task<ActionResult> VoidRemittance(Guid id, [FromBody] VoidRemittanceRequest request)
    {
        try
        {
            var remittance = await _codRemittanceService.GetByIdAsync(id);
            if (remittance == null)
                return NotFound();

            if (remittance.Status == RemittanceStatus.Paid)
                return BadRequest("Cannot void a paid remittance.");

            var voidedRemittance = await _codRemittanceService.VoidRemittanceAsync(id, request.Reason, request.UserId);
            if (voidedRemittance == null)
                return BadRequest("Unable to void the remittance.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding COD remittance {RemittanceId}", id);
            return StatusCode(500, "An error occurred while voiding the remittance.");
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<CODRemittanceSummaryDto>> GetSummary(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var summary = await _codRemittanceService.GetRemittanceSummaryAsync(fromDate, toDate);

            return Ok(new CODRemittanceSummaryDto
            {
                TotalRemittances = summary.TotalRemittances,
                PendingRemittances = summary.PendingRemittances,
                PaidRemittances = summary.PaidRemittances,
                TotalCODAmount = summary.TotalCODAmount,
                TotalPaidAmount = summary.TotalPaidAmount,
                TotalPendingCOD = summary.TotalPendingCOD
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating COD remittance summary");
            return StatusCode(500, "An error occurred while generating the summary.");
        }
    }

    private static CODRemittanceListDto MapToListDto(CODRemittance remittance) => new()
    {
        Id = remittance.Id,
        RemittanceNumber = remittance.RemittanceNumber,
        RemittanceDate = remittance.RemittanceDate,
        CustomerId = remittance.CustomerId,
        CustomerName = remittance.CustomerName,
        Status = remittance.Status.ToString(),
        TotalCODAmount = remittance.TotalCODAmount,
        NetPayableAmount = remittance.NetPayableAmount,
        PaidAmount = remittance.PaidAmount,
        BalanceAmount = remittance.BalanceAmount,
        ShipmentCount = remittance.ShipmentCount,
        ProcessedAt = remittance.Status == RemittanceStatus.Paid ? remittance.UpdatedAt : null
    };

    private static CODRemittanceDetailDto MapToDetailDto(CODRemittance remittance) => new()
    {
        Id = remittance.Id,
        RemittanceNumber = remittance.RemittanceNumber,
        RemittanceDate = remittance.RemittanceDate,
        CustomerId = remittance.CustomerId,
        CustomerName = remittance.CustomerName,
        Status = remittance.Status.ToString(),
        TotalCODAmount = remittance.TotalCODAmount,
        TotalMaterialCost = remittance.TotalMaterialCost,
        DeductionAmount = remittance.DeductionAmount,
        DeductionReason = remittance.DeductionReason,
        NetPayableAmount = remittance.NetPayableAmount,
        PaidAmount = remittance.PaidAmount,
        BalanceAmount = remittance.BalanceAmount,
        PaymentMethod = remittance.PaymentMethod,
        PaymentReference = remittance.PaymentReference,
        DueDate = remittance.DueDate,
        Items = remittance.Items?.Select(i => new CODRemittanceItemDto
        {
            Id = i.Id,
            ShipmentId = i.ShipmentId,
            AWBNumber = i.AWBNumber,
            DeliveryDate = i.DeliveryDate,
            CODAmount = i.CODAmount,
            MaterialCostAmount = i.MaterialCostAmount,
            NetAmount = i.NetAmount,
            ReceiverName = i.Shipment?.ReceiverName,
            ReceiverCity = i.Shipment?.ReceiverCity
        }).ToList() ?? new()
    };
}

public class CODRemittanceListDto
{
    public Guid Id { get; set; }
    public string RemittanceNumber { get; set; } = string.Empty;
    public DateTime RemittanceDate { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalCODAmount { get; set; }
    public decimal NetPayableAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public int ShipmentCount { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class CODRemittanceDetailDto
{
    public Guid Id { get; set; }
    public string RemittanceNumber { get; set; } = string.Empty;
    public DateTime RemittanceDate { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalCODAmount { get; set; }
    public decimal TotalMaterialCost { get; set; }
    public decimal DeductionAmount { get; set; }
    public string? DeductionReason { get; set; }
    public decimal NetPayableAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime? DueDate { get; set; }
    public List<CODRemittanceItemDto> Items { get; set; } = new();
}

public class CODRemittanceItemDto
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public decimal CODAmount { get; set; }
    public decimal MaterialCostAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverCity { get; set; }
}

public class CustomerCODSummaryDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalCODAmount { get; set; }
    public int TotalShipments { get; set; }
    public DateTime? OldestDeliveryDate { get; set; }
}

public class PendingCODShipmentDto
{
    public Guid ShipmentId { get; set; }
    public string AWBNumber { get; set; } = string.Empty;
    public DateTime? DeliveryDate { get; set; }
    public decimal CODAmount { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverCity { get; set; }
}

public class CODRemittanceSummaryDto
{
    public int TotalRemittances { get; set; }
    public int PendingRemittances { get; set; }
    public int PaidRemittances { get; set; }
    public decimal TotalCODAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalPendingCOD { get; set; }
}

public class CreateRemittanceRequest
{
    public Guid CustomerId { get; set; }
    public List<Guid> ShipmentIds { get; set; } = new();
    public string? Remarks { get; set; }
}

public class ProcessPaymentRequest
{
    public Guid? ProcessedByUserId { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}

public class VoidRemittanceRequest
{
    public string Reason { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
}
