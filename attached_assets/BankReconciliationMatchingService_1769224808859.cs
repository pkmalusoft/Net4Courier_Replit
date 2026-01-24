using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.BankReconciliation.Models;
using Server.Modules.CashBank.Models;
using Shared.DTOs;

namespace Server.Modules.BankReconciliation.Services;

public interface IBankReconciliationMatchingService
{
    Task<List<MatchSuggestionResponse>> FindMatchSuggestions(
        Guid reconciliationId, 
        Guid tenantId, 
        int maxSuggestions = 10);
    
    Task<int> AutoMatchTransactions(
        Guid reconciliationId, 
        Guid tenantId, 
        Guid userId);
}

public class BankReconciliationMatchingService : IBankReconciliationMatchingService
{
    private readonly AppDbContext _context;
    private const decimal AmountTolerance = 0.01m; // $0.01 tolerance
    private const int DateToleranceDays = 3; // +/- 3 days for fuzzy matching

    public BankReconciliationMatchingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MatchSuggestionResponse>> FindMatchSuggestions(
        Guid reconciliationId, 
        Guid tenantId, 
        int maxSuggestions = 10)
    {
        var suggestions = new List<MatchSuggestionResponse>();

        // Get reconciliation with bank account
        var reconciliation = await _context.BankReconciliations
            .Include(r => r.BankAccount)
            .FirstOrDefaultAsync(r => r.Id == reconciliationId && r.TenantId == tenantId);

        if (reconciliation == null || reconciliation.BankAccount == null)
            return suggestions;

        // Get unmatched statement lines
        var unmatchedStatementLines = await _context.BankStatementLines
            .Where(l => l.BankStatementImport!.BankReconciliationId == reconciliationId 
                     && l.TenantId == tenantId 
                     && !l.IsMatched)
            .ToListAsync();

        // Get unmatched bank transactions for this bank account
        var unmatchedTransactions = await _context.CashBankTransactions
            .Where(t => t.BankAccountId == reconciliation.BankAccountId
                     && t.TenantId == tenantId
                     && t.Status == CashBankStatus.Posted
                     && !_context.ReconciliationMatches.Any(m => m.CashBankTransactionId == t.Id && !m.IsReversed))
            .ToListAsync();

        // Find matches for each statement line
        foreach (var statementLine in unmatchedStatementLines.Take(maxSuggestions))
        {
            var matches = FindMatchesForStatementLine(statementLine, unmatchedTransactions);
            suggestions.AddRange(matches);
        }

        return suggestions.Take(maxSuggestions).ToList();
    }

    public async Task<int> AutoMatchTransactions(
        Guid reconciliationId, 
        Guid tenantId, 
        Guid userId)
    {
        var matchedCount = 0;

        var reconciliation = await _context.BankReconciliations
            .Include(r => r.BankAccount)
            .FirstOrDefaultAsync(r => r.Id == reconciliationId && r.TenantId == tenantId);

        if (reconciliation == null || reconciliation.BankAccount == null)
            return 0;

        var unmatchedStatementLines = await _context.BankStatementLines
            .Where(l => l.BankStatementImport!.BankReconciliationId == reconciliationId 
                     && l.TenantId == tenantId 
                     && !l.IsMatched)
            .ToListAsync();

        var unmatchedTransactions = await _context.CashBankTransactions
            .Where(t => t.BankAccountId == reconciliation.BankAccountId
                     && t.TenantId == tenantId
                     && t.Status == CashBankStatus.Posted
                     && !_context.ReconciliationMatches.Any(m => m.CashBankTransactionId == t.Id && !m.IsReversed))
            .ToListAsync();

        foreach (var statementLine in unmatchedStatementLines)
        {
            var matches = FindMatchesForStatementLine(statementLine, unmatchedTransactions);
            
            // Only auto-match high confidence matches (>= 0.9)
            var bestMatch = matches.Where(m => m.Confidence >= 0.9m).OrderByDescending(m => m.Confidence).FirstOrDefault();
            
            if (bestMatch != null)
            {
                var transaction = unmatchedTransactions.FirstOrDefault(t => t.Id == bestMatch.TransactionId);
                if (transaction != null)
                {
                    // Create match
                    var reconciliationMatch = new ReconciliationMatch
                    {
                        BankReconciliationId = reconciliationId,
                        BankStatementLineId = statementLine.Id,
                        CashBankTransactionId = transaction.Id,
                        MatchedAmount = Math.Abs(statementLine.NetAmount),
                        MatchType = Models.MatchType.Automatic,
                        MatchedByUserId = userId,
                        MatchNotes = $"Auto-matched: {bestMatch.Reason}",
                        TenantId = tenantId
                    };

                    _context.ReconciliationMatches.Add(reconciliationMatch);
                    
                    // Mark statement line as matched
                    statementLine.IsMatched = true;
                    statementLine.MatchNotes = bestMatch.Reason;
                    
                    // Remove from unmatched list
                    unmatchedTransactions.Remove(transaction);
                    
                    matchedCount++;
                }
            }
        }

        await _context.SaveChangesAsync();
        return matchedCount;
    }

    private List<MatchSuggestionResponse> FindMatchesForStatementLine(
        BankStatementLine statementLine, 
        List<CashBankTransaction> transactions)
    {
        var suggestions = new List<MatchSuggestionResponse>();
        var statementAmount = Math.Abs(statementLine.NetAmount);

        foreach (var transaction in transactions)
        {
            // Rule 1: Exact match (amount + date)
            if (Math.Abs(transaction.TotalAmount - statementAmount) <= AmountTolerance 
                && IsSameDate(transaction.VoucherDate, statementLine.TransactionDate))
            {
                suggestions.Add(new MatchSuggestionResponse
                {
                    StatementLineId = statementLine.Id,
                    TransactionId = transaction.Id,
                    MatchType = "Exact",
                    Confidence = 1.0m,
                    Reason = "Exact match: Same amount and date"
                });
                continue;
            }

            // Rule 2: Cheque number match
            if (!string.IsNullOrWhiteSpace(statementLine.ChequeNumber) 
                && !string.IsNullOrWhiteSpace(transaction.ChequeNo)
                && statementLine.ChequeNumber.Equals(transaction.ChequeNo, StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add(new MatchSuggestionResponse
                {
                    StatementLineId = statementLine.Id,
                    TransactionId = transaction.Id,
                    MatchType = "Cheque",
                    Confidence = 0.95m,
                    Reason = $"Cheque number match: {statementLine.ChequeNumber}"
                });
                continue;
            }

            // Rule 3: Reference number match
            if (!string.IsNullOrWhiteSpace(statementLine.ReferenceNumber) 
                && !string.IsNullOrWhiteSpace(transaction.ReferenceNo)
                && statementLine.ReferenceNumber.Equals(transaction.ReferenceNo, StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add(new MatchSuggestionResponse
                {
                    StatementLineId = statementLine.Id,
                    TransactionId = transaction.Id,
                    MatchType = "Reference",
                    Confidence = 0.90m,
                    Reason = $"Reference number match: {statementLine.ReferenceNumber}"
                });
                continue;
            }

            // Rule 4: Fuzzy date window (amount match within +/- 3 days)
            if (Math.Abs(transaction.TotalAmount - statementAmount) <= AmountTolerance)
            {
                var daysDifference = Math.Abs((transaction.VoucherDate.Date - statementLine.TransactionDate.Date).Days);
                if (daysDifference <= DateToleranceDays)
                {
                    var confidence = 0.8m - (daysDifference * 0.05m); // Reduce confidence by 5% per day difference
                    suggestions.Add(new MatchSuggestionResponse
                    {
                        StatementLineId = statementLine.Id,
                        TransactionId = transaction.Id,
                        MatchType = "Amount",
                        Confidence = confidence,
                        Reason = $"Amount match within {daysDifference} day(s)"
                    });
                }
            }
        }

        return suggestions;
    }

    private bool IsSameDate(DateTime date1, DateTime date2)
    {
        return date1.Date == date2.Date;
    }
}
