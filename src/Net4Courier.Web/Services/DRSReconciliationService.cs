using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Operations.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class DRSReconciliationService
{
    private readonly ApplicationDbContext _context;

    public DRSReconciliationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DRSReconciliationSummary> GetReconciliationSummary(long drsId)
    {
        var drs = await _context.DRSs
            .Include(d => d.Details)
                .ThenInclude(d => d.Inscan)
            .Include(d => d.CashSubmissions)
            .Include(d => d.Expenses)
            .FirstOrDefaultAsync(d => d.Id == drsId);

        if (drs == null)
            return new DRSReconciliationSummary();

        return CalculateSummary(drs);
    }

    public DRSReconciliationSummary CalculateSummary(DRS drs)
    {
        var summary = new DRSReconciliationSummary
        {
            DRSId = drs.Id,
            DRSNo = drs.DRSNo,
            DRSDate = drs.DRSDate,
            CourierId = drs.DeliveryEmployeeId ?? 0,
            CourierName = drs.DeliveryEmployeeName ?? "",
            Status = drs.Status
        };

        foreach (var detail in drs.Details)
        {
            var inscan = detail.Inscan;
            if (inscan == null) continue;

            summary.TotalAWBs++;
            
            if (detail.Status == "Delivered")
            {
                summary.DeliveredCount++;
                
                var paymentMode = inscan.PaymentModeId;
                switch (paymentMode)
                {
                    case PaymentMode.COD:
                        summary.TotalMaterialCost += inscan.CODAmount ?? 0;
                        summary.TotalCourierCharges += inscan.CourierCharge ?? 0;
                        break;
                    case PaymentMode.PickupCash:
                        summary.PickupCash += (inscan.CourierCharge ?? 0) + (inscan.OtherCharge ?? 0);
                        break;
                    case PaymentMode.ToPay:
                        summary.TotalCourierCharges += inscan.CourierCharge ?? 0;
                        break;
                }
            }
            else if (detail.Status == "Returned" || detail.Status == "Refused")
            {
                summary.ReturnedCount++;
            }
            else
            {
                summary.PendingCount++;
            }
        }

        summary.ExpectedTotal = summary.TotalMaterialCost + summary.TotalCourierCharges + 
                                summary.PickupCash + summary.OutstandingCollected;

        var cashSubmission = drs.CashSubmissions.OrderByDescending(c => c.SubmissionTime).FirstOrDefault();
        summary.CashSubmitted = cashSubmission?.CashSubmittedAmount ?? 0;
        summary.CashReceived = cashSubmission?.ReceivedAmount ?? 0;
        summary.IsAcknowledged = cashSubmission?.IsAcknowledged ?? false;

        summary.TotalExpenses = drs.Expenses.Sum(e => e.Amount);
        summary.ApprovedExpenses = drs.Expenses.Where(e => e.Status == ExpenseStatus.Approved).Sum(e => e.Amount);
        summary.PendingExpenses = drs.Expenses.Where(e => e.Status == ExpenseStatus.Pending).Sum(e => e.Amount);

        summary.Variance = summary.ExpectedTotal - (summary.CashReceived + summary.ApprovedExpenses);

        return summary;
    }

    public async Task<bool> SubmitCash(CourierCashSubmission submission)
    {
        var drs = await _context.DRSs.FindAsync(submission.DRSId);
        if (drs == null) return false;

        submission.SubmissionTime = DateTime.UtcNow;
        _context.CourierCashSubmissions.Add(submission);

        drs.Status = DRSStatus.Submitted;
        drs.SubmittedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AcknowledgeReceipt(long submissionId, int receivedById, string receivedByName, decimal receivedAmount)
    {
        var submission = await _context.CourierCashSubmissions
            .Include(c => c.DRS)
            .FirstOrDefaultAsync(c => c.Id == submissionId);

        if (submission == null) return false;

        submission.ReceivedById = receivedById;
        submission.ReceivedByName = receivedByName;
        submission.ReceivedAmount = receivedAmount;
        submission.ReceivedAt = DateTime.UtcNow;
        submission.IsAcknowledged = true;
        submission.ReceiptNo = await GenerateReceiptNumber();

        var drs = submission.DRS;
        drs.ActualReceived = receivedAmount;

        await _context.SaveChangesAsync();
        await ReconcileDRS(drs.Id);

        return true;
    }

    public async Task<bool> AddExpense(CourierExpense expense)
    {
        expense.Status = ExpenseStatus.Pending;
        _context.CourierExpenses.Add(expense);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveExpense(long expenseId, int approvedById, string approvedByName, string? remarks = null)
    {
        var expense = await _context.CourierExpenses.FindAsync(expenseId);
        if (expense == null) return false;

        expense.Status = ExpenseStatus.Approved;
        expense.ApprovedById = approvedById;
        expense.ApprovedByName = approvedByName;
        expense.ApprovedAt = DateTime.UtcNow;
        expense.ApprovalRemarks = remarks;

        await _context.SaveChangesAsync();
        await ReconcileDRS(expense.DRSId);

        return true;
    }

    public async Task<bool> RejectExpense(long expenseId, int approvedById, string approvedByName, string? remarks = null)
    {
        var expense = await _context.CourierExpenses.FindAsync(expenseId);
        if (expense == null) return false;

        expense.Status = ExpenseStatus.Rejected;
        expense.ApprovedById = approvedById;
        expense.ApprovedByName = approvedByName;
        expense.ApprovedAt = DateTime.UtcNow;
        expense.ApprovalRemarks = remarks;

        await _context.SaveChangesAsync();
        await ReconcileDRS(expense.DRSId);

        return true;
    }

    public async Task ReconcileDRS(long drsId)
    {
        var drs = await _context.DRSs
            .Include(d => d.Details)
                .ThenInclude(d => d.Inscan)
            .Include(d => d.CashSubmissions)
            .Include(d => d.Expenses)
            .FirstOrDefaultAsync(d => d.Id == drsId);

        if (drs == null) return;

        var summary = CalculateSummary(drs);

        drs.TotalCourierCharges = summary.TotalCourierCharges;
        drs.TotalMaterialCost = summary.TotalMaterialCost;
        drs.PickupCash = summary.PickupCash;
        drs.OutstandingCollected = summary.OutstandingCollected;
        drs.ExpectedTotal = summary.ExpectedTotal;
        drs.ActualReceived = summary.CashReceived;
        drs.ApprovedExpenses = summary.ApprovedExpenses;
        drs.Variance = summary.Variance;

        bool hasPendingExpenses = drs.Expenses.Any(e => e.Status == ExpenseStatus.Pending);
        bool isAcknowledged = summary.IsAcknowledged;

        if (isAcknowledged && !hasPendingExpenses)
        {
            drs.Status = DRSStatus.Reconciled;
            drs.ReconciledAt = DateTime.UtcNow;

            if (summary.Variance != 0)
            {
                await CreateLedgerEntry(drs, summary.Variance);
            }
        }
        else if (isAcknowledged)
        {
            drs.Status = DRSStatus.PartiallyReconciled;
        }

        await _context.SaveChangesAsync();
    }

    private async Task CreateLedgerEntry(DRS drs, decimal variance)
    {
        var existingEntry = await _context.CourierLedgers
            .FirstOrDefaultAsync(l => l.DRSId == drs.Id && !l.IsSettled);

        if (existingEntry != null)
        {
            existingEntry.DebitAmount = variance > 0 ? variance : 0;
            existingEntry.CreditAmount = variance < 0 ? Math.Abs(variance) : 0;
            existingEntry.RunningBalance = await CalculateRunningBalance(drs.DeliveryEmployeeId ?? 0, variance);
        }
        else
        {
            var ledgerEntry = new CourierLedger
            {
                CompanyId = drs.CompanyId,
                BranchId = drs.BranchId,
                CourierId = drs.DeliveryEmployeeId ?? 0,
                CourierName = drs.DeliveryEmployeeName,
                TransactionDate = DateTime.UtcNow,
                EntryType = variance > 0 ? LedgerEntryType.Shortage : LedgerEntryType.Excess,
                DRSId = drs.Id,
                DRSNo = drs.DRSNo,
                DebitAmount = variance > 0 ? variance : 0,
                CreditAmount = variance < 0 ? Math.Abs(variance) : 0,
                Narration = variance > 0 
                    ? $"Short collection from DRS {drs.DRSNo}" 
                    : $"Excess collection from DRS {drs.DRSNo}",
                IsSettled = false
            };

            ledgerEntry.RunningBalance = await CalculateRunningBalance(drs.DeliveryEmployeeId ?? 0, variance);
            _context.CourierLedgers.Add(ledgerEntry);
        }
    }

    private async Task<decimal> CalculateRunningBalance(int courierId, decimal newVariance)
    {
        var lastEntry = await _context.CourierLedgers
            .Where(l => l.CourierId == courierId)
            .OrderByDescending(l => l.TransactionDate)
            .ThenByDescending(l => l.Id)
            .FirstOrDefaultAsync();

        decimal currentBalance = lastEntry?.RunningBalance ?? 0;
        return currentBalance + newVariance;
    }

    public async Task<List<DRS>> GetPendingReconciliations(long? branchId = null)
    {
        var query = _context.DRSs
            .Include(d => d.CashSubmissions)
            .Include(d => d.Expenses)
            .Where(d => d.Status == DRSStatus.Open || d.Status == DRSStatus.Submitted || d.Status == DRSStatus.PartiallyReconciled);

        if (branchId.HasValue)
            query = query.Where(d => d.BranchId == branchId);

        return await query.OrderByDescending(d => d.DRSDate).ToListAsync();
    }

    public async Task<List<CourierExpense>> GetPendingExpenses(long? branchId = null)
    {
        var query = _context.CourierExpenses
            .Include(e => e.DRS)
            .Where(e => e.Status == ExpenseStatus.Pending);

        if (branchId.HasValue)
            query = query.Where(e => e.DRS.BranchId == branchId);

        return await query.OrderBy(e => e.ExpenseDate).ToListAsync();
    }

    public async Task<List<CourierLedger>> GetCourierLedger(int courierId)
    {
        return await _context.CourierLedgers
            .Where(l => l.CourierId == courierId)
            .OrderByDescending(l => l.TransactionDate)
            .ThenByDescending(l => l.Id)
            .ToListAsync();
    }

    public async Task<decimal> GetCourierBalance(long courierId)
    {
        var lastEntry = await _context.CourierLedgers
            .Where(l => l.CourierId == courierId)
            .OrderByDescending(l => l.TransactionDate)
            .ThenByDescending(l => l.Id)
            .FirstOrDefaultAsync();

        return lastEntry?.RunningBalance ?? 0;
    }

    public async Task<List<CourierBalanceSummary>> GetCourierBalances(long? branchId = null)
    {
        var query = _context.CourierLedgers
            .GroupBy(l => new { l.CourierId, l.CourierName })
            .Select(g => new CourierBalanceSummary
            {
                CourierId = g.Key.CourierId,
                CourierName = g.Key.CourierName ?? "",
                TotalDebits = g.Sum(l => l.DebitAmount),
                TotalCredits = g.Sum(l => l.CreditAmount),
                Balance = g.OrderByDescending(l => l.TransactionDate).ThenByDescending(l => l.Id).First().RunningBalance,
                UnsettledCount = g.Count(l => !l.IsSettled)
            });

        return await query.ToListAsync();
    }

    private async Task<string> GenerateReceiptNumber()
    {
        var today = DateTime.UtcNow;
        var prefix = $"RCP{today:yyyyMMdd}";
        
        var lastReceipt = await _context.CourierCashSubmissions
            .Where(c => c.ReceiptNo != null && c.ReceiptNo.StartsWith(prefix))
            .OrderByDescending(c => c.ReceiptNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastReceipt?.ReceiptNo != null)
        {
            var lastNum = lastReceipt.ReceiptNo.Substring(prefix.Length);
            if (int.TryParse(lastNum, out int num))
                sequence = num + 1;
        }

        return $"{prefix}{sequence:D4}";
    }
}

public class DRSReconciliationSummary
{
    public long DRSId { get; set; }
    public string DRSNo { get; set; } = "";
    public DateTime DRSDate { get; set; }
    public int CourierId { get; set; }
    public string CourierName { get; set; } = "";
    public DRSStatus Status { get; set; }

    public int TotalAWBs { get; set; }
    public int DeliveredCount { get; set; }
    public int PendingCount { get; set; }
    public int ReturnedCount { get; set; }

    public decimal TotalCourierCharges { get; set; }
    public decimal TotalMaterialCost { get; set; }
    public decimal PickupCash { get; set; }
    public decimal OutstandingCollected { get; set; }
    public decimal ExpectedTotal { get; set; }

    public decimal CashSubmitted { get; set; }
    public decimal CashReceived { get; set; }
    public bool IsAcknowledged { get; set; }

    public decimal TotalExpenses { get; set; }
    public decimal ApprovedExpenses { get; set; }
    public decimal PendingExpenses { get; set; }

    public decimal Variance { get; set; }
}

public class CourierBalanceSummary
{
    public int CourierId { get; set; }
    public string CourierName { get; set; } = "";
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal Balance { get; set; }
    public int UnsettledCount { get; set; }
}
