using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.BankReconciliation.Models;

namespace Server.Modules.BankReconciliation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BankAccountController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public BankAccountController(AppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.CurrentTenantId ?? throw new UnauthorizedAccessException("Tenant context not available");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BankAccount>>> GetAll()
    {
        var tenantId = GetCurrentTenantId();
        return await _context.BankAccounts
            .Include(b => b.ChartOfAccount)
            .Include(b => b.Currency)
            .Where(b => b.TenantId == tenantId && b.IsActive)
            .OrderBy(b => b.AccountName)
            .ToListAsync();
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<BankAccount>>> GetActive()
    {
        var tenantId = GetCurrentTenantId();
        return await _context.BankAccounts
            .Include(b => b.ChartOfAccount)
            .Include(b => b.Currency)
            .Where(b => b.TenantId == tenantId && b.IsActive)
            .OrderBy(b => b.AccountName)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankAccount>> GetById(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var bankAccount = await _context.BankAccounts
            .Include(b => b.ChartOfAccount)
            .Include(b => b.Currency)
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);
        
        if (bankAccount == null)
        {
            return NotFound();
        }

        return bankAccount;
    }

    [HttpGet("validate/account-number-exists")]
    public async Task<ActionResult<bool>> AccountNumberExists([FromQuery] string accountNumber, [FromQuery] Guid? excludeId = null)
    {
        var tenantId = GetCurrentTenantId();
        var exists = await _context.BankAccounts
            .AnyAsync(b => b.TenantId == tenantId && b.AccountNumber == accountNumber && b.Id != (excludeId ?? Guid.Empty));
        return Ok(exists);
    }

    [HttpPost]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<BankAccount>> Create([FromBody] BankAccount bankAccount)
    {
        var tenantId = GetCurrentTenantId();
        
        // CRITICAL: Force TenantId to authenticated tenant (prevent tenant-escape)
        bankAccount.TenantId = tenantId;
        
        // Check for duplicate account number within tenant
        if (await _context.BankAccounts.AnyAsync(b => b.TenantId == tenantId && b.AccountNumber == bankAccount.AccountNumber))
        {
            return BadRequest($"Bank account with account number '{bankAccount.AccountNumber}' already exists in this tenant");
        }

        // Validate ChartOfAccount exists and belongs to tenant
        if (!await _context.ChartOfAccounts.AnyAsync(c => c.Id == bankAccount.ChartOfAccountId && c.TenantId == tenantId))
        {
            return BadRequest("Chart of Account not found or does not belong to this tenant");
        }

        // Validate Currency if provided
        if (bankAccount.CurrencyId.HasValue && !await _context.Currencies.AnyAsync(c => c.Id == bankAccount.CurrencyId))
        {
            return BadRequest("Currency not found");
        }

        // Fix: Ensure OpeningBalanceDate is UTC
        if (bankAccount.OpeningBalanceDate.Kind != DateTimeKind.Utc)
        {
            bankAccount.OpeningBalanceDate = DateTime.SpecifyKind(bankAccount.OpeningBalanceDate, DateTimeKind.Utc);
        }

        _context.BankAccounts.Add(bankAccount);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = bankAccount.Id }, bankAccount);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BankAccount updatedBankAccount)
    {
        var tenantId = GetCurrentTenantId();

        var existingBankAccount = await _context.BankAccounts.FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);
        if (existingBankAccount == null)
        {
            return NotFound();
        }

        // CRITICAL: Prevent tenant-escape by forcing TenantId
        updatedBankAccount.TenantId = tenantId;
        updatedBankAccount.Id = id;

        // Check for duplicate account number (excluding current record)
        if (await _context.BankAccounts.AnyAsync(b => b.TenantId == tenantId && b.AccountNumber == updatedBankAccount.AccountNumber && b.Id != id))
        {
            return BadRequest($"Bank account with account number '{updatedBankAccount.AccountNumber}' already exists in this tenant");
        }

        // Validate ChartOfAccount exists and belongs to tenant
        if (!await _context.ChartOfAccounts.AnyAsync(c => c.Id == updatedBankAccount.ChartOfAccountId && c.TenantId == tenantId))
        {
            return BadRequest("Chart of Account not found or does not belong to this tenant");
        }

        // Validate Currency if provided
        if (updatedBankAccount.CurrencyId.HasValue && !await _context.Currencies.AnyAsync(c => c.Id == updatedBankAccount.CurrencyId))
        {
            return BadRequest("Currency not found");
        }

        // Update properties
        existingBankAccount.AccountNumber = updatedBankAccount.AccountNumber;
        existingBankAccount.AccountName = updatedBankAccount.AccountName;
        existingBankAccount.BankName = updatedBankAccount.BankName;
        existingBankAccount.BranchName = updatedBankAccount.BranchName;
        existingBankAccount.SwiftCode = updatedBankAccount.SwiftCode;
        existingBankAccount.IbanNumber = updatedBankAccount.IbanNumber;
        existingBankAccount.ChartOfAccountId = updatedBankAccount.ChartOfAccountId;
        existingBankAccount.CurrencyId = updatedBankAccount.CurrencyId;
        existingBankAccount.OpeningBalance = updatedBankAccount.OpeningBalance;
        
        // Fix: Ensure OpeningBalanceDate is UTC
        existingBankAccount.OpeningBalanceDate = updatedBankAccount.OpeningBalanceDate.Kind == DateTimeKind.Utc
            ? updatedBankAccount.OpeningBalanceDate
            : DateTime.SpecifyKind(updatedBankAccount.OpeningBalanceDate, DateTimeKind.Utc);
        
        existingBankAccount.IsActive = updatedBankAccount.IsActive;
        existingBankAccount.Notes = updatedBankAccount.Notes;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "owner,admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetCurrentTenantId();

        var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);
        if (bankAccount == null)
        {
            return NotFound();
        }

        // Check if bank account has reconciliations
        var hasReconciliations = await _context.BankReconciliations.AnyAsync(r => r.BankAccountId == id && r.TenantId == tenantId);
        if (hasReconciliations)
        {
            return BadRequest("Cannot delete bank account with existing reconciliations. Consider marking it as inactive instead.");
        }

        // Check if bank account has transactions
        var hasTransactions = await _context.CashBankTransactions.AnyAsync(t => t.BankAccountId == id && t.TenantId == tenantId);
        if (hasTransactions)
        {
            return BadRequest("Cannot delete bank account with existing transactions. Consider marking it as inactive instead.");
        }

        _context.BankAccounts.Remove(bankAccount);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/current-balance")]
    public async Task<ActionResult<decimal>> GetCurrentBalance(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        
        var bankAccount = await _context.BankAccounts.FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);
        if (bankAccount == null)
        {
            return NotFound();
        }

        // Calculate current balance: opening balance + sum of posted transactions
        var transactionSum = await _context.CashBankTransactions
            .Where(t => t.BankAccountId == id && t.TenantId == tenantId && t.Status == Server.Modules.CashBank.Models.CashBankStatus.Posted)
            .SumAsync(t => t.RecPayType == Server.Modules.CashBank.Models.RecPayType.Receipt ? t.TotalAmount : -t.TotalAmount);

        var currentBalance = bankAccount.OpeningBalance + transactionSum;
        
        return Ok(currentBalance);
    }
}
