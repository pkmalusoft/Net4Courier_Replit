using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.GeneralLedger.Models;
using Shared.Enums;

namespace Server.Modules.GeneralLedger.Controllers;

[Authorize(Policy = "CookieOrJwt")]
[ApiController]
[Route("api/[controller]")]
public class CashFlowController : ControllerBase
{
    private readonly AppDbContext _context;

    public CashFlowController(AppDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Invalid tenant context");
        }
        return tenantId;
    }

    [HttpPost("direct")]
    public async Task<ActionResult<CashFlowDirectResultDto>> GetDirectCashFlow(
        [FromBody] CashFlowRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var (periodStart, periodEnd) = GetPeriodDates(request);

        var cashAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Asset
                && (a.AccountName.ToLower().Contains("cash")
                    || a.AccountName.ToLower().Contains("bank")
                    || a.AccountCode!.StartsWith("101")
                    || a.AccountCode.StartsWith("102")))
            .Select(a => a.Id)
            .ToListAsync();

        var cashTransactionsQuery = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Include(jel => jel.Account)
            .Where(jel => jel.TenantId == tenantId
                && cashAccounts.Contains(jel.AccountId)
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate >= periodStart
                && jel.JournalEntry.EntryDate <= periodEnd);

        if (request.BranchId.HasValue)
            cashTransactionsQuery = cashTransactionsQuery.Where(jel => jel.JournalEntry.BranchId == request.BranchId.Value);
        if (request.DepartmentId.HasValue)
            cashTransactionsQuery = cashTransactionsQuery.Where(jel => jel.JournalEntry.DepartmentId == request.DepartmentId.Value);

        var cashTransactions = await cashTransactionsQuery
            .Select(jel => new
            {
                jel.JournalEntryId,
                jel.AccountId,
                jel.Debit,
                jel.Credit,
                jel.JournalEntry.EntryDate,
                jel.JournalEntry.EntryNumber,
                JournalDescription = jel.JournalEntry.Description,
                AccountName = jel.Account!.AccountName
            })
            .ToListAsync();

        var journalIds = cashTransactions.Select(t => t.JournalEntryId).Distinct().ToList();

        var counterpartLines = await _context.JournalEntryLines
            .Include(jel => jel.Account)
            .Where(jel => jel.TenantId == tenantId
                && journalIds.Contains(jel.JournalEntryId)
                && !cashAccounts.Contains(jel.AccountId))
            .Select(jel => new
            {
                jel.JournalEntryId,
                jel.AccountId,
                AccountType = jel.Account!.AccountType,
                AccountName = jel.Account.AccountName,
                jel.Debit,
                jel.Credit
            })
            .ToListAsync();

        var operatingReceipts = new List<CashFlowLineItemDto>();
        var operatingPayments = new List<CashFlowLineItemDto>();
        var investingReceipts = new List<CashFlowLineItemDto>();
        var investingPayments = new List<CashFlowLineItemDto>();
        var financingReceipts = new List<CashFlowLineItemDto>();
        var financingPayments = new List<CashFlowLineItemDto>();

        foreach (var transaction in cashTransactions)
        {
            var counterparts = counterpartLines.Where(c => c.JournalEntryId == transaction.JournalEntryId).ToList();
            var primaryCounterpart = counterparts.FirstOrDefault();

            if (primaryCounterpart == null) continue;

            var isReceipt = transaction.Debit > 0;
            var amount = isReceipt ? transaction.Debit : transaction.Credit;

            var category = ClassifyCashFlowCategory(primaryCounterpart.AccountType, primaryCounterpart.AccountName);

            var item = new CashFlowLineItemDto
            {
                Date = transaction.EntryDate,
                VoucherNumber = transaction.EntryNumber,
                Description = transaction.JournalDescription,
                CounterpartAccount = primaryCounterpart.AccountName,
                Amount = amount
            };

            switch (category)
            {
                case "Operating":
                    if (isReceipt)
                        operatingReceipts.Add(item);
                    else
                        operatingPayments.Add(item);
                    break;
                case "Investing":
                    if (isReceipt)
                        investingReceipts.Add(item);
                    else
                        investingPayments.Add(item);
                    break;
                case "Financing":
                    if (isReceipt)
                        financingReceipts.Add(item);
                    else
                        financingPayments.Add(item);
                    break;
            }
        }

        var openingBalance = await CalculateOpeningCashBalance(tenantId, cashAccounts, periodStart, request.BranchId, request.DepartmentId);
        var netOperating = operatingReceipts.Sum(x => x.Amount) - operatingPayments.Sum(x => x.Amount);
        var netInvesting = investingReceipts.Sum(x => x.Amount) - investingPayments.Sum(x => x.Amount);
        var netFinancing = financingReceipts.Sum(x => x.Amount) - financingPayments.Sum(x => x.Amount);
        var netChange = netOperating + netInvesting + netFinancing;
        var closingBalance = openingBalance + netChange;

        return Ok(new CashFlowDirectResultDto
        {
            ReportType = "Direct",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            OpeningCashBalance = openingBalance,
            OperatingActivities = new CashFlowActivityDto
            {
                ActivityType = "Operating",
                Receipts = operatingReceipts,
                Payments = operatingPayments,
                TotalReceipts = operatingReceipts.Sum(x => x.Amount),
                TotalPayments = operatingPayments.Sum(x => x.Amount),
                NetCashFlow = netOperating
            },
            InvestingActivities = new CashFlowActivityDto
            {
                ActivityType = "Investing",
                Receipts = investingReceipts,
                Payments = investingPayments,
                TotalReceipts = investingReceipts.Sum(x => x.Amount),
                TotalPayments = investingPayments.Sum(x => x.Amount),
                NetCashFlow = netInvesting
            },
            FinancingActivities = new CashFlowActivityDto
            {
                ActivityType = "Financing",
                Receipts = financingReceipts,
                Payments = financingPayments,
                TotalReceipts = financingReceipts.Sum(x => x.Amount),
                TotalPayments = financingPayments.Sum(x => x.Amount),
                NetCashFlow = netFinancing
            },
            NetChangeInCash = netChange,
            ClosingCashBalance = closingBalance
        });
    }

    [HttpPost("indirect")]
    public async Task<ActionResult<CashFlowIndirectResultDto>> GetIndirectCashFlow(
        [FromBody] CashFlowRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var (periodStart, periodEnd) = GetPeriodDates(request);

        var netIncome = await CalculateNetIncome(tenantId, periodStart, periodEnd, request.BranchId, request.DepartmentId);

        var adjustments = await CalculateNonCashAdjustments(tenantId, periodStart, periodEnd, request.BranchId, request.DepartmentId);

        var workingCapitalChanges = await CalculateWorkingCapitalChanges(tenantId, periodStart, periodEnd, request.BranchId, request.DepartmentId);

        var operatingCashFlow = netIncome + adjustments.Sum(a => a.Amount) + workingCapitalChanges.Sum(w => w.Amount);

        var (investingItems, netInvesting) = await CalculateInvestingActivities(tenantId, periodStart, periodEnd, request.BranchId, request.DepartmentId);
        var (financingItems, netFinancing) = await CalculateFinancingActivities(tenantId, periodStart, periodEnd, request.BranchId, request.DepartmentId);

        var cashAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Asset
                && (a.AccountName.ToLower().Contains("cash")
                    || a.AccountName.ToLower().Contains("bank")
                    || a.AccountCode!.StartsWith("101")
                    || a.AccountCode.StartsWith("102")))
            .Select(a => a.Id)
            .ToListAsync();

        var openingBalance = await CalculateOpeningCashBalance(tenantId, cashAccounts, periodStart, request.BranchId, request.DepartmentId);
        var netChange = operatingCashFlow + netInvesting + netFinancing;
        var closingBalance = openingBalance + netChange;

        return Ok(new CashFlowIndirectResultDto
        {
            ReportType = "Indirect",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            OpeningCashBalance = openingBalance,
            NetIncome = netIncome,
            NonCashAdjustments = adjustments,
            TotalNonCashAdjustments = adjustments.Sum(a => a.Amount),
            WorkingCapitalChanges = workingCapitalChanges,
            TotalWorkingCapitalChanges = workingCapitalChanges.Sum(w => w.Amount),
            NetCashFromOperating = operatingCashFlow,
            InvestingItems = investingItems,
            NetCashFromInvesting = netInvesting,
            FinancingItems = financingItems,
            NetCashFromFinancing = netFinancing,
            NetChangeInCash = netChange,
            ClosingCashBalance = closingBalance
        });
    }

    private async Task<decimal> CalculateNetIncome(Guid tenantId, DateTime periodStart, DateTime periodEnd,
        Guid? branchId = null, Guid? departmentId = null)
    {
        var revenueExpenseAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && (a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense))
            .Select(a => new { a.Id, a.AccountType })
            .ToListAsync();

        var accountIds = revenueExpenseAccounts.Select(a => a.Id).ToList();

        var journalLinesQuery = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && accountIds.Contains(jel.AccountId)
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate >= periodStart
                && jel.JournalEntry.EntryDate <= periodEnd);

        if (branchId.HasValue)
            journalLinesQuery = journalLinesQuery.Where(jel => jel.JournalEntry.BranchId == branchId.Value);
        if (departmentId.HasValue)
            journalLinesQuery = journalLinesQuery.Where(jel => jel.JournalEntry.DepartmentId == departmentId.Value);

        var journalLines = await journalLinesQuery
            .GroupBy(jel => jel.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                TotalDebit = g.Sum(x => x.Debit),
                TotalCredit = g.Sum(x => x.Credit)
            })
            .ToListAsync();

        var balanceDict = journalLines.ToDictionary(x => x.AccountId, x => x);

        decimal totalRevenue = 0;
        decimal totalExpense = 0;

        foreach (var account in revenueExpenseAccounts)
        {
            var balance = balanceDict.GetValueOrDefault(account.Id);
            if (balance == null) continue;

            if (account.AccountType == AccountType.Revenue)
            {
                totalRevenue += balance.TotalCredit - balance.TotalDebit;
            }
            else
            {
                totalExpense += balance.TotalDebit - balance.TotalCredit;
            }
        }

        return totalRevenue - totalExpense;
    }

    private async Task<List<CashFlowAdjustmentDto>> CalculateNonCashAdjustments(Guid tenantId, DateTime periodStart, DateTime periodEnd,
        Guid? branchId = null, Guid? departmentId = null)
    {
        var adjustments = new List<CashFlowAdjustmentDto>();

        var depreciationAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Expense
                && (a.AccountName.ToLower().Contains("depreciation")
                    || a.AccountName.ToLower().Contains("amortization")))
            .Select(a => a.Id)
            .ToListAsync();

        if (depreciationAccounts.Any())
        {
            var depreciationQuery = _context.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.TenantId == tenantId
                    && depreciationAccounts.Contains(jel.AccountId)
                    && jel.JournalEntry.Status == JournalEntryStatus.Posted
                    && !jel.JournalEntry.IsVoided
                    && jel.JournalEntry.EntryDate >= periodStart
                    && jel.JournalEntry.EntryDate <= periodEnd);

            if (branchId.HasValue)
                depreciationQuery = depreciationQuery.Where(jel => jel.JournalEntry.BranchId == branchId.Value);
            if (departmentId.HasValue)
                depreciationQuery = depreciationQuery.Where(jel => jel.JournalEntry.DepartmentId == departmentId.Value);

            var depreciation = await depreciationQuery.SumAsync(jel => jel.Debit - jel.Credit);

            if (depreciation != 0)
            {
                adjustments.Add(new CashFlowAdjustmentDto
                {
                    Description = "Depreciation & Amortization",
                    Amount = depreciation
                });
            }
        }

        return adjustments;
    }

    private async Task<List<CashFlowAdjustmentDto>> CalculateWorkingCapitalChanges(Guid tenantId, DateTime periodStart, DateTime periodEnd,
        Guid? branchId = null, Guid? departmentId = null)
    {
        var changes = new List<CashFlowAdjustmentDto>();

        var receivableAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Asset
                && (a.AccountName.ToLower().Contains("receivable")
                    || a.AccountName.ToLower().Contains("debtors")))
            .Select(a => a.Id)
            .ToListAsync();

        if (receivableAccounts.Any())
        {
            var openingReceivable = await GetAccountBalance(tenantId, receivableAccounts, null, periodStart.AddDays(-1), branchId, departmentId);
            var closingReceivable = await GetAccountBalance(tenantId, receivableAccounts, null, periodEnd, branchId, departmentId);
            var changeReceivable = closingReceivable - openingReceivable;

            if (changeReceivable != 0)
            {
                changes.Add(new CashFlowAdjustmentDto
                {
                    Description = "Change in Accounts Receivable",
                    Amount = -changeReceivable
                });
            }
        }

        var inventoryAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Asset
                && (a.AccountName.ToLower().Contains("inventory")
                    || a.AccountName.ToLower().Contains("stock")))
            .Select(a => a.Id)
            .ToListAsync();

        if (inventoryAccounts.Any())
        {
            var openingInventory = await GetAccountBalance(tenantId, inventoryAccounts, null, periodStart.AddDays(-1), branchId, departmentId);
            var closingInventory = await GetAccountBalance(tenantId, inventoryAccounts, null, periodEnd, branchId, departmentId);
            var changeInventory = closingInventory - openingInventory;

            if (changeInventory != 0)
            {
                changes.Add(new CashFlowAdjustmentDto
                {
                    Description = "Change in Inventory",
                    Amount = -changeInventory
                });
            }
        }

        var payableAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Liability
                && (a.AccountName.ToLower().Contains("payable")
                    || a.AccountName.ToLower().Contains("creditors")))
            .Select(a => a.Id)
            .ToListAsync();

        if (payableAccounts.Any())
        {
            var openingPayable = await GetAccountBalance(tenantId, payableAccounts, AccountType.Liability, periodStart.AddDays(-1), branchId, departmentId);
            var closingPayable = await GetAccountBalance(tenantId, payableAccounts, AccountType.Liability, periodEnd, branchId, departmentId);
            var changePayable = closingPayable - openingPayable;

            if (changePayable != 0)
            {
                changes.Add(new CashFlowAdjustmentDto
                {
                    Description = "Change in Accounts Payable",
                    Amount = changePayable
                });
            }
        }

        return changes;
    }

    private async Task<decimal> GetAccountBalance(Guid tenantId, List<Guid> accountIds, AccountType? accountType, DateTime asAtDate,
        Guid? branchId = null, Guid? departmentId = null)
    {
        var query = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && accountIds.Contains(jel.AccountId)
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate <= asAtDate);

        if (branchId.HasValue)
            query = query.Where(jel => jel.JournalEntry.BranchId == branchId.Value);
        if (departmentId.HasValue)
            query = query.Where(jel => jel.JournalEntry.DepartmentId == departmentId.Value);

        var balance = await query.SumAsync(jel => jel.Debit - jel.Credit);

        if (accountType == AccountType.Liability || accountType == AccountType.Equity)
        {
            balance = -balance;
        }

        return balance;
    }

    private async Task<(List<CashFlowItemDto> items, decimal netAmount)> CalculateInvestingActivities(Guid tenantId, DateTime periodStart, DateTime periodEnd,
        Guid? branchId = null, Guid? departmentId = null)
    {
        var items = new List<CashFlowItemDto>();

        var fixedAssetAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Asset
                && (a.AccountName.ToLower().Contains("fixed asset")
                    || a.AccountName.ToLower().Contains("equipment")
                    || a.AccountName.ToLower().Contains("property")
                    || a.AccountName.ToLower().Contains("vehicle")
                    || a.AccountCode!.StartsWith("15")
                    || a.AccountCode.StartsWith("16")))
            .Select(a => new { a.Id, a.AccountName })
            .ToListAsync();

        foreach (var account in fixedAssetAccounts)
        {
            var movementsQuery = _context.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.TenantId == tenantId
                    && jel.AccountId == account.Id
                    && jel.JournalEntry.Status == JournalEntryStatus.Posted
                    && !jel.JournalEntry.IsVoided
                    && jel.JournalEntry.EntryDate >= periodStart
                    && jel.JournalEntry.EntryDate <= periodEnd);

            if (branchId.HasValue)
                movementsQuery = movementsQuery.Where(jel => jel.JournalEntry.BranchId == branchId.Value);
            if (departmentId.HasValue)
                movementsQuery = movementsQuery.Where(jel => jel.JournalEntry.DepartmentId == departmentId.Value);

            var movements = await movementsQuery.SumAsync(jel => jel.Debit - jel.Credit);

            if (movements != 0)
            {
                items.Add(new CashFlowItemDto
                {
                    Description = movements > 0 ? $"Purchase of {account.AccountName}" : $"Sale of {account.AccountName}",
                    Amount = -movements
                });
            }
        }

        return (items, items.Sum(i => i.Amount));
    }

    private async Task<(List<CashFlowItemDto> items, decimal netAmount)> CalculateFinancingActivities(Guid tenantId, DateTime periodStart, DateTime periodEnd,
        Guid? branchId = null, Guid? departmentId = null)
    {
        var items = new List<CashFlowItemDto>();

        var loanAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Liability
                && (a.AccountName.ToLower().Contains("loan")
                    || a.AccountName.ToLower().Contains("borrowing")
                    || a.AccountName.ToLower().Contains("debt")))
            .Select(a => new { a.Id, a.AccountName })
            .ToListAsync();

        foreach (var account in loanAccounts)
        {
            var movementsQuery = _context.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.TenantId == tenantId
                    && jel.AccountId == account.Id
                    && jel.JournalEntry.Status == JournalEntryStatus.Posted
                    && !jel.JournalEntry.IsVoided
                    && jel.JournalEntry.EntryDate >= periodStart
                    && jel.JournalEntry.EntryDate <= periodEnd);

            if (branchId.HasValue)
                movementsQuery = movementsQuery.Where(jel => jel.JournalEntry.BranchId == branchId.Value);
            if (departmentId.HasValue)
                movementsQuery = movementsQuery.Where(jel => jel.JournalEntry.DepartmentId == departmentId.Value);

            var movements = await movementsQuery.SumAsync(jel => jel.Credit - jel.Debit);

            if (movements != 0)
            {
                items.Add(new CashFlowItemDto
                {
                    Description = movements > 0 ? $"Proceeds from {account.AccountName}" : $"Repayment of {account.AccountName}",
                    Amount = movements
                });
            }
        }

        var equityAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Equity
                && (a.AccountName.ToLower().Contains("capital")
                    || a.AccountName.ToLower().Contains("share")
                    || a.AccountName.ToLower().Contains("dividend")))
            .Select(a => new { a.Id, a.AccountName })
            .ToListAsync();

        foreach (var account in equityAccounts)
        {
            var movementsQuery = _context.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.TenantId == tenantId
                    && jel.AccountId == account.Id
                    && jel.JournalEntry.Status == JournalEntryStatus.Posted
                    && !jel.JournalEntry.IsVoided
                    && jel.JournalEntry.EntryDate >= periodStart
                    && jel.JournalEntry.EntryDate <= periodEnd);

            if (branchId.HasValue)
                movementsQuery = movementsQuery.Where(jel => jel.JournalEntry.BranchId == branchId.Value);
            if (departmentId.HasValue)
                movementsQuery = movementsQuery.Where(jel => jel.JournalEntry.DepartmentId == departmentId.Value);

            var movements = await movementsQuery.SumAsync(jel => jel.Credit - jel.Debit);

            if (movements != 0)
            {
                var isDividend = account.AccountName.ToLower().Contains("dividend");
                items.Add(new CashFlowItemDto
                {
                    Description = isDividend ? "Dividends Paid" : $"Change in {account.AccountName}",
                    Amount = isDividend ? -Math.Abs(movements) : movements
                });
            }
        }

        return (items, items.Sum(i => i.Amount));
    }

    private async Task<decimal> CalculateOpeningCashBalance(Guid tenantId, List<Guid> cashAccounts, DateTime periodStart,
        Guid? branchId = null, Guid? departmentId = null)
    {
        var query = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && cashAccounts.Contains(jel.AccountId)
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate < periodStart);

        if (branchId.HasValue)
            query = query.Where(jel => jel.JournalEntry.BranchId == branchId.Value);
        if (departmentId.HasValue)
            query = query.Where(jel => jel.JournalEntry.DepartmentId == departmentId.Value);

        return await query.SumAsync(jel => jel.Debit - jel.Credit);
    }

    private static string ClassifyCashFlowCategory(AccountType accountType, string accountName)
    {
        var lowerName = accountName.ToLower();

        if (lowerName.Contains("fixed asset") || lowerName.Contains("equipment")
            || lowerName.Contains("property") || lowerName.Contains("vehicle")
            || lowerName.Contains("investment"))
        {
            return "Investing";
        }

        if (lowerName.Contains("loan") || lowerName.Contains("borrowing")
            || lowerName.Contains("capital") || lowerName.Contains("share")
            || lowerName.Contains("dividend"))
        {
            return "Financing";
        }

        return "Operating";
    }

    private (DateTime periodStart, DateTime periodEnd) GetPeriodDates(CashFlowRequestDto request)
    {
        DateTime periodStart, periodEnd;

        if (request.Month == 0)
        {
            periodStart = new DateTime(request.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            periodEnd = new DateTime(request.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }
        else
        {
            periodStart = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            periodEnd = new DateTime(request.Year, request.Month,
                DateTime.DaysInMonth(request.Year, request.Month), 23, 59, 59, DateTimeKind.Utc);
        }

        return (periodStart, periodEnd);
    }
}

public class CashFlowRequestDto
{
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public int Month { get; set; } = 0;
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class CashFlowLineItemDto
{
    public DateTime Date { get; set; }
    public string VoucherNumber { get; set; } = "";
    public string Description { get; set; } = "";
    public string CounterpartAccount { get; set; } = "";
    public decimal Amount { get; set; }
}

public class CashFlowActivityDto
{
    public string ActivityType { get; set; } = "";
    public List<CashFlowLineItemDto> Receipts { get; set; } = new();
    public List<CashFlowLineItemDto> Payments { get; set; } = new();
    public decimal TotalReceipts { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal NetCashFlow { get; set; }
}

public class CashFlowDirectResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal OpeningCashBalance { get; set; }
    public CashFlowActivityDto OperatingActivities { get; set; } = new();
    public CashFlowActivityDto InvestingActivities { get; set; } = new();
    public CashFlowActivityDto FinancingActivities { get; set; } = new();
    public decimal NetChangeInCash { get; set; }
    public decimal ClosingCashBalance { get; set; }
}

public class CashFlowAdjustmentDto
{
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
}

public class CashFlowItemDto
{
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
}

public class CashFlowIndirectResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal OpeningCashBalance { get; set; }
    public decimal NetIncome { get; set; }
    public List<CashFlowAdjustmentDto> NonCashAdjustments { get; set; } = new();
    public decimal TotalNonCashAdjustments { get; set; }
    public List<CashFlowAdjustmentDto> WorkingCapitalChanges { get; set; } = new();
    public decimal TotalWorkingCapitalChanges { get; set; }
    public decimal NetCashFromOperating { get; set; }
    public List<CashFlowItemDto> InvestingItems { get; set; } = new();
    public decimal NetCashFromInvesting { get; set; }
    public List<CashFlowItemDto> FinancingItems { get; set; } = new();
    public decimal NetCashFromFinancing { get; set; }
    public decimal NetChangeInCash { get; set; }
    public decimal ClosingCashBalance { get; set; }
}
