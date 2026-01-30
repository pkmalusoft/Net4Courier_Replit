using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Core.Infrastructure;
using Truebooks.Platform.Core.MultiTenancy;

namespace Truebooks.Platform.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankAccountController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly ITenantContext _tenantContext;

    public BankAccountController(PlatformDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<BankAccountDto>>> GetAll()
    {
        var tenantId = _tenantContext.TenantId;
        var accounts = await _context.BankAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .Include(a => a.Currency)
            .OrderBy(a => a.AccountName)
            .Select(a => new BankAccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                AccountName = a.AccountName,
                BankName = a.BankName,
                BranchName = a.BranchName,
                CurrencyId = a.CurrencyId,
                CurrencyCode = a.Currency != null ? a.Currency.Code ?? "" : "",
                OpeningBalance = a.OpeningBalance,
                IsActive = a.IsActive
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankAccountDto>> GetById(Guid id)
    {
        var tenantId = _tenantContext.TenantId;
        var a = await _context.BankAccounts
            .Where(x => x.TenantId == tenantId && x.Id == id)
            .Include(x => x.Currency)
            .FirstOrDefaultAsync();

        if (a == null)
            return NotFound();

        return Ok(new BankAccountDto
        {
            Id = a.Id,
            AccountNumber = a.AccountNumber,
            AccountName = a.AccountName,
            BankName = a.BankName,
            BranchName = a.BranchName,
            SwiftCode = a.SwiftCode,
            IbanNumber = a.IbanNumber,
            ChartOfAccountId = a.ChartOfAccountId,
            CurrencyId = a.CurrencyId,
            CurrencyCode = a.Currency?.Code ?? "",
            OpeningBalance = a.OpeningBalance,
            OpeningBalanceDate = a.OpeningBalanceDate,
            IsActive = a.IsActive,
            Notes = a.Notes
        });
    }

    [HttpPost]
    public async Task<ActionResult<BankAccountDto>> Create([FromBody] CreateBankAccountRequest dto)
    {
        var tenantIdValue = _tenantContext.TenantId ?? Guid.Empty;
        if (tenantIdValue == Guid.Empty)
            return Unauthorized();

        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantIdValue,
            AccountNumber = dto.AccountNumber,
            AccountName = dto.AccountName,
            BankName = dto.BankName,
            BranchName = dto.BranchName,
            SwiftCode = dto.SwiftCode,
            IbanNumber = dto.IbanNumber,
            ChartOfAccountId = dto.ChartOfAccountId,
            CurrencyId = dto.CurrencyId,
            OpeningBalance = dto.OpeningBalance,
            OpeningBalanceDate = dto.OpeningBalanceDate,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        return Ok(new BankAccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            BankName = account.BankName,
            IsActive = account.IsActive
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BankAccountDto>> Update(Guid id, [FromBody] CreateBankAccountRequest dto)
    {
        var tenantIdValue = _tenantContext.TenantId ?? Guid.Empty;
        if (tenantIdValue == Guid.Empty)
            return Unauthorized();

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantIdValue && a.Id == id);

        if (account == null)
            return NotFound();

        account.AccountNumber = dto.AccountNumber;
        account.AccountName = dto.AccountName;
        account.BankName = dto.BankName;
        account.BranchName = dto.BranchName;
        account.SwiftCode = dto.SwiftCode;
        account.IbanNumber = dto.IbanNumber;
        account.ChartOfAccountId = dto.ChartOfAccountId;
        account.CurrencyId = dto.CurrencyId;
        account.OpeningBalance = dto.OpeningBalance;
        account.OpeningBalanceDate = dto.OpeningBalanceDate;
        account.Notes = dto.Notes;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new BankAccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber,
            AccountName = account.AccountName,
            BankName = account.BankName,
            IsActive = account.IsActive
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = _tenantContext.TenantId;
        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == id);

        if (account == null)
            return NotFound();

        account.IsActive = false;
        account.DeactivatedDate = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Bank account deactivated" });
    }
}

public class CreateBankAccountRequest
{
    public string AccountNumber { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string BankName { get; set; } = "";
    public string? BranchName { get; set; }
    public string? SwiftCode { get; set; }
    public string? IbanNumber { get; set; }
    public Guid ChartOfAccountId { get; set; }
    public Guid? CurrencyId { get; set; }
    public decimal OpeningBalance { get; set; }
    public DateTime OpeningBalanceDate { get; set; }
    public string? Notes { get; set; }
}

public class BankAccountDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string BankName { get; set; } = "";
    public string? BranchName { get; set; }
    public string? SwiftCode { get; set; }
    public string? IbanNumber { get; set; }
    public Guid ChartOfAccountId { get; set; }
    public Guid? CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = "";
    public decimal OpeningBalance { get; set; }
    public DateTime OpeningBalanceDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
