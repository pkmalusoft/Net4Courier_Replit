using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Server.Modules.GeneralLedger.Models;
using System.Security.Claims;

namespace Server.Modules.GeneralLedger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FinancialPeriodController : ControllerBase
{
    private readonly IFinancialPeriodService _periodService;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<FinancialPeriodController> _logger;

    public FinancialPeriodController(
        IFinancialPeriodService periodService,
        ITenantProvider tenantProvider,
        ILogger<FinancialPeriodController> logger)
    {
        _periodService = periodService;
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

    private string GetCurrentUserRole()
    {
        // ASP.NET Identity uses multiple role claims, one per role
        // Check ClaimTypes.Role first (standard claim type)
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(roleClaim))
            return roleClaim;
        
        // Also check for the full URI claim type used by ASP.NET Identity
        var identityRoleClaim = User.Claims
            .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ||
                                  c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase))?.Value;
        if (!string.IsNullOrEmpty(identityRoleClaim))
            return identityRoleClaim;
        
        // Check for custom "role" claim (lowercase, as used in JWT)
        var customRoleClaim = User.FindFirst("role")?.Value;
        if (!string.IsNullOrEmpty(customRoleClaim))
            return customRoleClaim;
        
        return "user";
    }

    [HttpGet]
    public async Task<ActionResult<List<FinancialPeriod>>> GetAll()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var periods = await _periodService.GetAllPeriodsAsync(tenantId);
            return Ok(periods);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial periods");
            return StatusCode(500, "An error occurred while retrieving financial periods");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FinancialPeriod>> GetById(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var period = await _periodService.GetPeriodByIdAsync(id, tenantId);
            if (period == null)
            {
                return NotFound();
            }
            return Ok(period);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial period {PeriodId}", id);
            return StatusCode(500, "An error occurred while retrieving the financial period");
        }
    }

    [HttpGet("current")]
    public async Task<ActionResult<FinancialPeriod>> GetCurrent()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var period = await _periodService.GetCurrentPeriodAsync(tenantId);
            if (period == null)
            {
                return NotFound("No financial period defined for the current date");
            }
            return Ok(period);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current financial period");
            return StatusCode(500, "An error occurred while retrieving the current financial period");
        }
    }

    [HttpGet("for-date")]
    public async Task<ActionResult<FinancialPeriod>> GetForDate([FromQuery] DateTime date)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var period = await _periodService.GetPeriodForDateAsync(date, tenantId);
            if (period == null)
            {
                return NotFound($"No financial period defined for date {date:yyyy-MM-dd}");
            }
            return Ok(period);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial period for date {Date}", date);
            return StatusCode(500, "An error occurred while retrieving the financial period");
        }
    }

    [HttpGet("can-post")]
    public async Task<ActionResult<bool>> CanPostToDate([FromQuery] DateTime date)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var canPost = await _periodService.CanPostToDateAsync(date, tenantId);
            return Ok(canPost);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if can post to date {Date}", date);
            return StatusCode(500, "An error occurred while checking posting availability");
        }
    }

    [HttpGet("selectable")]
    [Authorize(AuthenticationSchemes = "Cookies,Bearer")]
    public async Task<ActionResult<List<FinancialPeriod>>> GetSelectablePeriods()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var periods = await _periodService.GetSelectablePeriodsAsync(tenantId);
            return Ok(periods);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting selectable financial periods");
            return StatusCode(500, "An error occurred while retrieving selectable periods");
        }
    }

    [HttpPost("create-fiscal-year")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<List<FinancialPeriod>>> CreateFiscalYear([FromBody] CreateFiscalYearRequest request)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var periods = await _periodService.CreateFiscalYearAsync(
                request.FiscalYear,
                request.StartMonth,
                tenantId,
                userId);
            
            return CreatedAtAction(nameof(GetAll), periods);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fiscal year {FiscalYear}", request.FiscalYear);
            return StatusCode(500, "An error occurred while creating the fiscal year");
        }
    }

    [HttpPost("{id}/open")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<FinancialPeriod>> OpenPeriod(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var period = await _periodService.OpenPeriodAsync(id, tenantId, userId, userRole);
            return Ok(period);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening period {PeriodId}", id);
            return StatusCode(500, "An error occurred while opening the period");
        }
    }

    [HttpPost("{id}/soft-close")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<FinancialPeriod>> SoftClosePeriod(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var period = await _periodService.SoftClosePeriodAsync(id, tenantId, userId, userRole);
            return Ok(period);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft closing period {PeriodId}", id);
            return StatusCode(500, "An error occurred while closing the period");
        }
    }

    [HttpPost("{id}/hard-close")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<FinancialPeriod>> HardClosePeriod(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var period = await _periodService.HardClosePeriodAsync(id, tenantId, userId, userRole);
            return Ok(period);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hard closing period {PeriodId}", id);
            return StatusCode(500, "An error occurred while hard closing the period");
        }
    }

    [HttpPost("{id}/reopen")]
    [Authorize(Roles = "owner,admin,auditor")]
    public async Task<ActionResult<FinancialPeriod>> ReopenPeriod(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var period = await _periodService.ReopenPeriodAsync(id, tenantId, userId, userRole);
            return Ok(period);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening period {PeriodId}", id);
            return StatusCode(500, "An error occurred while reopening the period");
        }
    }

    [HttpPost("{id}/archive")]
    [Authorize(Roles = "owner")]
    public async Task<ActionResult<FinancialPeriod>> ArchivePeriod(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            var period = await _periodService.LockPeriodAsync(id, tenantId, userId, userRole);
            return Ok(period);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving period {PeriodId}", id);
            return StatusCode(500, "An error occurred while archiving the period");
        }
    }

    [HttpGet("{id}/validation")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<PeriodValidationResult>> ValidatePeriod(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var result = await _periodService.ValidatePeriodForClosingAsync(id, tenantId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating period {PeriodId}", id);
            return StatusCode(500, "An error occurred while validating the period");
        }
    }

    [HttpGet("posting-permission")]
    [Authorize(AuthenticationSchemes = "Cookies,Bearer")]
    public async Task<ActionResult<PeriodPostingPermission>> GetPostingPermission([FromQuery] DateTime date)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userRole = GetCurrentUserRole();
            var result = await _periodService.GetPostingPermissionForDateAsync(date, tenantId, userRole);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking posting permission for date {Date}", date);
            return StatusCode(500, "An error occurred while checking posting permission");
        }
    }

    [HttpGet("{id}/audit-log")]
    [Authorize(Roles = "owner,admin,auditor")]
    public async Task<ActionResult<List<PeriodStatusChangeLog>>> GetPeriodAuditLog(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var logs = await _periodService.GetPeriodAuditLogAsync(id, tenantId);
            return Ok(logs);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log for period {PeriodId}", id);
            return StatusCode(500, "An error occurred while retrieving the audit log");
        }
    }
}

public class CreateFiscalYearRequest
{
    public int FiscalYear { get; set; }
    public int StartMonth { get; set; } = 1;
}
