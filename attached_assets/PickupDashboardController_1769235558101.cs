using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PickupDashboardController : ControllerBase
{
    private readonly IPickupDashboardService _pickupDashboardService;
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<PickupDashboardController> _logger;

    public PickupDashboardController(
        IPickupDashboardService pickupDashboardService,
        AppDbContext context,
        ITenantProvider tenantProvider,
        ILogger<PickupDashboardController> logger)
    {
        _pickupDashboardService = pickupDashboardService;
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<PickupDashboardSummaryDto>> GetSummary(
        [FromQuery] DateRangeType rangeType = DateRangeType.Today,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var summary = await _pickupDashboardService.GetSummaryAsync(rangeType, fromDate, toDate);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pickup dashboard summary");
            return StatusCode(500, new { error = "An error occurred while loading pickup summary", message = ex.Message });
        }
    }

    [HttpGet("by-customer")]
    public async Task<ActionResult<List<CustomerPickupGroupDto>>> GetPickupsByCustomer(
        [FromQuery] DateRangeType rangeType = DateRangeType.Today,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var grouped = await _pickupDashboardService.GetPickupsByCustomerAsync(rangeType, fromDate, toDate);
            return Ok(grouped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pickups by customer");
            return StatusCode(500, new { error = "An error occurred while loading customer pickups", message = ex.Message });
        }
    }

    [HttpGet("pickup-request/{pickupRequestId}/awbs")]
    public async Task<ActionResult<List<PickupAwbDto>>> GetAwbsByPickupRequest(Guid pickupRequestId)
    {
        try
        {
            var awbs = await _pickupDashboardService.GetAwbsByPickupRequestAsync(pickupRequestId);
            return Ok(awbs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AWBs for pickup request {PickupRequestId}", pickupRequestId);
            return StatusCode(500, new { error = "An error occurred while loading AWBs", message = ex.Message });
        }
    }

    [HttpGet("customer/{customerId}/awbs")]
    public async Task<ActionResult<List<PickupAwbDto>>> GetAwbsByCustomer(
        Guid customerId,
        [FromQuery] DateRangeType rangeType = DateRangeType.Today,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var awbs = await _pickupDashboardService.GetAwbsByCustomerAsync(customerId, rangeType, fromDate, toDate);
            return Ok(awbs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AWBs for customer {CustomerId}", customerId);
            return StatusCode(500, new { error = "An error occurred while loading AWBs", message = ex.Message });
        }
    }

    [HttpGet("courier-performance")]
    public async Task<ActionResult<List<CourierPerformanceDto>>> GetCourierPerformance(
        [FromQuery] DateRangeType rangeType = DateRangeType.Today,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var performance = await _pickupDashboardService.GetCourierPerformanceAsync(rangeType, fromDate, toDate);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courier performance");
            return StatusCode(500, new { error = "An error occurred while loading courier performance", message = ex.Message });
        }
    }

    [HttpPost("assign-shipments")]
    public async Task<ActionResult> AssignShipmentsToAgent([FromBody] AssignShipmentsRequest request)
    {
        try
        {
            if (request.ShipmentIds == null || !request.ShipmentIds.Any())
                return BadRequest(new { error = "No shipments selected for assignment" });

            if (request.AgentId == Guid.Empty)
                return BadRequest(new { error = "Agent not specified" });

            var result = await _pickupDashboardService.AssignShipmentsToAgentAsync(request.ShipmentIds, request.AgentId);
            
            if (!result)
                return NotFound(new { error = "Agent not found or inactive" });

            return Ok(new { message = $"Successfully assigned {request.ShipmentIds.Count} shipments to agent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning shipments to agent");
            return StatusCode(500, new { error = "An error occurred while assigning shipments", message = ex.Message });
        }
    }

    [HttpPost("assign-pickup")]
    public async Task<ActionResult> AssignPickupToAgent([FromBody] AssignPickupRequest request)
    {
        try
        {
            if (request.PickupRequestId == Guid.Empty)
                return BadRequest(new { error = "Pickup request not specified" });

            if (request.AgentId == Guid.Empty)
                return BadRequest(new { error = "Agent not specified" });

            var result = await _pickupDashboardService.AssignPickupRequestToAgentAsync(request.PickupRequestId, request.AgentId);
            
            if (!result)
                return NotFound(new { error = "Agent or pickup request not found" });

            return Ok(new { message = "Successfully assigned pickup request to agent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning pickup request to agent");
            return StatusCode(500, new { error = "An error occurred while assigning pickup request", message = ex.Message });
        }
    }

    [HttpGet("available-agents")]
    public async Task<ActionResult<List<AgentSelectDto>>> GetAvailableAgents()
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return Unauthorized();

            var agents = await _context.CourierAgents
                .Where(a => a.TenantId == tenantId.Value && a.IsActive &&
                           (a.AgentType == AgentType.PickupAgent || a.AgentType == AgentType.DeliveryAgent))
                .Select(a => new AgentSelectDto
                {
                    Id = a.Id,
                    AgentCode = a.AgentCode,
                    Name = a.Name,
                    AgentType = a.AgentType.ToString(),
                    Phone = a.Phone ?? a.Mobile
                })
                .OrderBy(a => a.Name)
                .ToListAsync();

            return Ok(agents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available agents");
            return StatusCode(500, new { error = "An error occurred while loading agents", message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PickupRequestDetailDto>> GetPickupRequest(Guid id)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return Unauthorized();

            var pickup = await _context.PickupRequests
                .Include(p => p.Customer)
                .Where(p => p.Id == id && p.TenantId == tenantId.Value)
                .Select(p => new PickupRequestDetailDto
                {
                    Id = p.Id,
                    RequestNumber = p.RequestNumber,
                    AWBNumber = p.AWBNumber,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer != null ? p.Customer.Name : null,
                    ContactName = p.ContactName,
                    ContactPhone = p.ContactPhone,
                    ContactEmail = p.ContactEmail,
                    PickupAddress = p.PickupAddress,
                    City = p.City,
                    State = p.State,
                    PostalCode = p.PostalCode,
                    Country = p.Country,
                    ScheduledDate = p.ScheduledDate,
                    PreferredTimeFrom = p.PreferredTimeFrom,
                    PreferredTimeTo = p.PreferredTimeTo,
                    ExpectedPieces = p.ExpectedPieces,
                    ExpectedWeight = p.ExpectedWeight,
                    ActualPieces = p.ActualPieces,
                    ActualWeight = p.ActualWeight,
                    SpecialInstructions = p.SpecialInstructions,
                    Status = p.Status.ToString(),
                    AssignedAgentId = p.AssignedAgentId
                })
                .FirstOrDefaultAsync();

            if (pickup == null)
                return NotFound(new { error = "Pickup request not found" });

            return Ok(pickup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pickup request {Id}", id);
            return StatusCode(500, new { error = "An error occurred while loading pickup request", message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<PickupRequestDetailDto>> CreatePickupRequest([FromBody] CreatePickupRequestDto request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return Unauthorized();

            var requestNumber = await GenerateRequestNumber(tenantId.Value);

            var pickup = new PickupRequest
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId.Value,
                RequestNumber = requestNumber,
                AWBNumber = request.AWBNumber,
                RequestDate = DateTime.UtcNow,
                CustomerId = request.CustomerId,
                ContactName = request.ContactName,
                ContactPhone = request.ContactPhone,
                ContactEmail = request.ContactEmail,
                PickupAddress = request.PickupAddress,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                ScheduledDate = request.ScheduledDate,
                PreferredTimeFrom = request.PreferredTimeFrom,
                PreferredTimeTo = request.PreferredTimeTo,
                ExpectedPieces = request.ExpectedPieces,
                ExpectedWeight = request.ExpectedWeight,
                SpecialInstructions = request.SpecialInstructions,
                Status = PickupStatus.Requested,
                CreatedAt = DateTime.UtcNow
            };

            _context.PickupRequests.Add(pickup);
            await _context.SaveChangesAsync();

            return Ok(new PickupRequestDetailDto
            {
                Id = pickup.Id,
                RequestNumber = pickup.RequestNumber,
                CustomerId = pickup.CustomerId,
                ContactName = pickup.ContactName,
                ContactPhone = pickup.ContactPhone,
                ContactEmail = pickup.ContactEmail,
                PickupAddress = pickup.PickupAddress,
                City = pickup.City,
                State = pickup.State,
                PostalCode = pickup.PostalCode,
                Country = pickup.Country,
                ScheduledDate = pickup.ScheduledDate,
                PreferredTimeFrom = pickup.PreferredTimeFrom,
                PreferredTimeTo = pickup.PreferredTimeTo,
                ExpectedPieces = pickup.ExpectedPieces,
                ExpectedWeight = pickup.ExpectedWeight,
                SpecialInstructions = pickup.SpecialInstructions,
                Status = pickup.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pickup request");
            return StatusCode(500, new { error = "An error occurred while creating pickup request", message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PickupRequestDetailDto>> UpdatePickupRequest(Guid id, [FromBody] CreatePickupRequestDto request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return Unauthorized();

            var pickup = await _context.PickupRequests
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId.Value);

            if (pickup == null)
                return NotFound(new { error = "Pickup request not found" });

            pickup.CustomerId = request.CustomerId;
            pickup.AWBNumber = request.AWBNumber;
            pickup.ContactName = request.ContactName;
            pickup.ContactPhone = request.ContactPhone;
            pickup.ContactEmail = request.ContactEmail;
            pickup.PickupAddress = request.PickupAddress;
            pickup.City = request.City;
            pickup.State = request.State;
            pickup.PostalCode = request.PostalCode;
            pickup.Country = request.Country;
            pickup.ScheduledDate = request.ScheduledDate;
            pickup.PreferredTimeFrom = request.PreferredTimeFrom;
            pickup.PreferredTimeTo = request.PreferredTimeTo;
            pickup.ExpectedPieces = request.ExpectedPieces;
            pickup.ExpectedWeight = request.ExpectedWeight;
            pickup.SpecialInstructions = request.SpecialInstructions;
            pickup.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new PickupRequestDetailDto
            {
                Id = pickup.Id,
                RequestNumber = pickup.RequestNumber,
                CustomerId = pickup.CustomerId,
                ContactName = pickup.ContactName,
                ContactPhone = pickup.ContactPhone,
                ContactEmail = pickup.ContactEmail,
                PickupAddress = pickup.PickupAddress,
                City = pickup.City,
                State = pickup.State,
                PostalCode = pickup.PostalCode,
                Country = pickup.Country,
                ScheduledDate = pickup.ScheduledDate,
                PreferredTimeFrom = pickup.PreferredTimeFrom,
                PreferredTimeTo = pickup.PreferredTimeTo,
                ExpectedPieces = pickup.ExpectedPieces,
                ExpectedWeight = pickup.ExpectedWeight,
                SpecialInstructions = pickup.SpecialInstructions,
                Status = pickup.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pickup request {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating pickup request", message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdatePickupStatus(Guid id, [FromBody] UpdatePickupStatusRequest request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return Unauthorized();

            var pickup = await _context.PickupRequests
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId.Value);

            if (pickup == null)
                return NotFound(new { error = "Pickup request not found" });

            if (Enum.TryParse<PickupStatus>(request.Status, out var status))
            {
                pickup.Status = status;
                
                if (status == PickupStatus.Completed && request.ActualPieces.HasValue)
                {
                    pickup.ActualPieces = request.ActualPieces.Value;
                    pickup.ActualWeight = request.ActualWeight;
                    pickup.ActualPickupTime = DateTime.UtcNow;
                }
                
                if (status == PickupStatus.Failed || status == PickupStatus.Cancelled)
                {
                    pickup.FailureReason = request.Reason;
                    if (status == PickupStatus.Cancelled)
                        pickup.CancellationReason = request.Reason;
                }

                pickup.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Pickup request status updated to {status}" });
            }

            return BadRequest(new { error = "Invalid status value" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pickup request status {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating status", message = ex.Message });
        }
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<PickupRequestListItemDto>>> GetPickupRequestList(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return Unauthorized();

            var query = _context.PickupRequests
                .Include(p => p.Customer)
                .Include(p => p.AssignedAgent)
                .Where(p => p.TenantId == tenantId.Value && !p.IsVoided);

            if (fromDate.HasValue)
                query = query.Where(p => p.ScheduledDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(p => p.ScheduledDate <= toDate.Value.AddDays(1));
            
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<PickupStatus>(status, out var pickupStatus))
                query = query.Where(p => p.Status == pickupStatus);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p => 
                    p.RequestNumber.ToLower().Contains(searchLower) ||
                    (p.AWBNumber != null && p.AWBNumber.ToLower().Contains(searchLower)) ||
                    (p.Customer != null && p.Customer.Name.ToLower().Contains(searchLower)) ||
                    p.ContactName.ToLower().Contains(searchLower));
            }

            var items = await query
                .OrderByDescending(p => p.ScheduledDate)
                .ThenByDescending(p => p.RequestNumber)
                .Select(p => new PickupRequestListItemDto
                {
                    Id = p.Id,
                    RequestNumber = p.RequestNumber,
                    AWBNumber = p.AWBNumber,
                    ScheduledDate = p.ScheduledDate,
                    CustomerName = p.Customer != null ? p.Customer.Name : null,
                    ContactName = p.ContactName,
                    ContactPhone = p.ContactPhone,
                    City = p.City,
                    Country = p.Country,
                    ExpectedPieces = p.ExpectedPieces,
                    ExpectedWeight = p.ExpectedWeight,
                    AssignedAgentName = p.AssignedAgent != null ? p.AssignedAgent.Name : null,
                    Status = p.Status.ToString()
                })
                .ToListAsync();

            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pickup request list");
            return StatusCode(500, new { error = "An error occurred while loading pickup requests", message = ex.Message });
        }
    }

    [HttpGet("next-awb-preview")]
    public async Task<ActionResult> GetNextAWBPreview()
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return Unauthorized();

            var nextAwb = await PreviewNextAwbNumber(tenantId.Value);
            return Ok(new { nextAWBNumber = nextAwb });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next AWB preview");
            return StatusCode(500, new { error = "An error occurred while fetching next AWB number", message = ex.Message });
        }
    }

    [HttpPost("inscan")]
    public async Task<ActionResult> CreateShipmentFromInscan([FromBody] InscanShipmentDto request)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return Unauthorized();

            if (request.NumberOfPieces <= 0)
                return BadRequest(new { error = "Number of pieces must be greater than zero" });

            if (request.Weight <= 0)
                return BadRequest(new { error = "Weight must be greater than zero" });

            if (string.IsNullOrWhiteSpace(request.SenderName))
                return BadRequest(new { error = "Sender name is required" });

            if (string.IsNullOrWhiteSpace(request.ReceiverName))
                return BadRequest(new { error = "Receiver name is required" });

            var pickupRequest = await _context.PickupRequests
                .FirstOrDefaultAsync(p => p.Id == request.PickupRequestId && p.TenantId == tenantId.Value);

            if (pickupRequest == null)
                return NotFound(new { error = "Pickup request not found" });

            if (pickupRequest.Status == PickupStatus.Completed)
                return BadRequest(new { error = "This pickup request has already been completed. No more shipments can be added." });

            string awbNumber;
            if (!request.AutoGenerateAWB && !string.IsNullOrWhiteSpace(request.AWBNumber))
            {
                awbNumber = request.AWBNumber.Trim();
                
                var existingAwb = await _context.Shipments
                    .AnyAsync(s => s.TenantId == tenantId.Value && s.AWBNumber == awbNumber);
                
                if (existingAwb)
                    return BadRequest(new { error = $"Air Waybill Number '{awbNumber}' already exists. Please enter a unique AWB number." });
            }
            else
            {
                awbNumber = await GenerateAwbNumber(tenantId.Value);
            }

            var defaultServiceType = await _context.CourierServiceTypes
                .Where(c => c.TenantId == tenantId.Value && c.IsActive)
                .OrderBy(c => c.Name)
                .FirstOrDefaultAsync();

            if (defaultServiceType == null)
                return BadRequest(new { error = "No active courier service type found. Please configure at least one service type in Settings → Service Types." });

            var shipment = new Shipment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId.Value,
                AWBNumber = awbNumber,
                BookingDate = DateTime.UtcNow,
                CustomerId = request.CustomerId ?? pickupRequest.CustomerId,
                CourierServiceTypeId = defaultServiceType.Id,
                SenderName = request.SenderName,
                SenderPhone = request.SenderPhone,
                SenderAddress = request.SenderAddress,
                ReceiverName = request.ReceiverName,
                ReceiverPhone = request.ReceiverPhone,
                ReceiverAddress = request.ReceiverAddress,
                ReceiverCity = request.ReceiverCity,
                ReceiverState = request.ReceiverState,
                ReceiverPostalCode = request.ReceiverPostalCode,
                ReceiverCountry = request.ReceiverCountry,
                NumberOfPieces = request.NumberOfPieces,
                ActualWeight = request.Weight,
                PaymentMode = Enum.TryParse<PaymentMode>(request.PaymentMode, out var pm) ? pm : PaymentMode.Prepaid,
                CODAmount = request.CODAmount ?? 0,
                ContentDescription = request.ContentDescription,
                SpecialInstructions = request.SpecialInstructions,
                Status = ShipmentStatus.Booked,
                PickupRequestId = request.PickupRequestId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            if (request.MarkCompleted)
            {
                var totalPieces = await _context.Shipments
                    .Where(s => s.PickupRequestId == request.PickupRequestId && s.TenantId == tenantId.Value)
                    .SumAsync(s => s.NumberOfPieces);
                    
                var totalWeight = await _context.Shipments
                    .Where(s => s.PickupRequestId == request.PickupRequestId && s.TenantId == tenantId.Value)
                    .SumAsync(s => s.ActualWeight);

                pickupRequest.ActualPieces = totalPieces;
                pickupRequest.ActualWeight = totalWeight;
                pickupRequest.Status = PickupStatus.Completed;
                pickupRequest.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new { 
                message = request.MarkCompleted 
                    ? "Shipment created and pickup request marked as completed" 
                    : "Shipment created successfully", 
                awbNumber = shipment.AWBNumber,
                shipmentId = shipment.Id,
                pickupCompleted = request.MarkCompleted
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment from inscan");
            return StatusCode(500, new { error = "An error occurred while creating shipment", message = ex.Message });
        }
    }

    private async Task<string> GenerateRequestNumber(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PR{today:yyMMdd}";
        
        var lastRequest = await _context.PickupRequests
            .Where(p => p.TenantId == tenantId && p.RequestNumber.StartsWith(prefix))
            .OrderByDescending(p => p.RequestNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastRequest != null && lastRequest.RequestNumber.Length > prefix.Length)
        {
            var seqStr = lastRequest.RequestNumber.Substring(prefix.Length);
            if (int.TryParse(seqStr, out var lastSeq))
                sequence = lastSeq + 1;
        }

        return $"{prefix}{sequence:D4}";
    }

    private async Task<string> GenerateAwbNumber(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"AWB{today:yyMMdd}";
        
        var lastShipment = await _context.Shipments
            .Where(s => s.TenantId == tenantId && s.AWBNumber.StartsWith(prefix))
            .OrderByDescending(s => s.AWBNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastShipment != null && lastShipment.AWBNumber.Length > prefix.Length)
        {
            var seqStr = lastShipment.AWBNumber.Substring(prefix.Length);
            if (int.TryParse(seqStr, out var lastSeq))
                sequence = lastSeq + 1;
        }

        return $"{prefix}{sequence:D5}";
    }

    private async Task<string> PreviewNextAwbNumber(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"AWB{today:yyMMdd}";
        
        var lastShipment = await _context.Shipments
            .Where(s => s.TenantId == tenantId && s.AWBNumber.StartsWith(prefix))
            .OrderByDescending(s => s.AWBNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastShipment != null && lastShipment.AWBNumber.Length > prefix.Length)
        {
            var seqStr = lastShipment.AWBNumber.Substring(prefix.Length);
            if (int.TryParse(seqStr, out var lastSeq))
                sequence = lastSeq + 1;
        }

        return $"{prefix}{sequence:D5}";
    }
}

public class AgentSelectDto
{
    public Guid Id { get; set; }
    public string AgentCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AgentType { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class PickupRequestDetailDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string? AWBNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public DateTime ScheduledDate { get; set; }
    public TimeSpan? PreferredTimeFrom { get; set; }
    public TimeSpan? PreferredTimeTo { get; set; }
    public int ExpectedPieces { get; set; }
    public decimal? ExpectedWeight { get; set; }
    public int ActualPieces { get; set; }
    public decimal? ActualWeight { get; set; }
    public string? SpecialInstructions { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedAgentId { get; set; }
}

public class CreatePickupRequestDto
{
    public Guid? CustomerId { get; set; }
    public string? AWBNumber { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public DateTime ScheduledDate { get; set; }
    public TimeSpan? PreferredTimeFrom { get; set; }
    public TimeSpan? PreferredTimeTo { get; set; }
    public int ExpectedPieces { get; set; } = 1;
    public decimal? ExpectedWeight { get; set; }
    public string? SpecialInstructions { get; set; }
}

public class PickupRequestListItemDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string? AWBNumber { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? CustomerName { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int ExpectedPieces { get; set; }
    public decimal? ExpectedWeight { get; set; }
    public string? AssignedAgentName { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdatePickupStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public int? ActualPieces { get; set; }
    public decimal? ActualWeight { get; set; }
    public string? Reason { get; set; }
}

public class InscanShipmentDto
{
    public Guid? PickupRequestId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? AWBNumber { get; set; }
    public bool AutoGenerateAWB { get; set; } = true;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderPhone { get; set; }
    public string? SenderAddress { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverPhone { get; set; }
    public string? ReceiverAddress { get; set; }
    public string? ReceiverCity { get; set; }
    public string? ReceiverState { get; set; }
    public string? ReceiverPostalCode { get; set; }
    public string? ReceiverCountry { get; set; }
    public int NumberOfPieces { get; set; } = 1;
    public decimal Weight { get; set; }
    public string PaymentMode { get; set; } = "Prepaid";
    public decimal? CODAmount { get; set; }
    public string? ContentDescription { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool MarkCompleted { get; set; }
}
