using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.BankReconciliation.Models;
using Server.Modules.CashBank.Models;
using Shared.DTOs;
using System.Security.Claims;
using System.Text;

namespace Server.Modules.BankReconciliation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BankReconciliationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IBankStatementImportService _importService;
    private readonly IBankReconciliationMatchingService _matchingService;

    public BankReconciliationController(
        AppDbContext context, 
        ITenantProvider tenantProvider,
        IBankStatementImportService importService,
        IBankReconciliationMatchingService matchingService)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _importService = importService;
        _matchingService = matchingService;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.CurrentTenantId ?? throw new UnauthorizedAccessException("Tenant context not available");
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReconciliationSummaryResponse>>> GetAll()
    {
        var tenantId = GetCurrentTenantId();
        
        var reconciliations = await _context.BankReconciliations
            .Include(r => r.BankAccount)
            .Include(r => r.StatementImports)
            .Include(r => r.Matches)
            .Include(r => r.Adjustments)
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.StatementDate)
            .Select(r => new ReconciliationSummaryResponse
            {
                Id = r.Id,
                ReconciliationNumber = r.ReconciliationNumber,
                StatementDate = r.StatementDate,
                Status = r.Status.ToString(),
                StatementOpeningBalance = r.StatementOpeningBalance,
                StatementClosingBalance = r.StatementClosingBalance,
                BookOpeningBalance = r.BookOpeningBalance,
                BookClosingBalance = r.BookClosingBalance,
                DifferenceAmount = r.DifferenceAmount,
                TotalStatementLines = r.StatementImports.SelectMany(i => i.StatementLines).Count(),
                MatchedLines = r.StatementImports.SelectMany(i => i.StatementLines).Count(l => l.IsMatched),
                UnmatchedLines = r.StatementImports.SelectMany(i => i.StatementLines).Count(l => !l.IsMatched),
                AdjustmentCount = r.Adjustments.Count
            })
            .ToListAsync();

        return Ok(reconciliations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReconciliationSummaryResponse>> GetById(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        
        var reconciliation = await _context.BankReconciliations
            .Include(r => r.BankAccount)
            .Include(r => r.StatementImports)
                .ThenInclude(i => i.StatementLines)
            .Include(r => r.Matches)
            .Include(r => r.Adjustments)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reconciliation == null)
            return NotFound();

        var summary = new ReconciliationSummaryResponse
        {
            Id = reconciliation.Id,
            ReconciliationNumber = reconciliation.ReconciliationNumber,
            StatementDate = reconciliation.StatementDate,
            Status = reconciliation.Status.ToString(),
            StatementOpeningBalance = reconciliation.StatementOpeningBalance,
            StatementClosingBalance = reconciliation.StatementClosingBalance,
            BookOpeningBalance = reconciliation.BookOpeningBalance,
            BookClosingBalance = reconciliation.BookClosingBalance,
            DifferenceAmount = reconciliation.DifferenceAmount,
            TotalStatementLines = reconciliation.StatementImports.SelectMany(i => i.StatementLines).Count(),
            MatchedLines = reconciliation.StatementImports.SelectMany(i => i.StatementLines).Count(l => l.IsMatched),
            UnmatchedLines = reconciliation.StatementImports.SelectMany(i => i.StatementLines).Count(l => !l.IsMatched),
            AdjustmentCount = reconciliation.Adjustments.Count
        };

        return Ok(summary);
    }

    [HttpPost("start")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<Models.BankReconciliation>> StartReconciliation([FromBody] StartReconciliationRequest request)
    {
        var tenantId = GetCurrentTenantId();

        // Validate bank account
        var bankAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == request.BankAccountId && b.TenantId == tenantId);

        if (bankAccount == null)
            return BadRequest("Bank account not found");

        // Generate reconciliation number
        var year = request.StatementDate.Year;
        var lastRecNumber = await _context.BankReconciliations
            .Where(r => r.TenantId == tenantId && r.ReconciliationNumber.StartsWith($"REC-{year}/"))
            .OrderByDescending(r => r.ReconciliationNumber)
            .Select(r => r.ReconciliationNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (!string.IsNullOrEmpty(lastRecNumber))
        {
            var parts = lastRecNumber.Split('/');
            if (parts.Length == 2 && int.TryParse(parts[1], out var lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        var reconciliationNumber = $"REC-{year}/{sequence:D4}";

        // Calculate book opening balance
        var bookOpeningBalance = bankAccount.OpeningBalance + await _context.CashBankTransactions
            .Where(t => t.BankAccountId == request.BankAccountId 
                     && t.TenantId == tenantId 
                     && t.Status == Server.Modules.CashBank.Models.CashBankStatus.Posted
                     && t.VoucherDate < request.StatementDate.Date)
            .SumAsync(t => t.RecPayType == Server.Modules.CashBank.Models.RecPayType.Receipt ? t.TotalAmount : -t.TotalAmount);

        var reconciliation = new Models.BankReconciliation
        {
            BankAccountId = request.BankAccountId,
            ReconciliationNumber = reconciliationNumber,
            StatementDate = DateTime.SpecifyKind(request.StatementDate, DateTimeKind.Utc),
            StatementOpeningBalance = request.StatementOpeningBalance,
            StatementClosingBalance = request.StatementClosingBalance,
            BookOpeningBalance = bookOpeningBalance,
            BookClosingBalance = bookOpeningBalance, // Will be updated after matching
            DifferenceAmount = request.StatementClosingBalance - bookOpeningBalance,
            Status = ReconciliationStatus.Draft,
            TenantId = tenantId
        };

        _context.BankReconciliations.Add(reconciliation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = reconciliation.Id }, reconciliation);
    }

    [HttpPost("{id}/import-statement")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult> ImportStatement(Guid id, [FromBody] ImportStatementRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var userId = GetCurrentUserId();

        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reconciliation == null)
            return NotFound();

        if (reconciliation.Status == ReconciliationStatus.Locked)
            return BadRequest("Cannot import statement for locked reconciliation");

        try
        {
            // Decode base64 file content
            var fileContent = Encoding.UTF8.GetString(Convert.FromBase64String(request.FileContent));

            // Create import record
            var import = new BankStatementImport
            {
                BankReconciliationId = id,
                FileName = request.FileName,
                Format = request.Format == "Excel" ? ImportFormat.Excel : ImportFormat.CSV,
                ImportedByUserId = userId,
                ColumnMapping = System.Text.Json.JsonSerializer.Serialize(request.ColumnMapping),
                TenantId = tenantId
            };

            // Parse CSV
            var (lines, fileHash) = await _importService.ParseCsvStatement(fileContent, request.ColumnMapping, import.Id, tenantId);

            // Check for duplicate import
            var existingImport = await _context.BankStatementImports
                .AnyAsync(i => i.TenantId == tenantId && i.FileHash == fileHash);

            if (existingImport)
                return BadRequest("This statement has already been imported");

            import.FileHash = fileHash;
            import.TotalLines = lines.Count;
            import.ImportedLines = lines.Count;
            import.SkippedLines = 0;

            _context.BankStatementImports.Add(import);
            _context.BankStatementLines.AddRange(lines);

            // Update reconciliation status
            if (reconciliation.Status == ReconciliationStatus.Draft)
            {
                reconciliation.Status = ReconciliationStatus.InProgress;
            }

            await _context.SaveChangesAsync();

            return Ok(new { ImportId = import.Id, LinesImported = lines.Count });
        }
        catch (Exception ex)
        {
            return BadRequest($"Import failed: {ex.Message}");
        }
    }

    [HttpGet("{id}/statement-lines")]
    public async Task<ActionResult<IEnumerable<StatementLineResponse>>> GetStatementLines(Guid id)
    {
        var tenantId = GetCurrentTenantId();

        var lines = await _context.BankStatementLines
            .Where(l => l.BankStatementImport!.BankReconciliationId == id && l.TenantId == tenantId)
            .Select(l => new StatementLineResponse
            {
                Id = l.Id,
                TransactionDate = l.TransactionDate,
                ValueDate = l.ValueDate,
                Description = l.Description,
                ChequeNumber = l.ChequeNumber,
                ReferenceNumber = l.ReferenceNumber,
                DebitAmount = l.DebitAmount,
                CreditAmount = l.CreditAmount,
                Balance = l.Balance,
                NetAmount = l.NetAmount,
                IsMatched = l.IsMatched,
                IsAdjustment = l.IsAdjustment,
                MatchNotes = l.MatchNotes
            })
            .ToListAsync();

        return Ok(lines);
    }

    [HttpGet("{id}/unmatched-transactions")]
    public async Task<ActionResult<IEnumerable<UnmatchedTransactionResponse>>> GetUnmatchedTransactions(Guid id)
    {
        var tenantId = GetCurrentTenantId();

        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reconciliation == null)
            return NotFound();

        var unmatchedTransactions = await _context.CashBankTransactions
            .Where(t => t.BankAccountId == reconciliation.BankAccountId
                     && t.TenantId == tenantId
                     && t.Status == Server.Modules.CashBank.Models.CashBankStatus.Posted
                     && !_context.ReconciliationMatches.Any(m => m.CashBankTransactionId == t.Id && !m.IsReversed))
            .Select(t => new UnmatchedTransactionResponse
            {
                Id = t.Id,
                VoucherNo = t.VoucherNo,
                VoucherDate = t.VoucherDate,
                TransactionType = t.TransactionType.ToString(),
                RecPayType = t.RecPayType.ToString(),
                TotalAmount = t.TotalAmount,
                ChequeNo = t.ChequeNo,
                ReferenceNo = t.ReferenceNo,
                IsMatched = false
            })
            .ToListAsync();

        return Ok(unmatchedTransactions);
    }

    [HttpPost("{id}/auto-match")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult> AutoMatch(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var userId = GetCurrentUserId();

        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reconciliation == null)
            return NotFound();

        if (reconciliation.Status == ReconciliationStatus.Locked)
            return BadRequest("Cannot auto-match for locked reconciliation");

        var matchedCount = await _matchingService.AutoMatchTransactions(id, tenantId, userId);

        return Ok(new { MatchedCount = matchedCount });
    }

    [HttpGet("{id}/match-suggestions")]
    public async Task<ActionResult<IEnumerable<MatchSuggestionResponse>>> GetMatchSuggestions(Guid id)
    {
        var tenantId = GetCurrentTenantId();

        var suggestions = await _matchingService.FindMatchSuggestions(id, tenantId);
        return Ok(suggestions);
    }

    [HttpPost("{id}/match")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult> ManualMatch(Guid id, [FromBody] MatchTransactionRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var userId = GetCurrentUserId();

        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reconciliation == null)
            return NotFound();

        if (reconciliation.Status == ReconciliationStatus.Locked)
            return BadRequest("Cannot match for locked reconciliation");

        var statementLine = await _context.BankStatementLines
            .FirstOrDefaultAsync(l => l.Id == request.StatementLineId && l.TenantId == tenantId);

        if (statementLine == null)
            return BadRequest("Statement line not found");

        CashBankTransaction? transaction = null;
        if (request.CashBankTransactionId.HasValue)
        {
            transaction = await _context.CashBankTransactions
                .FirstOrDefaultAsync(t => t.Id == request.CashBankTransactionId && t.TenantId == tenantId);

            if (transaction == null)
                return BadRequest("Transaction not found");
        }

        var match = new ReconciliationMatch
        {
            BankReconciliationId = id,
            BankStatementLineId = request.StatementLineId,
            CashBankTransactionId = request.CashBankTransactionId,
            MatchedAmount = request.MatchedAmount,
            MatchType = Models.MatchType.Manual,
            MatchGroup = request.MatchGroup,
            MatchedByUserId = userId,
            MatchNotes = request.Notes,
            TenantId = tenantId
        };

        _context.ReconciliationMatches.Add(match);

        statementLine.IsMatched = true;
        statementLine.MatchNotes = request.Notes;

        await _context.SaveChangesAsync();

        return Ok(match);
    }

    [HttpDelete("{id}/matches/{matchId}")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult> Unmatch(Guid id, Guid matchId, [FromBody] UnmatchRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var userId = GetCurrentUserId();

        var match = await _context.ReconciliationMatches
            .Include(m => m.BankStatementLine)
            .FirstOrDefaultAsync(m => m.Id == matchId && m.BankReconciliationId == id && m.TenantId == tenantId);

        if (match == null)
            return NotFound();

        match.IsReversed = true;
        match.ReversedAt = DateTime.UtcNow;
        match.ReversedByUserId = userId;
        match.ReversalReason = request.Reason;

        if (match.BankStatementLine != null)
        {
            match.BankStatementLine.IsMatched = false;
            match.BankStatementLine.MatchNotes = null;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/adjustments")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult> AddAdjustment(Guid id, [FromBody] AddAdjustmentRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var userId = GetCurrentUserId();

        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reconciliation == null)
            return NotFound();

        if (reconciliation.Status == ReconciliationStatus.Locked)
            return BadRequest("Cannot add adjustment for locked reconciliation");

        var adjustmentType = Enum.Parse<ReconciliationAdjustmentType>(request.AdjustmentType);

        var adjustment = new ReconciliationAdjustment
        {
            BankReconciliationId = id,
            BankStatementLineId = request.StatementLineId,
            AdjustmentType = adjustmentType,
            Description = request.Description,
            Amount = request.Amount,
            AdjustmentDate = DateTime.SpecifyKind(request.AdjustmentDate, DateTimeKind.Utc),
            Notes = request.Notes,
            TenantId = tenantId
        };

        _context.ReconciliationAdjustments.Add(adjustment);
        await _context.SaveChangesAsync();

        return Ok(adjustment);
    }

    [HttpPost("{id}/finalize")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult> FinalizeReconciliation(Guid id, [FromBody] FinalizeReconciliationRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var userId = GetCurrentUserId();

        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reconciliation == null)
            return NotFound();

        if (reconciliation.Status == ReconciliationStatus.Locked)
            return BadRequest("Reconciliation is already locked");

        reconciliation.Status = ReconciliationStatus.Completed;
        reconciliation.CompletedDate = DateTime.UtcNow;
        reconciliation.CompletedByUserId = userId;
        reconciliation.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return Ok(reconciliation);
    }

    [HttpPost("{id}/lock")]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult> LockReconciliation(Guid id)
    {
        var tenantId = GetCurrentTenantId();

        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reconciliation == null)
            return NotFound();

        if (reconciliation.Status != ReconciliationStatus.Completed)
            return BadRequest("Can only lock completed reconciliations");

        reconciliation.Status = ReconciliationStatus.Locked;
        await _context.SaveChangesAsync();

        return Ok(reconciliation);
    }
}
