using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.GeneralLedger.Models;
using Server.Modules.GeneralLedger.Services;
using System.Security.Claims;

namespace Server.Modules.GeneralLedger.Controllers;

[Authorize]
[ApiController]
[Route("api/financial-calendars")]
public class FinancialCalendarController : ControllerBase
{
    private readonly IFinancialCalendarService _calendarService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<FinancialCalendarController> _logger;

    public FinancialCalendarController(
        IFinancialCalendarService calendarService,
        ITenantProvider tenantProvider,
        ILogger<FinancialCalendarController> logger)
    {
        _calendarService = calendarService;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.CurrentTenantId ?? throw new UnauthorizedAccessException("Tenant context not available");
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User context not available");
        }
        return userId;
    }

    [HttpGet]
    public async Task<ActionResult<List<FinancialCalendar>>> GetAll()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var calendars = await _calendarService.GetAllCalendarsAsync(tenantId);
            return Ok(calendars);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial calendars");
            return StatusCode(500, "An error occurred while retrieving financial calendars");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FinancialCalendar>> GetById(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var calendar = await _calendarService.GetCalendarByIdAsync(id, tenantId);
            if (calendar == null)
            {
                return NotFound();
            }
            return Ok(calendar);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial calendar {CalendarId}", id);
            return StatusCode(500, "An error occurred while retrieving the financial calendar");
        }
    }

    [HttpGet("primary")]
    public async Task<ActionResult<FinancialCalendar>> GetPrimary()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var calendar = await _calendarService.GetPrimaryCalendarAsync(tenantId);
            if (calendar == null)
            {
                return NotFound("No primary calendar configured");
            }
            return Ok(calendar);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting primary financial calendar");
            return StatusCode(500, "An error occurred while retrieving the primary financial calendar");
        }
    }

    [HttpGet("multi-calendar-enabled")]
    public async Task<ActionResult<bool>> IsMultiCalendarEnabled()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var enabled = await _calendarService.IsMultiCalendarEnabledAsync(tenantId);
            return Ok(enabled);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking multi-calendar status");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost("multi-calendar-enabled")]
    public async Task<ActionResult> SetMultiCalendarEnabled([FromBody] bool enabled)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            await _calendarService.SetMultiCalendarEnabledAsync(tenantId, enabled);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting multi-calendar status");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost]
    public async Task<ActionResult<FinancialCalendar>> Create([FromBody] FinancialCalendar calendar)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var isEnabled = await _calendarService.IsMultiCalendarEnabledAsync(tenantId);
            if (!isEnabled)
            {
                return BadRequest("Multiple financial calendars feature is not enabled. Enable it in Company Settings first.");
            }
            
            calendar.TenantId = tenantId;
            calendar.Id = Guid.NewGuid();
            
            var created = await _calendarService.CreateCalendarAsync(calendar, tenantId, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating financial calendar");
            return StatusCode(500, "An error occurred while creating the financial calendar");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FinancialCalendar>> Update(Guid id, [FromBody] FinancialCalendar calendar)
    {
        try
        {
            if (id != calendar.Id)
            {
                return BadRequest("Calendar ID mismatch");
            }

            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var isEnabled = await _calendarService.IsMultiCalendarEnabledAsync(tenantId);
            if (!isEnabled)
            {
                return BadRequest("Multiple financial calendars feature is not enabled.");
            }
            
            calendar.TenantId = tenantId;
            
            var updated = await _calendarService.UpdateCalendarAsync(calendar, tenantId, userId);
            return Ok(updated);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating financial calendar {CalendarId}", id);
            return StatusCode(500, "An error occurred while updating the financial calendar");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            
            var isEnabled = await _calendarService.IsMultiCalendarEnabledAsync(tenantId);
            if (!isEnabled)
            {
                return BadRequest("Multiple financial calendars feature is not enabled.");
            }
            
            await _calendarService.DeleteCalendarAsync(id, tenantId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting financial calendar {CalendarId}", id);
            return StatusCode(500, "An error occurred while deleting the financial calendar");
        }
    }

    [HttpPost("{id}/generate-periods")]
    public async Task<ActionResult<List<FinancialPeriod>>> GeneratePeriods(Guid id, [FromBody] GeneratePeriodsRequest request)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var isEnabled = await _calendarService.IsMultiCalendarEnabledAsync(tenantId);
            if (!isEnabled)
            {
                return BadRequest("Multiple financial calendars feature is not enabled.");
            }
            
            var periods = await _calendarService.GeneratePeriodsAsync(id, request.FiscalYear, tenantId, userId);
            return Ok(periods);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating periods for calendar {CalendarId}", id);
            return StatusCode(500, "An error occurred while generating periods");
        }
    }
}

public class GeneratePeriodsRequest
{
    public int FiscalYear { get; set; }
}
