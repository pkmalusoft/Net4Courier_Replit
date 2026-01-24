using Microsoft.EntityFrameworkCore;
using Net4Courier.Finance.Entities;
using Net4Courier.Infrastructure.Data;

namespace Net4Courier.Web.Services;

public class MatchSuggestion
{
    public long StatementLineId { get; set; }
    public long JournalId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public decimal Amount { get; set; }
    public string MatchType { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public interface IBankReconciliationService
{
    Task<List<MatchSuggestion>> FindMatchSuggestions(long reconciliationId, int maxSuggestions = 10);
    Task<int> AutoMatchTransactions(long reconciliationId, int userId);
    Task<bool> CreateManualMatch(long reconciliationId, long statementLineId, long journalId, int userId, string? notes = null);
    Task<bool> UnmatchTransaction(long matchId, int userId, string reason);
    Task<decimal> CalculateBookBalance(long bankAccountId, DateTime asOfDate);
    Task<bool> CompleteReconciliation(long reconciliationId, int userId);
    Task<bool> LockReconciliation(long reconciliationId, int userId);
    Task UpdateReconciliationBalances(long reconciliationId);
}

public class BankReconciliationService : IBankReconciliationService
{
    private readonly ApplicationDbContext _context;
    private const decimal AmountTolerance = 0.01m;
    private const int DateToleranceDays = 3;

    public BankReconciliationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MatchSuggestion>> FindMatchSuggestions(long reconciliationId, int maxSuggestions = 10)
    {
        var suggestions = new List<MatchSuggestion>();

        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == reconciliationId);

        if (reconciliation == null)
            return suggestions;

        var unmatchedStatementLines = await _context.BankStatementLines
            .Where(l => l.BankStatementImport!.BankReconciliationId == reconciliationId && !l.IsMatched)
            .ToListAsync();

        var unmatchedVouchers = await _context.Journals
            .Include(j => j.Entries)
            .Where(j => j.VoucherType == "BV" 
                     && j.IsPosted 
                     && j.Entries.Any(e => e.AccountHeadId == reconciliation.BankAccountId)
                     && !_context.ReconciliationMatches.Any(m => m.JournalId == j.Id && !m.IsReversed))
            .ToListAsync();

        foreach (var statementLine in unmatchedStatementLines.Take(maxSuggestions))
        {
            var matches = FindMatchesForStatementLine(statementLine, unmatchedVouchers);
            suggestions.AddRange(matches);
        }

        return suggestions.OrderByDescending(s => s.Confidence).Take(maxSuggestions).ToList();
    }

    public async Task<int> AutoMatchTransactions(long reconciliationId, int userId)
    {
        var matchedCount = 0;

        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == reconciliationId);

        if (reconciliation == null)
            return 0;

        if (reconciliation.Status == ReconciliationStatus.Locked || 
            reconciliation.Status == ReconciliationStatus.Completed)
            return 0;

        var unmatchedStatementLines = await _context.BankStatementLines
            .Where(l => l.BankStatementImport!.BankReconciliationId == reconciliationId && !l.IsMatched)
            .ToListAsync();

        var unmatchedVouchers = await _context.Journals
            .Include(j => j.Entries)
            .Where(j => j.VoucherType == "BV" 
                     && j.IsPosted 
                     && j.Entries.Any(e => e.AccountHeadId == reconciliation.BankAccountId)
                     && !_context.ReconciliationMatches.Any(m => m.JournalId == j.Id && !m.IsReversed))
            .ToListAsync();

        foreach (var statementLine in unmatchedStatementLines)
        {
            var matches = FindMatchesForStatementLine(statementLine, unmatchedVouchers);
            
            var bestMatch = matches.Where(m => m.Confidence >= 0.9m).OrderByDescending(m => m.Confidence).FirstOrDefault();
            
            if (bestMatch != null)
            {
                var voucher = unmatchedVouchers.FirstOrDefault(v => v.Id == bestMatch.JournalId);
                if (voucher != null)
                {
                    var reconciliationMatch = new ReconciliationMatch
                    {
                        BankReconciliationId = reconciliationId,
                        BankStatementLineId = statementLine.Id,
                        JournalId = voucher.Id,
                        MatchedAmount = Math.Abs(statementLine.CreditAmount - statementLine.DebitAmount),
                        MatchType = Finance.Entities.MatchType.Automatic,
                        MatchedByUserId = userId,
                        MatchNotes = $"Auto-matched: {bestMatch.Reason}"
                    };

                    _context.ReconciliationMatches.Add(reconciliationMatch);
                    
                    statementLine.IsMatched = true;
                    statementLine.MatchNotes = bestMatch.Reason;
                    
                    unmatchedVouchers.Remove(voucher);
                    
                    matchedCount++;
                }
            }
        }

        if (matchedCount > 0)
        {
            await _context.SaveChangesAsync();
            await UpdateReconciliationBalances(reconciliationId);
        }

        return matchedCount;
    }

    public async Task<bool> CreateManualMatch(long reconciliationId, long statementLineId, long journalId, int userId, string? notes = null)
    {
        var reconciliation = await _context.BankReconciliations.FindAsync(reconciliationId);
        if (reconciliation == null || 
            reconciliation.Status == ReconciliationStatus.Locked || 
            reconciliation.Status == ReconciliationStatus.Completed)
            return false;

        var statementLine = await _context.BankStatementLines.FindAsync(statementLineId);
        if (statementLine == null || statementLine.IsMatched)
            return false;

        var existingMatch = await _context.ReconciliationMatches
            .AnyAsync(m => m.JournalId == journalId && !m.IsReversed);
        if (existingMatch)
            return false;

        var match = new ReconciliationMatch
        {
            BankReconciliationId = reconciliationId,
            BankStatementLineId = statementLineId,
            JournalId = journalId,
            MatchedAmount = Math.Abs(statementLine.CreditAmount - statementLine.DebitAmount),
            MatchType = Finance.Entities.MatchType.Manual,
            MatchedByUserId = userId,
            MatchNotes = notes
        };

        _context.ReconciliationMatches.Add(match);
        statementLine.IsMatched = true;
        
        await _context.SaveChangesAsync();
        await UpdateReconciliationBalances(reconciliationId);
        
        return true;
    }

    public async Task<bool> UnmatchTransaction(long matchId, int userId, string reason)
    {
        var match = await _context.ReconciliationMatches
            .Include(m => m.BankStatementLine)
            .Include(m => m.BankReconciliation)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null || match.IsReversed)
            return false;

        if (match.BankReconciliation?.Status == ReconciliationStatus.Locked)
            return false;

        match.IsReversed = true;
        match.ReversedAt = DateTime.UtcNow;
        match.ReversedByUserId = userId;
        match.ReversalReason = reason;

        if (match.BankStatementLine != null)
        {
            match.BankStatementLine.IsMatched = false;
            match.BankStatementLine.MatchNotes = null;
        }

        await _context.SaveChangesAsync();
        await UpdateReconciliationBalances(match.BankReconciliationId);
        
        return true;
    }

    public async Task<decimal> CalculateBookBalance(long bankAccountId, DateTime asOfDate)
    {
        var bankAccount = await _context.AccountHeads.FindAsync(bankAccountId);
        if (bankAccount == null)
            return 0;

        var openingBalance = bankAccount.OpeningBalance ?? 0;

        var journalSum = await _context.JournalEntries
            .Where(e => e.AccountHeadId == bankAccountId 
                     && e.Journal.IsPosted 
                     && e.Journal.VoucherDate <= asOfDate.Date)
            .SumAsync(e => (e.Debit ?? 0) - (e.Credit ?? 0));

        return openingBalance + journalSum;
    }

    public async Task UpdateReconciliationBalances(long reconciliationId)
    {
        var reconciliation = await _context.BankReconciliations.FindAsync(reconciliationId);
        if (reconciliation == null) return;

        reconciliation.BookClosingBalance = await CalculateBookBalance(
            reconciliation.BankAccountId, 
            reconciliation.StatementDate);

        var adjustmentTotal = await _context.ReconciliationAdjustments
            .Where(a => a.BankReconciliationId == reconciliationId && !a.IsPosted)
            .SumAsync(a => a.Amount);

        reconciliation.DifferenceAmount = reconciliation.StatementClosingBalance - reconciliation.BookClosingBalance - adjustmentTotal;

        if (reconciliation.Status == ReconciliationStatus.Draft)
        {
            reconciliation.Status = ReconciliationStatus.InProgress;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> CompleteReconciliation(long reconciliationId, int userId)
    {
        var reconciliation = await _context.BankReconciliations.FindAsync(reconciliationId);
        if (reconciliation == null)
            return false;

        if (reconciliation.Status == ReconciliationStatus.Locked)
            return false;

        await UpdateReconciliationBalances(reconciliationId);
        
        reconciliation = await _context.BankReconciliations.FindAsync(reconciliationId);
        if (reconciliation == null)
            return false;

        if (Math.Abs(reconciliation.DifferenceAmount) > 0.01m)
            return false;

        reconciliation.Status = ReconciliationStatus.Completed;
        reconciliation.CompletedDate = DateTime.UtcNow;
        reconciliation.CompletedByUserId = userId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LockReconciliation(long reconciliationId, int userId)
    {
        var reconciliation = await _context.BankReconciliations.FindAsync(reconciliationId);
        if (reconciliation == null || reconciliation.Status != ReconciliationStatus.Completed)
            return false;

        reconciliation.Status = ReconciliationStatus.Locked;
        reconciliation.LockedByUserId = userId;
        reconciliation.LockedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private List<MatchSuggestion> FindMatchesForStatementLine(BankStatementLine statementLine, List<Journal> vouchers)
    {
        var suggestions = new List<MatchSuggestion>();
        var statementAmount = Math.Abs(statementLine.CreditAmount - statementLine.DebitAmount);

        foreach (var voucher in vouchers)
        {
            var voucherAmount = voucher.TotalDebit ?? voucher.TotalCredit ?? 0;

            if (Math.Abs(voucherAmount - statementAmount) <= AmountTolerance 
                && voucher.VoucherDate.Date == statementLine.TransactionDate.Date)
            {
                suggestions.Add(new MatchSuggestion
                {
                    StatementLineId = statementLine.Id,
                    JournalId = voucher.Id,
                    VoucherNo = voucher.VoucherNo,
                    VoucherDate = voucher.VoucherDate,
                    Amount = voucherAmount,
                    MatchType = "Exact",
                    Confidence = 1.0m,
                    Reason = "Exact match: Same amount and date"
                });
                continue;
            }

            if (!string.IsNullOrWhiteSpace(statementLine.ChequeNumber) 
                && !string.IsNullOrWhiteSpace(voucher.Reference)
                && statementLine.ChequeNumber.Equals(voucher.Reference, StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add(new MatchSuggestion
                {
                    StatementLineId = statementLine.Id,
                    JournalId = voucher.Id,
                    VoucherNo = voucher.VoucherNo,
                    VoucherDate = voucher.VoucherDate,
                    Amount = voucherAmount,
                    MatchType = "Cheque",
                    Confidence = 0.95m,
                    Reason = $"Cheque number match: {statementLine.ChequeNumber}"
                });
                continue;
            }

            if (!string.IsNullOrWhiteSpace(statementLine.ReferenceNumber) 
                && !string.IsNullOrWhiteSpace(voucher.Reference)
                && statementLine.ReferenceNumber.Equals(voucher.Reference, StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add(new MatchSuggestion
                {
                    StatementLineId = statementLine.Id,
                    JournalId = voucher.Id,
                    VoucherNo = voucher.VoucherNo,
                    VoucherDate = voucher.VoucherDate,
                    Amount = voucherAmount,
                    MatchType = "Reference",
                    Confidence = 0.90m,
                    Reason = $"Reference number match: {statementLine.ReferenceNumber}"
                });
                continue;
            }

            if (Math.Abs(voucherAmount - statementAmount) <= AmountTolerance)
            {
                var daysDifference = Math.Abs((voucher.VoucherDate.Date - statementLine.TransactionDate.Date).Days);
                if (daysDifference <= DateToleranceDays)
                {
                    var confidence = 0.8m - (daysDifference * 0.05m);
                    suggestions.Add(new MatchSuggestion
                    {
                        StatementLineId = statementLine.Id,
                        JournalId = voucher.Id,
                        VoucherNo = voucher.VoucherNo,
                        VoucherDate = voucher.VoucherDate,
                        Amount = voucherAmount,
                        MatchType = "Amount",
                        Confidence = confidence,
                        Reason = $"Amount match within {daysDifference} day(s)"
                    });
                }
            }
        }

        return suggestions;
    }
}
