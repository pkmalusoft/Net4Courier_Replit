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
public class ProfitLossController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProfitLossController(AppDbContext context)
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

    [HttpPost("standard")]
    public async Task<ActionResult<ProfitLossResultDto>> GetStandardProfitLoss(
        [FromBody] ProfitLossRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var (periodStart, periodEnd) = GetPeriodDates(request);

        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && (a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense))
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountCode)
            .Select(a => new { a.Id, a.AccountCode, a.AccountName, a.AccountType, a.ParentAccountId })
            .ToListAsync();

        var journalLinesQuery = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate >= periodStart
                && jel.JournalEntry.EntryDate <= periodEnd);

        if (request.BranchId.HasValue)
            journalLinesQuery = journalLinesQuery.Where(jel => jel.JournalEntry.BranchId == request.BranchId.Value);
        if (request.DepartmentId.HasValue)
            journalLinesQuery = journalLinesQuery.Where(jel => jel.JournalEntry.DepartmentId == request.DepartmentId.Value);

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

        var revenueItems = new List<ProfitLossLineItemDto>();
        var expenseItems = new List<ProfitLossLineItemDto>();

        foreach (var account in accounts)
        {
            var balance = balanceDict.GetValueOrDefault(account.Id);
            var debit = balance?.TotalDebit ?? 0m;
            var credit = balance?.TotalCredit ?? 0m;

            decimal amount;
            if (account.AccountType == AccountType.Revenue)
            {
                amount = credit - debit;
            }
            else
            {
                amount = debit - credit;
            }

            if (amount == 0) continue;

            var item = new ProfitLossLineItemDto
            {
                AccountId = account.Id,
                AccountCode = account.AccountCode ?? "",
                AccountName = account.AccountName,
                Amount = amount
            };

            if (account.AccountType == AccountType.Revenue)
            {
                revenueItems.Add(item);
            }
            else
            {
                expenseItems.Add(item);
            }
        }

        var totalRevenue = revenueItems.Sum(x => x.Amount);
        var totalExpenses = expenseItems.Sum(x => x.Amount);
        var netProfitLoss = totalRevenue - totalExpenses;

        return Ok(new ProfitLossResultDto
        {
            ReportType = "Standard",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            RevenueItems = revenueItems,
            ExpenseItems = expenseItems,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            GrossProfit = totalRevenue,
            NetProfitLoss = netProfitLoss
        });
    }

    [HttpPost("period")]
    public async Task<ActionResult<ProfitLossPeriodResultDto>> GetPeriodProfitLoss(
        [FromBody] ProfitLossPeriodRequestDto request)
    {
        var tenantId = GetCurrentTenantId();

        var periods = new List<PeriodColumnDto>();
        var allAccountAmounts = new Dictionary<Guid, Dictionary<int, decimal>>();

        for (int i = 0; i < request.NumberOfPeriods; i++)
        {
            DateTime periodStart, periodEnd;
            string periodLabel;

            if (request.PeriodType == "Monthly")
            {
                var targetDate = new DateTime(request.Year, request.Month, 1).AddMonths(-i);
                periodStart = new DateTime(targetDate.Year, targetDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                periodEnd = new DateTime(targetDate.Year, targetDate.Month,
                    DateTime.DaysInMonth(targetDate.Year, targetDate.Month), 23, 59, 59, DateTimeKind.Utc);
                periodLabel = targetDate.ToString("MMM yyyy");
            }
            else if (request.PeriodType == "Quarterly")
            {
                var targetDate = new DateTime(request.Year, request.Month, 1).AddMonths(-i * 3);
                var quarter = (targetDate.Month - 1) / 3;
                var quarterStart = new DateTime(targetDate.Year, quarter * 3 + 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var quarterEnd = quarterStart.AddMonths(3).AddDays(-1);
                periodStart = quarterStart;
                periodEnd = new DateTime(quarterEnd.Year, quarterEnd.Month, quarterEnd.Day, 23, 59, 59, DateTimeKind.Utc);
                periodLabel = $"Q{quarter + 1} {targetDate.Year}";
            }
            else
            {
                var targetYear = request.Year - i;
                periodStart = new DateTime(targetYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                periodEnd = new DateTime(targetYear, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                periodLabel = targetYear.ToString();
            }

            periods.Add(new PeriodColumnDto
            {
                PeriodIndex = i,
                PeriodLabel = periodLabel,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd
            });

            var journalLinesQuery = _context.JournalEntryLines
                .Include(jel => jel.JournalEntry)
                .Where(jel => jel.TenantId == tenantId
                    && jel.JournalEntry.Status == JournalEntryStatus.Posted
                    && !jel.JournalEntry.IsVoided
                    && jel.JournalEntry.EntryDate >= periodStart
                    && jel.JournalEntry.EntryDate <= periodEnd);

            if (request.BranchId.HasValue)
                journalLinesQuery = journalLinesQuery.Where(jel => jel.JournalEntry.BranchId == request.BranchId.Value);
            if (request.DepartmentId.HasValue)
                journalLinesQuery = journalLinesQuery.Where(jel => jel.JournalEntry.DepartmentId == request.DepartmentId.Value);

            var journalLines = await journalLinesQuery
                .GroupBy(jel => jel.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    TotalDebit = g.Sum(x => x.Debit),
                    TotalCredit = g.Sum(x => x.Credit)
                })
                .ToListAsync();

            foreach (var line in journalLines)
            {
                if (!allAccountAmounts.ContainsKey(line.AccountId))
                {
                    allAccountAmounts[line.AccountId] = new Dictionary<int, decimal>();
                }
                allAccountAmounts[line.AccountId][i] = line.TotalDebit - line.TotalCredit;
            }
        }

        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && (a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense))
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountCode)
            .Select(a => new { a.Id, a.AccountCode, a.AccountName, a.AccountType })
            .ToListAsync();

        var revenueItems = new List<ProfitLossPeriodItemDto>();
        var expenseItems = new List<ProfitLossPeriodItemDto>();

        foreach (var account in accounts)
        {
            var periodAmounts = new List<decimal>();
            bool hasAnyAmount = false;

            for (int i = 0; i < request.NumberOfPeriods; i++)
            {
                decimal amount = 0;
                if (allAccountAmounts.TryGetValue(account.Id, out var amounts) && amounts.TryGetValue(i, out var rawAmount))
                {
                    if (account.AccountType == AccountType.Revenue)
                    {
                        amount = -rawAmount;
                    }
                    else
                    {
                        amount = rawAmount;
                    }
                    if (amount != 0) hasAnyAmount = true;
                }
                periodAmounts.Add(amount);
            }

            if (!hasAnyAmount) continue;

            var item = new ProfitLossPeriodItemDto
            {
                AccountId = account.Id,
                AccountCode = account.AccountCode ?? "",
                AccountName = account.AccountName,
                PeriodAmounts = periodAmounts
            };

            if (account.AccountType == AccountType.Revenue)
            {
                revenueItems.Add(item);
            }
            else
            {
                expenseItems.Add(item);
            }
        }

        var revenueTotals = new List<decimal>();
        var expenseTotals = new List<decimal>();
        var netProfitLossTotals = new List<decimal>();

        for (int i = 0; i < request.NumberOfPeriods; i++)
        {
            var revTotal = revenueItems.Sum(x => x.PeriodAmounts.ElementAtOrDefault(i));
            var expTotal = expenseItems.Sum(x => x.PeriodAmounts.ElementAtOrDefault(i));
            revenueTotals.Add(revTotal);
            expenseTotals.Add(expTotal);
            netProfitLossTotals.Add(revTotal - expTotal);
        }

        return Ok(new ProfitLossPeriodResultDto
        {
            ReportType = "Period Comparison",
            PeriodType = request.PeriodType,
            Periods = periods,
            RevenueItems = revenueItems,
            ExpenseItems = expenseItems,
            RevenueTotals = revenueTotals,
            ExpenseTotals = expenseTotals,
            NetProfitLossTotals = netProfitLossTotals
        });
    }

    [HttpPost("itemwise")]
    public async Task<ActionResult<ProfitLossItemWiseResultDto>> GetItemWiseProfitLoss(
        [FromBody] ProfitLossRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var (periodStart, periodEnd) = GetPeriodDates(request);

        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && (a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense))
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountCode)
            .Select(a => new { a.Id, a.AccountCode, a.AccountName, a.AccountType, a.ParentAccountId })
            .ToListAsync();

        var journalLinesQuery = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Include(jel => jel.Account)
            .Where(jel => jel.TenantId == tenantId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate >= periodStart
                && jel.JournalEntry.EntryDate <= periodEnd
                && jel.Account != null
                && (jel.Account.AccountType == AccountType.Revenue
                    || jel.Account.AccountType == AccountType.Expense));

        if (request.BranchId.HasValue)
            journalLinesQuery = journalLinesQuery.Where(jel => jel.JournalEntry.BranchId == request.BranchId.Value);
        if (request.DepartmentId.HasValue)
            journalLinesQuery = journalLinesQuery.Where(jel => jel.JournalEntry.DepartmentId == request.DepartmentId.Value);

        var journalLines = await journalLinesQuery
            .Select(jel => new
            {
                jel.AccountId,
                jel.JournalEntry.EntryDate,
                VoucherNumber = jel.JournalEntry.EntryNumber,
                Reference = jel.JournalEntry.Description,
                jel.Description,
                jel.Debit,
                jel.Credit,
                AccountType = jel.Account!.AccountType,
                AccountCode = jel.Account.AccountCode,
                AccountName = jel.Account.AccountName
            })
            .OrderBy(x => x.AccountId)
            .ThenBy(x => x.EntryDate)
            .ToListAsync();

        var groupedByAccount = journalLines
            .GroupBy(x => new { x.AccountId, x.AccountCode, x.AccountName, x.AccountType })
            .Select(g => new ProfitLossItemWiseAccountDto
            {
                AccountId = g.Key.AccountId,
                AccountCode = g.Key.AccountCode ?? "",
                AccountName = g.Key.AccountName,
                AccountType = g.Key.AccountType.ToString(),
                Transactions = g.Select(t => new ProfitLossTransactionDto
                {
                    EntryDate = t.EntryDate,
                    VoucherNumber = t.VoucherNumber ?? "",
                    Reference = t.Reference ?? "",
                    Description = t.Description ?? "",
                    Debit = t.Debit,
                    Credit = t.Credit,
                    Amount = g.Key.AccountType == AccountType.Revenue 
                        ? t.Credit - t.Debit 
                        : t.Debit - t.Credit
                }).ToList(),
                TotalAmount = g.Key.AccountType == AccountType.Revenue
                    ? g.Sum(t => t.Credit - t.Debit)
                    : g.Sum(t => t.Debit - t.Credit)
            })
            .ToList();

        var revenueAccounts = groupedByAccount
            .Where(x => x.AccountType == "Revenue" && x.TotalAmount != 0)
            .OrderBy(x => x.AccountCode)
            .ToList();

        var expenseAccounts = groupedByAccount
            .Where(x => x.AccountType == "Expense" && x.TotalAmount != 0)
            .OrderBy(x => x.AccountCode)
            .ToList();

        var totalRevenue = revenueAccounts.Sum(x => x.TotalAmount);
        var totalExpenses = expenseAccounts.Sum(x => x.TotalAmount);

        return Ok(new ProfitLossItemWiseResultDto
        {
            ReportType = "Item-wise Detail",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            RevenueAccounts = revenueAccounts,
            ExpenseAccounts = expenseAccounts,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            NetProfitLoss = totalRevenue - totalExpenses
        });
    }

    private (DateTime Start, DateTime End) GetPeriodDates(ProfitLossRequestDto request)
    {
        DateTime periodStart;
        DateTime periodEnd;

        if (request.Year.HasValue && request.Month.HasValue)
        {
            periodStart = new DateTime(request.Year.Value, request.Month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            periodEnd = new DateTime(request.Year.Value, request.Month.Value,
                DateTime.DaysInMonth(request.Year.Value, request.Month.Value), 23, 59, 59, DateTimeKind.Utc);
        }
        else if (request.Year.HasValue)
        {
            periodStart = new DateTime(request.Year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            periodEnd = new DateTime(request.Year.Value, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }
        else
        {
            var now = DateTime.UtcNow;
            periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            periodEnd = new DateTime(now.Year, now.Month,
                DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59, DateTimeKind.Utc);
        }

        return (periodStart, periodEnd);
    }
}

public class ProfitLossRequestDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class ProfitLossPeriodRequestDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string PeriodType { get; set; } = "Monthly";
    public int NumberOfPeriods { get; set; } = 12;
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class ProfitLossLineItemDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public decimal Amount { get; set; }
}

public class ProfitLossResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<ProfitLossLineItemDto> RevenueItems { get; set; } = new();
    public List<ProfitLossLineItemDto> ExpenseItems { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfitLoss { get; set; }
}

public class PeriodColumnDto
{
    public int PeriodIndex { get; set; }
    public string PeriodLabel { get; set; } = "";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class ProfitLossPeriodItemDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public List<decimal> PeriodAmounts { get; set; } = new();
}

public class ProfitLossPeriodResultDto
{
    public string ReportType { get; set; } = "";
    public string PeriodType { get; set; } = "";
    public List<PeriodColumnDto> Periods { get; set; } = new();
    public List<ProfitLossPeriodItemDto> RevenueItems { get; set; } = new();
    public List<ProfitLossPeriodItemDto> ExpenseItems { get; set; } = new();
    public List<decimal> RevenueTotals { get; set; } = new();
    public List<decimal> ExpenseTotals { get; set; } = new();
    public List<decimal> NetProfitLossTotals { get; set; } = new();
}

public class ProfitLossTransactionDto
{
    public DateTime EntryDate { get; set; }
    public string VoucherNumber { get; set; } = "";
    public string Reference { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Amount { get; set; }
}

public class ProfitLossItemWiseAccountDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string AccountType { get; set; } = "";
    public List<ProfitLossTransactionDto> Transactions { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class ProfitLossItemWiseResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<ProfitLossItemWiseAccountDto> RevenueAccounts { get; set; } = new();
    public List<ProfitLossItemWiseAccountDto> ExpenseAccounts { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfitLoss { get; set; }
}
