using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truebooks.Platform.Core.Infrastructure;
using Truebooks.Platform.Core.MultiTenancy;

namespace Truebooks.Platform.Host.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CashBankRegisterController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CashBankRegisterController> _logger;

    public CashBankRegisterController(
        PlatformDbContext context,
        ITenantContext tenantContext,
        ILogger<CashBankRegisterController> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    [HttpGet("accounts")]
    public ActionResult<List<CashBankAccountDto>> GetAccounts()
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == null || tenantId == Guid.Empty)
            return Unauthorized();

        return Ok(new List<CashBankAccountDto>());
    }

    [HttpGet("register")]
    public ActionResult<CashBankRegisterResultDto> GetRegister(
        [FromQuery] Guid? accountId,
        [FromQuery] string? transactionType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == null || tenantId == Guid.Empty)
            return Unauthorized();

        return Ok(new CashBankRegisterResultDto
        {
            AccountName = "All Accounts",
            FromDate = fromDate ?? DateTime.Today.AddMonths(-1),
            ToDate = toDate ?? DateTime.Today,
            OpeningBalance = 0,
            TotalReceipts = 0,
            TotalPayments = 0,
            ClosingBalance = 0,
            Transactions = new List<CashBankRegisterItemDto>()
        });
    }
}

public class CashBankAccountDto
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string AccountType { get; set; } = "";
    public decimal CurrentBalance { get; set; }
}

public class CashBankRegisterResultDto
{
    public string AccountName { get; set; } = "";
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal TotalReceipts { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<CashBankRegisterItemDto> Transactions { get; set; } = new();
}

public class CashBankRegisterItemDto
{
    public DateTime Date { get; set; }
    public string TransactionNo { get; set; } = "";
    public string TransactionType { get; set; } = "";
    public string Description { get; set; } = "";
    public string Reference { get; set; } = "";
    public decimal Receipts { get; set; }
    public decimal Payments { get; set; }
    public decimal Balance { get; set; }
}
