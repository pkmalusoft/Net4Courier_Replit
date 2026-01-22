using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/customer-rate-assignments")]
public class CustomerRateAssignmentController : ControllerBase
{
    private readonly ICustomerRateAssignmentService _assignmentService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CustomerRateAssignmentController> _logger;

    public CustomerRateAssignmentController(
        ICustomerRateAssignmentService assignmentService,
        ITenantProvider tenantProvider,
        ILogger<CustomerRateAssignmentController> logger)
    {
        _assignmentService = assignmentService;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerRateAssignmentDto>>> GetAll()
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            var assignments = await _assignmentService.GetAllAsync();
            return Ok(assignments.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching customer rate assignments");
            return StatusCode(500, "An error occurred while fetching assignments.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerRateAssignmentDto>> GetById(Guid id)
    {
        try
        {
            var assignment = await _assignmentService.GetByIdAsync(id);
            if (assignment == null)
                return NotFound();

            return Ok(MapToDto(assignment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assignment {Id}", id);
            return StatusCode(500, "An error occurred while fetching the assignment.");
        }
    }

    [HttpGet("by-customer/{customerId}")]
    public async Task<ActionResult<List<CustomerRateAssignmentDto>>> GetByCustomerId(Guid customerId)
    {
        try
        {
            var assignments = await _assignmentService.GetByCustomerIdAsync(customerId);
            return Ok(assignments.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assignments for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while fetching assignments.");
        }
    }

    [HttpGet("by-rate-name/{rateName}")]
    public async Task<ActionResult<List<CustomerRateAssignmentDto>>> GetByRateName(string rateName)
    {
        try
        {
            var assignments = await _assignmentService.GetByRateNameAsync(rateName);
            return Ok(assignments.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assignments for rate {RateName}", rateName);
            return StatusCode(500, "An error occurred while fetching assignments.");
        }
    }

    [HttpGet("active-rate/{customerId}")]
    public async Task<ActionResult<string?>> GetActiveRateForCustomer(Guid customerId, [FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            var rateName = await _assignmentService.GetActiveRateNameForCustomerAsync(customerId, asOfDate);
            return Ok(new { RateName = rateName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active rate for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while fetching the active rate.");
        }
    }

    [HttpGet("rate-names")]
    public async Task<ActionResult<List<string>>> GetDistinctRateNames()
    {
        try
        {
            var rateNames = await _assignmentService.GetDistinctRateNamesAsync();
            return Ok(rateNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching rate names");
            return StatusCode(500, "An error occurred while fetching rate names.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CustomerRateAssignmentDto>> Create([FromBody] CreateCustomerRateAssignmentDto dto)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            if (string.IsNullOrWhiteSpace(dto.RateName))
                return BadRequest("Rate name is required.");

            // Allow 1-day buffer for timezone differences (frontend enforces strict validation)
            if (dto.EffectiveFrom.Date < DateTime.UtcNow.Date.AddDays(-1))
                return BadRequest("Effective from date cannot be in the past.");

            var assignment = new CustomerRateAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId.Value,
                CustomerId = dto.CustomerId,
                RateName = dto.RateName.Trim(),
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                IsActive = dto.IsActive,
                Notes = dto.Notes
            };

            var created = await _assignmentService.CreateAsync(assignment);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer rate assignment");
            return StatusCode(500, "An error occurred while creating the assignment.");
        }
    }

    [HttpPost("bulk-assign")]
    public async Task<ActionResult<List<CustomerRateAssignmentDto>>> BulkAssign([FromBody] BulkAssignRateDto dto)
    {
        try
        {
            var tenantId = _tenantProvider.CurrentTenantId;
            if (!tenantId.HasValue)
                return BadRequest("Tenant context required.");

            if (string.IsNullOrWhiteSpace(dto.RateName))
                return BadRequest("Rate name is required.");

            if (dto.CustomerIds == null || !dto.CustomerIds.Any())
                return BadRequest("At least one customer is required.");

            // Allow 1-day buffer for timezone differences (frontend enforces strict validation)
            if (dto.EffectiveFrom.Date < DateTime.UtcNow.Date.AddDays(-1))
                return BadRequest("Effective from date cannot be in the past.");

            var assignments = await _assignmentService.BulkAssignAsync(
                dto.CustomerIds,
                dto.RateName.Trim(),
                dto.EffectiveFrom,
                dto.EffectiveTo,
                dto.Notes);

            return Ok(assignments.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk assigning rate");
            return StatusCode(500, "An error occurred while bulk assigning rates.");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerRateAssignmentDto>> Update(Guid id, [FromBody] UpdateCustomerRateAssignmentDto dto)
    {
        try
        {
            var existing = await _assignmentService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(dto.RateName))
                return BadRequest("Rate name is required.");

            existing.RateName = dto.RateName.Trim();
            existing.EffectiveFrom = dto.EffectiveFrom;
            existing.EffectiveTo = dto.EffectiveTo;
            existing.IsActive = dto.IsActive;
            existing.Notes = dto.Notes;

            var updated = await _assignmentService.UpdateAsync(existing);
            return Ok(MapToDto(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assignment {Id}", id);
            return StatusCode(500, "An error occurred while updating the assignment.");
        }
    }

    [HttpPut("{id}/deactivate")]
    public async Task<ActionResult> Deactivate(Guid id)
    {
        try
        {
            var result = await _assignmentService.DeactivateAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating assignment {Id}", id);
            return StatusCode(500, "An error occurred while deactivating the assignment.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _assignmentService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assignment {Id}", id);
            return StatusCode(500, "An error occurred while deleting the assignment.");
        }
    }

    private static CustomerRateAssignmentDto MapToDto(CustomerRateAssignment assignment)
    {
        return new CustomerRateAssignmentDto
        {
            Id = assignment.Id,
            CustomerId = assignment.CustomerId,
            CustomerName = assignment.Customer?.Name ?? string.Empty,
            CustomerCode = assignment.Customer?.CustomerCode ?? string.Empty,
            RateName = assignment.RateName,
            EffectiveFrom = assignment.EffectiveFrom,
            EffectiveTo = assignment.EffectiveTo,
            IsActive = assignment.IsActive,
            Notes = assignment.Notes,
            CreatedAt = assignment.CreatedAt,
            UpdatedAt = assignment.UpdatedAt
        };
    }
}

public class CustomerRateAssignmentDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string RateName { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCustomerRateAssignmentDto
{
    public Guid CustomerId { get; set; }
    public string RateName { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateCustomerRateAssignmentDto
{
    public string RateName { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class BulkAssignRateDto
{
    public List<Guid> CustomerIds { get; set; } = new();
    public string RateName { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
}
