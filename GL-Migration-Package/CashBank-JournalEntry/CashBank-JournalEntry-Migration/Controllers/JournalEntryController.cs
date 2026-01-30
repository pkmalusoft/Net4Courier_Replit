using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Truebooks.Platform.Contracts.Legacy.Models;
using VoucherTransactionType = Truebooks.Platform.Contracts.Enums.VoucherTransactionType;
using Truebooks.Platform.Core.Infrastructure;
using Truebooks.Platform.Core.MultiTenancy;
using Truebooks.Shared.UI.Services;

namespace Truebooks.Platform.Host.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JournalEntryController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IVoucherNumberingService _voucherNumberingService;

    public JournalEntryController(
        PlatformDbContext context,
        ITenantContext tenantContext,
        IVoucherNumberingService voucherNumberingService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _voucherNumberingService = voucherNumberingService;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context not available");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JournalEntryDto>>> GetAll(
        [FromQuery] int? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] bool includeVoided = false)
    {
        var tenantId = GetCurrentTenantId();
        var query = _context.JournalEntries
            .Where(j => j.TenantId == tenantId)
            .Include(j => j.Lines)
            .AsQueryable();

        if (!includeVoided)
        {
            query = query.Where(j => !j.IsVoided);
        }

        if (status.HasValue)
        {
            query = query.Where(j => (int)j.Status == status.Value);
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

        var entries = await query
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var accountIds = entries.SelectMany(e => e.Lines).Select(l => l.AccountId).Distinct().ToList();
        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && accountIds.Contains(a.Id))
            .AsNoTracking()
            .ToDictionaryAsync(a => a.Id);

        var result = entries.Select(e => new JournalEntryDto
        {
            Id = e.Id,
            EntryNumber = e.EntryNumber,
            EntryDate = e.EntryDate,
            Description = e.Description,
            Status = e.Status.ToString(),
            BranchId = e.BranchId,
            DepartmentId = e.DepartmentId,
            IsVoided = e.IsVoided,
            VoidReason = e.VoidReason,
            Lines = e.Lines.Select(l => new JournalEntryLineDto
            {
                Id = l.Id,
                AccountId = l.AccountId,
                AccountCode = accounts.TryGetValue(l.AccountId, out var acc) ? acc.AccountCode : null,
                AccountName = accounts.TryGetValue(l.AccountId, out var accName) ? accName.AccountName : null,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description
            }).ToList()
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JournalEntryDto>> GetById(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var entry = await _context.JournalEntries
            .Where(j => j.TenantId == tenantId && j.Id == id)
            .Include(j => j.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (entry == null)
        {
            return NotFound();
        }

        var accountIds = entry.Lines.Select(l => l.AccountId).Distinct().ToList();
        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && accountIds.Contains(a.Id))
            .AsNoTracking()
            .ToDictionaryAsync(a => a.Id);

        return Ok(new JournalEntryDto
        {
            Id = entry.Id,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            Description = entry.Description,
            Status = entry.Status.ToString(),
            BranchId = entry.BranchId,
            DepartmentId = entry.DepartmentId,
            IsVoided = entry.IsVoided,
            VoidReason = entry.VoidReason,
            Lines = entry.Lines.Select(l => new JournalEntryLineDto
            {
                Id = l.Id,
                AccountId = l.AccountId,
                AccountCode = accounts.TryGetValue(l.AccountId, out var acc) ? acc.AccountCode : null,
                AccountName = accounts.TryGetValue(l.AccountId, out var accName) ? accName.AccountName : null,
                Debit = l.Debit,
                Credit = l.Credit,
                Description = l.Description
            }).ToList()
        });
    }

    [HttpPost]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<JournalEntryDto>> Create([FromBody] CreateJournalEntryRequest request)
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

        var entryNumber = await _voucherNumberingService.GetNextVoucherNumberAsync(VoucherTransactionType.JournalEntry);

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
                Description = l.Description,
                FiscalYear = request.EntryDate.Year
            }).ToList()
        };

        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();

        var response = new JournalEntryDto
        {
            Id = entry.Id,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            Description = entry.Description,
            Status = entry.Status.ToString(),
            BranchId = entry.BranchId,
            DepartmentId = entry.DepartmentId,
            Lines = entry.Lines.Select(l => new JournalEntryLineDto
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
            Description = l.Description,
            FiscalYear = request.EntryDate.Year
        }).ToList();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/void")]
    [Authorize(Roles = "owner,admin")]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidTransactionRequest request)
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
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            entry.VoidedByUserId = userId;
        }
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

        var accountIds = entry.Lines.Select(l => l.AccountId).Distinct().ToList();
        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && accountIds.Contains(a.Id))
            .AsNoTracking()
            .ToDictionaryAsync(a => a.Id);

        foreach (var line in entry.Lines)
        {
            if (accounts.TryGetValue(line.AccountId, out var account) && !account.AllowPosting)
            {
                return BadRequest($"Account {account.AccountCode} does not allow posting");
            }
        }

        entry.Status = JournalEntryStatus.Posted;
        entry.PostedDate = DateTime.UtcNow;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            entry.PostedByUserId = userId;
        }

        await _context.SaveChangesAsync();

        return Ok(new JournalEntryDto
        {
            Id = entry.Id,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            Description = entry.Description,
            Status = entry.Status.ToString(),
            BranchId = entry.BranchId,
            DepartmentId = entry.DepartmentId,
            Lines = entry.Lines.Select(l => new JournalEntryLineDto
            {
                Id = l.Id,
                AccountId = l.AccountId,
                AccountCode = accounts.TryGetValue(l.AccountId, out var acc) ? acc.AccountCode : null,
                AccountName = accounts.TryGetValue(l.AccountId, out var accName) ? accName.AccountName : null,
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
        var tenantId = GetCurrentTenantId();
        var entry = await _context.JournalEntries.FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId);

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
        var tenantId = GetCurrentTenantId();
        var entry = await _context.JournalEntries.FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId);

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
    public async Task<ActionResult> GetTrialBalance([FromQuery] DateTime? asOfDate = null)
    {
        var tenantId = GetCurrentTenantId();
        var targetDate = asOfDate ?? DateTime.UtcNow;

        var postedEntries = await _context.JournalEntries
            .Where(j => j.TenantId == tenantId && j.Status == JournalEntryStatus.Posted && j.EntryDate <= targetDate && !j.IsVoided)
            .Include(j => j.Lines)
            .AsNoTracking()
            .ToListAsync();

        var accountIds = postedEntries.SelectMany(j => j.Lines).Select(l => l.AccountId).Distinct().ToList();
        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && accountIds.Contains(a.Id))
            .AsNoTracking()
            .ToDictionaryAsync(a => a.Id);

        var accountBalances = postedEntries
            .SelectMany(j => j.Lines)
            .GroupBy(l => l.AccountId)
            .Select(g =>
            {
                accounts.TryGetValue(g.Key, out var account);
                return new
                {
                    AccountId = g.Key,
                    AccountCode = account?.AccountCode,
                    AccountName = account?.AccountName,
                    AccountType = account?.AccountType.ToString(),
                    TotalDebit = g.Sum(l => l.Debit),
                    TotalCredit = g.Sum(l => l.Credit),
                    Balance = g.Sum(l => l.Debit - l.Credit)
                };
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

public class JournalEntryDto
{
    public Guid Id { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsVoided { get; set; }
    public string? VoidReason { get; set; }
    public List<JournalEntryLineDto> Lines { get; set; } = new();
}

public class JournalEntryLineDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CreateJournalEntryRequest
{
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    [Required]
    public List<CreateJournalEntryLineRequest> Lines { get; set; } = new();
}

public class CreateJournalEntryLineRequest
{
    [Required]
    public Guid AccountId { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}

public class UpdateJournalEntryRequest
{
    public Guid? Id { get; set; }

    public string? EntryNumber { get; set; }

    public DateTime EntryDate { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    [Required]
    public List<UpdateJournalEntryLineRequest> Lines { get; set; } = new();
}

public class UpdateJournalEntryLineRequest
{
    public Guid? Id { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}

public class VoidTransactionRequest
{
    [MaxLength(500)]
    public string VoidReason { get; set; } = string.Empty;
}
