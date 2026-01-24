using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;

namespace Server.Modules.GeneralLedger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class YearEndClosingController : ControllerBase
{
    private readonly IYearEndClosingService _yearEndClosingService;
    private readonly IControlAccountReconciliationService _reconciliationService;
    private readonly ILogger<YearEndClosingController> _logger;

    public YearEndClosingController(
        IYearEndClosingService yearEndClosingService,
        IControlAccountReconciliationService reconciliationService,
        ILogger<YearEndClosingController> logger)
    {
        _yearEndClosingService = yearEndClosingService;
        _reconciliationService = reconciliationService;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Invalid tenant ID");
        }
        return tenantId;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        return userId;
    }

    [HttpGet]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<List<YearEndClosingDto>>> GetAllClosings()
    {
        var tenantId = GetCurrentTenantId();
        var closings = await _yearEndClosingService.GetAllClosingsAsync(tenantId);
        
        var dtos = closings.Select(c => new YearEndClosingDto
        {
            Id = c.Id,
            ClosingNumber = c.ClosingNumber,
            FinancialPeriodId = c.FinancialPeriodId,
            PeriodName = c.FinancialPeriod?.PeriodName ?? "",
            FiscalYear = c.FinancialPeriod?.FiscalYear ?? 0,
            Status = c.Status,
            StatusName = c.Status.ToString(),
            ValidationDate = c.ValidationDate,
            ClosingDate = c.ClosingDate,
            TotalRevenue = c.TotalRevenue,
            TotalExpense = c.TotalExpense,
            NetIncomeOrLoss = c.NetIncomeOrLoss,
            CreatedAt = c.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<YearEndClosingDto>> GetClosing(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var closing = await _yearEndClosingService.GetClosingByIdAsync(id, tenantId);
        
        if (closing == null)
            return NotFound();

        var dto = new YearEndClosingDto
        {
            Id = closing.Id,
            ClosingNumber = closing.ClosingNumber,
            FinancialPeriodId = closing.FinancialPeriodId,
            PeriodName = closing.FinancialPeriod?.PeriodName ?? "",
            FiscalYear = closing.FinancialPeriod?.FiscalYear ?? 0,
            Status = closing.Status,
            StatusName = closing.Status.ToString(),
            ValidationDate = closing.ValidationDate,
            ClosingDate = closing.ClosingDate,
            TotalRevenue = closing.TotalRevenue,
            TotalExpense = closing.TotalExpense,
            NetIncomeOrLoss = closing.NetIncomeOrLoss,
            ARControlBalance = closing.ARControlBalance,
            ARSubledgerBalance = closing.ARSubledgerBalance,
            ARVariance = closing.ARVariance,
            APControlBalance = closing.APControlBalance,
            APSubledgerBalance = closing.APSubledgerBalance,
            APVariance = closing.APVariance,
            InventoryControlBalance = closing.InventoryControlBalance,
            InventorySubledgerBalance = closing.InventorySubledgerBalance,
            InventoryVariance = closing.InventoryVariance,
            ValidationErrors = closing.ValidationErrors,
            Notes = closing.Notes,
            CreatedAt = closing.CreatedAt
        };

        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<YearEndClosingDto>> CreateClosing([FromBody] CreateClosingRequest request)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var closing = await _yearEndClosingService.CreateClosingAsync(request.PeriodId, tenantId, userId);
            
            return Ok(new YearEndClosingDto
            {
                Id = closing.Id,
                ClosingNumber = closing.ClosingNumber,
                FinancialPeriodId = closing.FinancialPeriodId,
                Status = closing.Status,
                StatusName = closing.Status.ToString(),
                CreatedAt = closing.CreatedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/stage1-validate")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<YearEndClosingValidationResult>> Stage1Validate(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var result = await _yearEndClosingService.Stage1_ValidateAsync(id, tenantId, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/stage2-close-income-expense")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<YearEndClosingDto>> Stage2CloseIncomeExpense(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var closing = await _yearEndClosingService.Stage2_CloseIncomeExpenseAsync(id, tenantId, userId);
            return Ok(MapToDto(closing));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/stage3-snapshot-subledgers")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<YearEndClosingDto>> Stage3SnapshotSubledgers(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var closing = await _yearEndClosingService.Stage3_SnapshotSubledgersAsync(id, tenantId, userId);
            return Ok(MapToDto(closing));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/stage4-snapshot-inventory")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<YearEndClosingDto>> Stage4SnapshotInventory(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var closing = await _yearEndClosingService.Stage4_SnapshotInventoryAsync(id, tenantId, userId);
            return Ok(MapToDto(closing));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/stage5-generate-opening-je")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<YearEndClosingDto>> Stage5GenerateOpeningJE(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var closing = await _yearEndClosingService.Stage5_GenerateOpeningJEAsync(id, tenantId, userId);
            return Ok(MapToDto(closing));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<YearEndClosingDto>> CompleteClosing(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var closing = await _yearEndClosingService.CompleteClosingAsync(id, tenantId, userId);
            return Ok(MapToDto(closing));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/rollback")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<YearEndClosingDto>> RollbackClosing(Guid id, [FromBody] RollbackRequest request)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var userId = GetCurrentUserId();
            
            var closing = await _yearEndClosingService.RollbackClosingAsync(id, tenantId, userId, request.Reason);
            return Ok(MapToDto(closing));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("reconciliation")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<ControlAccountReconciliationResult>> GetReconciliation([FromQuery] DateTime? asOfDate)
    {
        var tenantId = GetCurrentTenantId();
        var date = asOfDate ?? DateTime.UtcNow.Date;
        
        var result = await _reconciliationService.ReconcileAllControlAccountsAsync(tenantId, date);
        return Ok(result);
    }

    private YearEndClosingDto MapToDto(Server.Modules.GeneralLedger.Models.YearEndClosing closing)
    {
        return new YearEndClosingDto
        {
            Id = closing.Id,
            ClosingNumber = closing.ClosingNumber,
            FinancialPeriodId = closing.FinancialPeriodId,
            PeriodName = closing.FinancialPeriod?.PeriodName ?? "",
            FiscalYear = closing.FinancialPeriod?.FiscalYear ?? 0,
            Status = closing.Status,
            StatusName = closing.Status.ToString(),
            ValidationDate = closing.ValidationDate,
            ClosingDate = closing.ClosingDate,
            TotalRevenue = closing.TotalRevenue,
            TotalExpense = closing.TotalExpense,
            NetIncomeOrLoss = closing.NetIncomeOrLoss,
            ARControlBalance = closing.ARControlBalance,
            ARSubledgerBalance = closing.ARSubledgerBalance,
            ARVariance = closing.ARVariance,
            APControlBalance = closing.APControlBalance,
            APSubledgerBalance = closing.APSubledgerBalance,
            APVariance = closing.APVariance,
            InventoryControlBalance = closing.InventoryControlBalance,
            InventorySubledgerBalance = closing.InventorySubledgerBalance,
            InventoryVariance = closing.InventoryVariance,
            ValidationErrors = closing.ValidationErrors,
            Notes = closing.Notes,
            CreatedAt = closing.CreatedAt
        };
    }
}

public class YearEndClosingDto
{
    public Guid Id { get; set; }
    public string ClosingNumber { get; set; } = string.Empty;
    public Guid FinancialPeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public int FiscalYear { get; set; }
    public YearEndClosingStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? ValidationDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetIncomeOrLoss { get; set; }
    public decimal ARControlBalance { get; set; }
    public decimal ARSubledgerBalance { get; set; }
    public decimal ARVariance { get; set; }
    public decimal APControlBalance { get; set; }
    public decimal APSubledgerBalance { get; set; }
    public decimal APVariance { get; set; }
    public decimal InventoryControlBalance { get; set; }
    public decimal InventorySubledgerBalance { get; set; }
    public decimal InventoryVariance { get; set; }
    public string? ValidationErrors { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateClosingRequest
{
    public Guid PeriodId { get; set; }
}

public class RollbackRequest
{
    public string Reason { get; set; } = string.Empty;
}
