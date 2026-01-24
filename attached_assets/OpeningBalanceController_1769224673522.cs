using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Server.Modules.GeneralLedger.Controllers;

[Authorize(Policy = "CookieOrJwt")]
[ApiController]
[Route("api/[controller]")]
public class OpeningBalanceController : ControllerBase
{
    private readonly IOpeningBalanceService _openingBalanceService;
    private readonly ILogger<OpeningBalanceController> _logger;

    public OpeningBalanceController(
        IOpeningBalanceService openingBalanceService,
        ILogger<OpeningBalanceController> logger)
    {
        _openingBalanceService = openingBalanceService;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Invalid tenant context");
        }
        return tenantId;
    }

    [HttpGet("{fiscalYear}")]
    public async Task<ActionResult<List<OpeningBalanceDto>>> GetOpeningBalancesForFiscalYear(int fiscalYear)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var balances = await _openingBalanceService.GetOpeningBalancesForFiscalYearAsync(fiscalYear, tenantId);
            return Ok(balances);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting opening balances for fiscal year {FiscalYear}", fiscalYear);
            return StatusCode(500, "An error occurred while retrieving opening balances");
        }
    }

    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<decimal>> GetOpeningBalanceForAccount(Guid accountId, [FromQuery] DateTime asOfDate)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var balance = await _openingBalanceService.GetOpeningBalanceForAccountAsync(accountId, asOfDate, tenantId);
            return Ok(balance);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting opening balance for account {AccountId}", accountId);
            return StatusCode(500, "An error occurred while retrieving the opening balance");
        }
    }

    [HttpGet("retained-earnings/{fiscalYear}")]
    public async Task<ActionResult<decimal>> GetRetainedEarningsCarryForward(int fiscalYear)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var carryForward = await _openingBalanceService.GetRetainedEarningsCarryForwardAsync(fiscalYear, tenantId);
            return Ok(carryForward);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting retained earnings carry forward for fiscal year {FiscalYear}", fiscalYear);
            return StatusCode(500, "An error occurred while calculating retained earnings carry forward");
        }
    }

    [HttpPost("all")]
    public async Task<ActionResult<Dictionary<Guid, decimal>>> GetAllOpeningBalances([FromBody] OpeningBalanceRequestDto request)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var balances = await _openingBalanceService.GetAllOpeningBalancesAsync(request.AsOfDate, tenantId);
            return Ok(balances);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all opening balances");
            return StatusCode(500, "An error occurred while retrieving opening balances");
        }
    }
}

public class OpeningBalanceRequestDto
{
    public DateTime AsOfDate { get; set; }
}
