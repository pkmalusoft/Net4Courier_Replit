using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Modules.GeneralLedger.Models;
using Server.Modules.Administration.Services;
using Shared.Enums;
using System.Security.Claims;

namespace Server.Modules.GeneralLedger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JournalEntryController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IVoucherNumberingService _voucherNumberingService;

    public JournalEntryController(
        AppDbContext context, 
        ITenantProvider tenantProvider,
        IVoucherNumberingService voucherNumberingService)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _voucherNumberingService = voucherNumberingService;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.CurrentTenantId ?? throw new UnauthorizedAccessException("Tenant context not available");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JournalEntry>>> GetAll(
        [FromQuery] JournalEntryStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] bool includeVoided = false)
    {
        var query = _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Currency)
            .AsQueryable();

        if (!includeVoided)
        {
            query = query.Where(j => !j.IsVoided);
        }

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            var fromDateUtc = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
            query = query.Where(j => j.EntryDate >= fromDateUtc);
        }

        if (toDate.HasValue)
        {
            var toDateUtc = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
            query = query.Where(j => j.EntryDate <= toDateUtc);
        }

        return await query
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JournalEntry>> GetById(Guid id)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Currency)
            .FirstOrDefaultAsync(j => j.Id == id);
        
        if (entry == null)
        {
            return NotFound();
        }

        return entry;
    }

    [HttpPost]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<JournalEntryResponse>> Create([FromBody] CreateJournalEntryRequest request)
    {
        var tenantId = GetCurrentTenantId();
        
        if (request.Lines == null || !request.Lines.Any())
        {
            return BadRequest("Journal entry must have at least one line");
        }

        var totalDebit = request.Lines.Sum(l => l.Debit);
        var totalCredit = request.Lines.Sum(l => l.Credit);

        if (Math.Abs(totalDebit - totalCredit) > 0.01m)
        {
            return BadRequest($"Journal entry is not balanced. Debit: {totalDebit}, Credit: {totalCredit}");
        }

        foreach (var line in request.Lines)
        {
            if (line.Debit > 0 && line.Credit > 0)
            {
                return BadRequest("A line cannot have both debit and credit amounts");
            }

            if (line.Debit == 0 && line.Credit == 0)
            {
                return BadRequest("A line must have either debit or credit amount");
            }
        }

        var entryNumber = await _voucherNumberingService.GenerateNumberAsync(VoucherTransactionType.JournalEntry, tenantId);

        var entry = new JournalEntry
        {
            TenantId = tenantId,
            EntryNumber = entryNumber,
            EntryDate = DateTime.SpecifyKind(request.EntryDate, DateTimeKind.Utc),
            Description = request.Description,
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId,
            Status = JournalEntryStatus.Draft,
            Source = JournalEntrySource.Manual,
            Lines = request.Lines.Select(l => new JournalEntryLine
            {
                TenantId = tenantId,
                AccountId = l.AccountId,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description
            }).ToList()
        };

        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();

        var response = new JournalEntryResponse
        {
            Id = entry.Id,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            Description = entry.Description,
            Status = entry.Status.ToString(),
            BranchId = entry.BranchId,
            DepartmentId = entry.DepartmentId,
            Lines = entry.Lines.Select(l => new JournalEntryLineResponse
            {
                Id = l.Id,
                AccountId = l.AccountId,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description
            }).ToList()
        };

        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJournalEntryRequest request)
    {
        var tenantId = GetCurrentTenantId();
        
        var existing = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId);

        if (existing == null)
        {
            return NotFound();
        }

        if (existing.Status == JournalEntryStatus.Posted)
        {
            return BadRequest("Cannot modify posted journal entry. Unpost first.");
        }

        if (request.Lines == null || !request.Lines.Any())
        {
            return BadRequest("Journal entry must have at least one line");
        }

        var totalDebit = request.Lines.Sum(l => l.Debit);
        var totalCredit = request.Lines.Sum(l => l.Credit);

        if (Math.Abs(totalDebit - totalCredit) > 0.01m)
        {
            return BadRequest($"Journal entry is not balanced. Debit: {totalDebit}, Credit: {totalCredit}");
        }

        foreach (var line in request.Lines)
        {
            if (line.Debit > 0 && line.Credit > 0)
            {
                return BadRequest("A line cannot have both debit and credit amounts");
            }

            if (line.Debit == 0 && line.Credit == 0)
            {
                return BadRequest("A line must have either debit or credit amount");
            }
        }

        _context.JournalEntryLines.RemoveRange(existing.Lines);
        
        existing.EntryDate = DateTime.SpecifyKind(request.EntryDate, DateTimeKind.Utc);
        existing.Description = request.Description;
        existing.BranchId = request.BranchId;
        existing.DepartmentId = request.DepartmentId;
        existing.Lines = request.Lines.Select(l => new JournalEntryLine
        {
            TenantId = tenantId,
            AccountId = l.AccountId,
            Debit = l.Debit,
            Credit = l.Credit,
            Description = l.Description
        }).ToList();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/void")]
    [Authorize(Roles = "owner,admin")]
    public async Task<IActionResult> Void(Guid id, [FromBody] Server.DTOs.VoidTransactionRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var entry = await _context.JournalEntries
            .FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId);

        if (entry == null)
        {
            return NotFound();
        }

        if (entry.Status != JournalEntryStatus.Posted)
        {
            return BadRequest("Only posted journal entries can be voided");
        }

        if (entry.IsVoided)
        {
            return BadRequest("Journal entry is already voided");
        }

        entry.IsVoided = true;
        entry.VoidedDate = DateTime.UtcNow;
        entry.VoidedByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        entry.VoidReason = request.VoidReason;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Journal entry voided successfully", entryNumber = entry.EntryNumber });
    }

    [HttpPost("{id}/post")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<IActionResult> Post(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId);

        if (entry == null)
        {
            return NotFound("Journal entry not found");
        }

        if (entry.Status == JournalEntryStatus.Posted)
        {
            return BadRequest("Journal entry is already posted");
        }

        if (entry.Status == JournalEntryStatus.Cancelled)
        {
            return BadRequest("Cannot post cancelled journal entry");
        }

        if (!ValidateJournalEntry(entry, out var validationError))
        {
            return BadRequest(validationError);
        }

        foreach (var line in entry.Lines)
        {
            if (!line.Account.AllowPosting)
            {
                return BadRequest($"Account {line.Account.AccountCode} does not allow posting");
            }
        }

        entry.Status = JournalEntryStatus.Posted;
        entry.PostedDate = DateTime.UtcNow;
        entry.PostedByUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _context.SaveChangesAsync();

        return Ok(new JournalEntryResponse
        {
            Id = entry.Id,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            Description = entry.Description,
            Status = entry.Status.ToString(),
            BranchId = entry.BranchId,
            DepartmentId = entry.DepartmentId,
            Lines = entry.Lines.Select(l => new JournalEntryLineResponse
            {
                Id = l.Id,
                AccountId = l.AccountId,
                AccountCode = l.Account?.AccountCode,
                AccountName = l.Account?.AccountName,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description
            }).ToList()
        });
    }

    [HttpPost("{id}/unpost")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<IActionResult> Unpost(Guid id)
    {
        var entry = await _context.JournalEntries.FindAsync(id);

        if (entry == null)
        {
            return NotFound();
        }

        if (entry.Status != JournalEntryStatus.Posted)
        {
            return BadRequest("Only posted journal entries can be unposted");
        }

        entry.Status = JournalEntryStatus.Draft;
        entry.PostedDate = null;
        entry.PostedByUserId = null;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Journal entry unposted successfully" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "owner,admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        
        if (entry == null)
        {
            return NotFound();
        }

        if (entry.Status == JournalEntryStatus.Posted)
        {
            return BadRequest("Cannot delete posted journal entry. Unpost first.");
        }

        entry.Status = JournalEntryStatus.Cancelled;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("trial-balance")]
    [Authorize]
    public async Task<ActionResult> GetTrialBalance(
        [FromQuery] DateTime? asOfDate = null)
    {
        var targetDate = asOfDate ?? DateTime.UtcNow;

        var postedEntries = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.Status == JournalEntryStatus.Posted && j.EntryDate <= targetDate && !j.IsVoided)
            .ToListAsync();

        var accountBalances = postedEntries
            .SelectMany(j => j.Lines)
            .GroupBy(l => new { l.AccountId, l.Account.AccountCode, l.Account.AccountName, l.Account.AccountType })
            .Select(g => new
            {
                AccountId = g.Key.AccountId,
                AccountCode = g.Key.AccountCode,
                AccountName = g.Key.AccountName,
                AccountType = g.Key.AccountType,
                TotalDebit = g.Sum(l => l.Debit),
                TotalCredit = g.Sum(l => l.Credit),
                Balance = g.Sum(l => l.Debit - l.Credit)
            })
            .OrderBy(a => a.AccountCode)
            .ToList();

        var totalDebit = accountBalances.Sum(a => a.TotalDebit);
        var totalCredit = accountBalances.Sum(a => a.TotalCredit);

        return Ok(new
        {
            AsOfDate = targetDate,
            Accounts = accountBalances,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            IsBalanced = Math.Abs(totalDebit - totalCredit) < 0.01m
        });
    }

    private bool ValidateJournalEntry(JournalEntry entry, out string error)
    {
        if (entry.Lines == null || !entry.Lines.Any())
        {
            error = "Journal entry must have at least one line";
            return false;
        }

        var totalDebit = entry.Lines.Sum(l => l.Debit);
        var totalCredit = entry.Lines.Sum(l => l.Credit);

        if (Math.Abs(totalDebit - totalCredit) > 0.01m)
        {
            error = $"Journal entry is not balanced. Debit: {totalDebit}, Credit: {totalCredit}";
            return false;
        }

        foreach (var line in entry.Lines)
        {
            if (line.Debit > 0 && line.Credit > 0)
            {
                error = "A line cannot have both debit and credit amounts";
                return false;
            }

            if (line.Debit == 0 && line.Credit == 0)
            {
                error = "A line must have either debit or credit amount";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }
}
