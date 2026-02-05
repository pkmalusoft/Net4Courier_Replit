using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;
using Net4Courier.Finance.Entities;

namespace Net4Courier.Infrastructure.Services;

public class PrepaidService
{
    private readonly ApplicationDbContext _context;
    private readonly AWBStockService _stockService;

    public PrepaidService(ApplicationDbContext context, AWBStockService stockService)
    {
        _context = context;
        _stockService = stockService;
    }

    public async Task<List<PrepaidDocument>> GetAllDocumentsAsync(long companyId, long? branchId = null, long? customerId = null)
    {
        var query = _context.PrepaidDocuments
            .Include(p => p.PrepaidAWBs)
            .Where(p => p.CompanyId == companyId && !p.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(p => p.BranchId == branchId.Value);

        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == customerId.Value);

        return await query.OrderByDescending(p => p.DocumentDate).ToListAsync();
    }

    public async Task<PrepaidDocument?> GetByIdAsync(long id)
    {
        return await _context.PrepaidDocuments
            .Include(p => p.PrepaidAWBs)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<PrepaidDocument?> GetByDocumentNoAsync(string documentNo, long companyId)
    {
        return await _context.PrepaidDocuments
            .Include(p => p.PrepaidAWBs)
            .FirstOrDefaultAsync(p => p.DocumentNo == documentNo && p.CompanyId == companyId && !p.IsDeleted);
    }

    public async Task<string> GenerateDocumentNoAsync(long companyId, long branchId)
    {
        var lastDoc = await _context.PrepaidDocuments
            .Where(p => p.CompanyId == companyId && p.BranchId == branchId)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastDoc?.DocumentNo != null)
        {
            var lastNum = lastDoc.DocumentNo.Split('-').LastOrDefault();
            if (int.TryParse(lastNum, out var num))
                nextNumber = num + 1;
        }

        return $"PRE-{DateTime.UtcNow:yyyyMM}-{nextNumber:D5}";
    }

    public async Task<PrepaidDocument> CreatePrepaidDocumentAsync(
        PrepaidDocument document, 
        long? awbStockId, 
        int userId, 
        string userName)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
            document.CreatedAt = DateTime.UtcNow;
            document.DocumentDate = DateTime.SpecifyKind(document.DocumentDate, DateTimeKind.Utc);
            document.CreatedBy = userId;
            document.CreatedByName = userName;
            document.BalanceAmount = document.TotalPrepaidAmount;
            document.UsedAmount = 0;
            document.Status = PrepaidDocumentStatus.Active;

            if (string.IsNullOrEmpty(document.DocumentNo))
                document.DocumentNo = await GenerateDocumentNoAsync(document.CompanyId, document.BranchId);

            _context.PrepaidDocuments.Add(document);
            await _context.SaveChangesAsync();

            if (awbStockId.HasValue && document.NoOfAWBs > 0)
            {
                var awbNumbers = await _stockService.GetAvailableAWBNumbersAsync(awbStockId.Value, document.NoOfAWBs);
                if (awbNumbers.Count < document.NoOfAWBs)
                    throw new InvalidOperationException("Not enough AWBs available in stock.");

                var ratePerAWB = document.TotalPrepaidAmount / document.NoOfAWBs;

                foreach (var awbNo in awbNumbers)
                {
                    var prepaidAWB = new PrepaidAWB
                    {
                        PrepaidDocumentId = document.Id,
                        AWBStockId = awbStockId.Value,
                        AWBNo = awbNo,
                        Rate = ratePerAWB,
                        Amount = ratePerAWB,
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };
                    _context.PrepaidAWBs.Add(prepaidAWB);
                }

                document.AWBNoFrom = awbNumbers.First();
                document.AWBNoTo = awbNumbers.Last();

                await _stockService.AllocateAWBsAsync(awbStockId.Value, document.NoOfAWBs);
            }

            await CreateSaleJournalEntryAsync(document, userId, userName);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return document;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private async Task CreateSaleJournalEntryAsync(PrepaidDocument document, int userId, string userName)
    {
        var prepaidControlAccount = await _context.AccountHeads
            .FirstOrDefaultAsync(a => a.Code == "PREPAID_CONTROL" && !a.IsDeleted);

        long? cashBankAccountId = null;
        if (document.PaymentMode == PrepaidPaymentMode.Cash)
        {
            var cashAccount = await _context.AccountHeads
                .FirstOrDefaultAsync(a => a.Code == "CASH" && !a.IsDeleted);
            cashBankAccountId = cashAccount?.Id ?? document.CashAccountId;
        }
        else if (document.PaymentMode == PrepaidPaymentMode.Bank || document.PaymentMode == PrepaidPaymentMode.Cheque)
        {
            // BankAccounts reference removed during Cash/Bank module migration
            // TODO: Use new CashBank service to get account information
            cashBankAccountId = null;
        }

        if (prepaidControlAccount == null || cashBankAccountId == null)
            return;

        var journal = new Journal
        {
            VoucherNo = $"JV-PRE-{document.DocumentNo}",
            VoucherDate = DateTime.SpecifyKind(document.DocumentDate, DateTimeKind.Utc),
            CompanyId = document.CompanyId,
            BranchId = document.BranchId,
            FinancialYearId = document.FinancialYearId,
            VoucherType = "PREPAID_SALE",
            Narration = $"Prepaid AWB sale to {document.CustomerName} - {document.DocumentNo}",
            Reference = document.DocumentNo,
            TotalDebit = document.TotalPrepaidAmount,
            TotalCredit = document.TotalPrepaidAmount,
            IsPosted = true,
            PostedAt = DateTime.UtcNow,
            PostedBy = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            CreatedByName = userName
        };

        _context.Journals.Add(journal);
        await _context.SaveChangesAsync();

        var debitEntry = new JournalEntry
        {
            JournalId = journal.Id,
            AccountHeadId = cashBankAccountId.Value,
            Debit = document.TotalPrepaidAmount,
            Credit = null,
            Narration = $"Cash/Bank received for prepaid AWB - {document.CustomerName}",
            PartyId = document.CustomerId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        var creditEntry = new JournalEntry
        {
            JournalId = journal.Id,
            AccountHeadId = prepaidControlAccount.Id,
            Debit = null,
            Credit = document.TotalPrepaidAmount,
            Narration = $"Prepaid AWB liability - {document.CustomerName}",
            PartyId = document.CustomerId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.JournalEntries.Add(debitEntry);
        _context.JournalEntries.Add(creditEntry);

        document.JournalId = journal.Id;
        document.PrepaidControlAccountId = prepaidControlAccount.Id;
    }

    public async Task<bool> UsePrepaidAWBAsync(long prepaidAWBId, long inscanMasterId, int userId, string userName, string? consignor = null, string? consignee = null)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var prepaidAWB = await _context.PrepaidAWBs
                .Include(p => p.PrepaidDocument)
                .FirstOrDefaultAsync(p => p.Id == prepaidAWBId && !p.IsUsed);

            if (prepaidAWB == null)
                return false;

            prepaidAWB.IsUsed = true;
            prepaidAWB.UsedDate = DateTime.UtcNow;
            prepaidAWB.InscanMasterId = inscanMasterId;
            prepaidAWB.Consignor = consignor;
            prepaidAWB.Consignee = consignee;
            prepaidAWB.ModifiedAt = DateTime.UtcNow;
            prepaidAWB.ModifiedBy = userId;

            var document = prepaidAWB.PrepaidDocument;
            document.UsedAmount += prepaidAWB.Amount;
            document.BalanceAmount = document.TotalPrepaidAmount - document.UsedAmount;
            document.ModifiedAt = DateTime.UtcNow;
            document.ModifiedBy = userId;

            if (document.BalanceAmount <= 0)
                document.Status = PrepaidDocumentStatus.FullyUsed;
            else if (document.UsedAmount > 0)
                document.Status = PrepaidDocumentStatus.PartiallyUsed;

            await CreateUsageJournalEntryAsync(prepaidAWB, document, userId, userName);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private async Task CreateUsageJournalEntryAsync(PrepaidAWB prepaidAWB, PrepaidDocument document, int userId, string userName)
    {
        var prepaidControlAccount = await _context.AccountHeads
            .FirstOrDefaultAsync(a => a.Code == "PREPAID_CONTROL" && !a.IsDeleted);

        var revenueAccount = await _context.AccountHeads
            .FirstOrDefaultAsync(a => a.Code == "REVENUE" && !a.IsDeleted);

        if (prepaidControlAccount == null || revenueAccount == null)
            return;

        var journal = new Journal
        {
            VoucherNo = $"JV-PRU-{document.DocumentNo}-{prepaidAWB.AWBNo}",
            VoucherDate = DateTime.UtcNow,
            CompanyId = document.CompanyId,
            BranchId = document.BranchId,
            FinancialYearId = document.FinancialYearId,
            VoucherType = "PREPAID_USAGE",
            Narration = $"Prepaid AWB usage - AWB {prepaidAWB.AWBNo} - {document.CustomerName}",
            Reference = prepaidAWB.AWBNo,
            TotalDebit = prepaidAWB.Amount,
            TotalCredit = prepaidAWB.Amount,
            IsPosted = true,
            PostedAt = DateTime.UtcNow,
            PostedBy = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            CreatedByName = userName
        };

        _context.Journals.Add(journal);
        await _context.SaveChangesAsync();

        var debitEntry = new JournalEntry
        {
            JournalId = journal.Id,
            AccountHeadId = prepaidControlAccount.Id,
            Debit = prepaidAWB.Amount,
            Credit = null,
            Narration = $"Prepaid liability utilized - AWB {prepaidAWB.AWBNo}",
            PartyId = document.CustomerId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        var creditEntry = new JournalEntry
        {
            JournalId = journal.Id,
            AccountHeadId = revenueAccount.Id,
            Debit = null,
            Credit = prepaidAWB.Amount,
            Narration = $"Revenue recognized - Prepaid AWB {prepaidAWB.AWBNo}",
            PartyId = document.CustomerId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.JournalEntries.Add(debitEntry);
        _context.JournalEntries.Add(creditEntry);

        prepaidAWB.UsageJournalId = journal.Id;
    }

    public async Task<PrepaidAWB?> GetAvailablePrepaidAWBAsync(long customerId, string? awbNo = null)
    {
        var query = _context.PrepaidAWBs
            .Include(p => p.PrepaidDocument)
            .Where(p => p.PrepaidDocument.CustomerId == customerId 
                && !p.IsUsed 
                && !p.IsDeleted 
                && p.PrepaidDocument.Status != PrepaidDocumentStatus.Cancelled);

        if (!string.IsNullOrEmpty(awbNo))
            query = query.Where(p => p.AWBNo == awbNo);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<PrepaidAWB>> GetPrepaidAWBsByDocumentAsync(long documentId)
    {
        return await _context.PrepaidAWBs
            .Where(p => p.PrepaidDocumentId == documentId && !p.IsDeleted)
            .OrderBy(p => p.AWBNo)
            .ToListAsync();
    }

    public async Task<decimal> GetCustomerPrepaidBalanceAsync(long customerId)
    {
        return await _context.PrepaidDocuments
            .Where(p => p.CustomerId == customerId 
                && !p.IsDeleted 
                && p.Status != PrepaidDocumentStatus.Cancelled)
            .SumAsync(p => p.BalanceAmount);
    }

    public async Task<int> GetCustomerAvailableAWBCountAsync(long customerId)
    {
        return await _context.PrepaidAWBs
            .Include(p => p.PrepaidDocument)
            .Where(p => p.PrepaidDocument.CustomerId == customerId 
                && !p.IsUsed 
                && !p.IsDeleted 
                && p.PrepaidDocument.Status != PrepaidDocumentStatus.Cancelled)
            .CountAsync();
    }

    public async Task<bool> CancelDocumentAsync(long id, int userId, string reason)
    {
        var document = await GetByIdAsync(id);
        if (document == null) return false;

        if (document.UsedAmount > 0)
            throw new InvalidOperationException("Cannot cancel document with used AWBs.");

        document.Status = PrepaidDocumentStatus.Cancelled;
        document.ModifiedAt = DateTime.UtcNow;
        document.ModifiedBy = userId;
        document.Remarks = $"Cancelled: {reason}";

        await _context.SaveChangesAsync();
        return true;
    }
}
