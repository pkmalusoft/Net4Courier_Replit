using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.GeneralLedger.Models;
using Server.Modules.GeneralLedger.Services;
using System.Security.Claims;

namespace Server.Modules.GeneralLedger.Controllers;

[Authorize]
[ApiController]
[Route("api/revenue-recognition")]
public class RevenueRecognitionController : ControllerBase
{
    private readonly IRevenueRecognitionService _recognitionService;
    private readonly IRevenueRecognitionEngine _recognitionEngine;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<RevenueRecognitionController> _logger;

    public RevenueRecognitionController(
        IRevenueRecognitionService recognitionService,
        IRevenueRecognitionEngine recognitionEngine,
        ITenantProvider tenantProvider,
        ILogger<RevenueRecognitionController> logger)
    {
        _recognitionService = recognitionService;
        _recognitionEngine = recognitionEngine;
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

    [HttpGet("schedules")]
    public async Task<ActionResult<List<RevenueRecognitionSchedule>>> GetAllSchedules()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var schedules = await _recognitionService.GetAllSchedulesAsync(tenantId);
            return Ok(schedules);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue recognition schedules");
            return StatusCode(500, "An error occurred while retrieving schedules");
        }
    }

    [HttpGet("schedules/{id}")]
    public async Task<ActionResult<RevenueRecognitionSchedule>> GetScheduleById(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var schedule = await _recognitionService.GetScheduleByIdAsync(id, tenantId);
            if (schedule == null)
            {
                return NotFound();
            }
            return Ok(schedule);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue recognition schedule {ScheduleId}", id);
            return StatusCode(500, "An error occurred while retrieving the schedule");
        }
    }

    [HttpGet("schedules/customer/{customerId}")]
    public async Task<ActionResult<List<RevenueRecognitionSchedule>>> GetSchedulesByCustomer(Guid customerId)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var schedules = await _recognitionService.GetSchedulesByCustomerAsync(customerId, tenantId);
            return Ok(schedules);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schedules for customer {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while retrieving schedules");
        }
    }

    [HttpPost("schedules")]
    public async Task<ActionResult<RevenueRecognitionSchedule>> CreateSchedule([FromBody] RevenueRecognitionSchedule schedule)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            schedule.TenantId = tenantId;
            schedule.Id = Guid.NewGuid();
            
            var created = await _recognitionService.CreateScheduleAsync(schedule, tenantId, userId);
            return CreatedAtAction(nameof(GetScheduleById), new { id = created.Id }, created);
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
            _logger.LogError(ex, "Error creating revenue recognition schedule");
            return StatusCode(500, "An error occurred while creating the schedule");
        }
    }

    [HttpGet("schedules/{id}/lines")]
    public async Task<ActionResult<List<RevenueRecognitionLine>>> GetScheduleLines(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var lines = await _recognitionService.GetScheduleLinesAsync(id, tenantId);
            return Ok(lines);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schedule lines for {ScheduleId}", id);
            return StatusCode(500, "An error occurred while retrieving schedule lines");
        }
    }

    [HttpPost("schedules/{id}/cancel")]
    public async Task<ActionResult<RevenueRecognitionSchedule>> CancelSchedule(Guid id, [FromBody] CancelScheduleRequest? request = null)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            var cancelled = await _recognitionService.CancelScheduleAsync(id, tenantId, userId, request?.Reason);
            return Ok(cancelled);
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
            _logger.LogError(ex, "Error cancelling schedule {ScheduleId}", id);
            return StatusCode(500, "An error occurred while cancelling the schedule");
        }
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DeferredRevenueDashboard>> GetDashboard()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var dashboard = await _recognitionService.GetDashboardAsync(tenantId);
            return Ok(dashboard);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deferred revenue dashboard");
            return StatusCode(500, "An error occurred while retrieving dashboard data");
        }
    }

    [HttpGet("pending-lines")]
    public async Task<ActionResult<List<RevenueRecognitionLine>>> GetPendingLines([FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var lines = await _recognitionService.GetPendingLinesAsync(tenantId, asOfDate);
            return Ok(lines);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending recognition lines");
            return StatusCode(500, "An error occurred while retrieving pending lines");
        }
    }

    [HttpPost("process-pending")]
    public async Task<ActionResult<RecognitionProcessingResult>> ProcessPendingRecognitions()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            var result = await _recognitionEngine.ProcessPendingRecognitionsAsync(tenantId, userId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending recognitions");
            return StatusCode(500, "An error occurred while processing recognitions");
        }
    }

    [HttpPost("lines/{lineId}/process")]
    public async Task<ActionResult<RecognitionProcessingResult>> ProcessSingleLine(Guid lineId)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            var result = await _recognitionEngine.ProcessSingleLineAsync(lineId, tenantId, userId);
            return Ok(result);
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
            _logger.LogError(ex, "Error processing recognition line {LineId}", lineId);
            return StatusCode(500, "An error occurred while processing the recognition line");
        }
    }

    [HttpGet("alerts/pending")]
    public async Task<ActionResult<List<DeferredRevenueAlert>>> GetPendingAlerts()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var alerts = await _recognitionService.GetAlertsAsync(tenantId, unreadOnly: true);
            return Ok(alerts);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending deferred revenue alerts");
            return StatusCode(500, "An error occurred while retrieving alerts");
        }
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<List<DeferredRevenueAlert>>> GetAlerts([FromQuery] bool unreadOnly = false)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var alerts = await _recognitionService.GetAlertsAsync(tenantId, unreadOnly);
            return Ok(alerts);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deferred revenue alerts");
            return StatusCode(500, "An error occurred while retrieving alerts");
        }
    }

    [HttpGet("alerts/summary")]
    public async Task<ActionResult<DeferredRevenueAlertSummary>> GetAlertSummary()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var summary = await _recognitionService.GetAlertSummaryAsync(tenantId);
            return Ok(summary);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alert summary");
            return StatusCode(500, "An error occurred while retrieving alert summary");
        }
    }

    [HttpPost("alerts/{alertId}/acknowledge")]
    public async Task<ActionResult> AcknowledgeAlert(Guid alertId)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            await _recognitionService.AcknowledgeAlertAsync(alertId, tenantId, userId);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alert {AlertId}", alertId);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost("alerts/{alertId}/dismiss")]
    public async Task<ActionResult> DismissAlert(Guid alertId)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            await _recognitionService.DismissAlertAsync(alertId, tenantId, userId);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing alert {AlertId}", alertId);
            return StatusCode(500, "An error occurred");
        }
    }
}

public class CancelScheduleRequest
{
    public string? Reason { get; set; }
}
