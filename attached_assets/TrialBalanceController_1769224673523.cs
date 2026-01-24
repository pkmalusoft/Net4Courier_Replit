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
public class TrialBalanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public TrialBalanceController(AppDbContext context)
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
    public async Task<ActionResult<TrialBalanceResultDto>> GetStandardTrialBalance(
        [FromBody] TrialBalanceRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var (periodStart, periodEnd) = GetPeriodDates(request);

        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive && a.AllowPosting)
            .OrderBy(a => a.AccountCode)
            .Select(a => new { a.Id, a.AccountCode, a.AccountName, a.AccountType })
            .ToListAsync();

        var journalLinesQuery = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
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

        var items = accounts.Select(a =>
        {
            var balance = balanceDict.GetValueOrDefault(a.Id);
            var debit = balance?.TotalDebit ?? 0m;
            var credit = balance?.TotalCredit ?? 0m;
            var netBalance = debit - credit;

            return new TrialBalanceItemDto
            {
                AccountId = a.Id,
                AccountCode = a.AccountCode ?? "",
                AccountName = a.AccountName,
                AccountType = a.AccountType.ToString(),
                DebitBalance = netBalance > 0 ? netBalance : 0,
                CreditBalance = netBalance < 0 ? Math.Abs(netBalance) : 0
            };
        })
        .Where(x => x.DebitBalance != 0 || x.CreditBalance != 0)
        .ToList();

        return Ok(new TrialBalanceResultDto
        {
            ReportType = "Standard",
            AsAtDate = periodEnd,
            TotalDebit = items.Sum(x => x.DebitBalance),
            TotalCredit = items.Sum(x => x.CreditBalance),
            Items = items
        });
    }

    [HttpPost("detail")]
    public async Task<ActionResult<TrialBalanceDetailResultDto>> GetDetailTrialBalance(
        [FromBody] TrialBalanceRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var (periodStart, periodEnd) = GetPeriodDates(request);

        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive && a.AllowPosting)
            .OrderBy(a => a.AccountCode)
            .Select(a => new { a.Id, a.AccountCode, a.AccountName, a.AccountType })
            .ToListAsync();

        var openingQuery = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate < periodStart);

        if (request.BranchId.HasValue)
            openingQuery = openingQuery.Where(jel => jel.JournalEntry.BranchId == request.BranchId.Value);
        if (request.DepartmentId.HasValue)
            openingQuery = openingQuery.Where(jel => jel.JournalEntry.DepartmentId == request.DepartmentId.Value);

        var openingBalances = await openingQuery
            .GroupBy(jel => jel.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                TotalDebit = g.Sum(x => x.Debit),
                TotalCredit = g.Sum(x => x.Credit)
            })
            .ToDictionaryAsync(x => x.AccountId, x => x.TotalDebit - x.TotalCredit);

        var periodQuery = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate >= periodStart
                && jel.JournalEntry.EntryDate <= periodEnd);

        if (request.BranchId.HasValue)
            periodQuery = periodQuery.Where(jel => jel.JournalEntry.BranchId == request.BranchId.Value);
        if (request.DepartmentId.HasValue)
            periodQuery = periodQuery.Where(jel => jel.JournalEntry.DepartmentId == request.DepartmentId.Value);

        var periodMovements = await periodQuery
            .GroupBy(jel => jel.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                PeriodDebit = g.Sum(x => x.Debit),
                PeriodCredit = g.Sum(x => x.Credit)
            })
            .ToDictionaryAsync(x => x.AccountId, x => x);

        var items = accounts.Select(a =>
        {
            var openingBalance = openingBalances.GetValueOrDefault(a.Id, 0m);
            var movement = periodMovements.GetValueOrDefault(a.Id);
            var periodDebit = movement?.PeriodDebit ?? 0m;
            var periodCredit = movement?.PeriodCredit ?? 0m;
            var closingBalance = openingBalance + periodDebit - periodCredit;

            return new TrialBalanceDetailItemDto
            {
                AccountId = a.Id,
                AccountCode = a.AccountCode ?? "",
                AccountName = a.AccountName,
                AccountType = a.AccountType.ToString(),
                OpeningDebit = openingBalance > 0 ? openingBalance : 0,
                OpeningCredit = openingBalance < 0 ? Math.Abs(openingBalance) : 0,
                PeriodDebit = periodDebit,
                PeriodCredit = periodCredit,
                ClosingDebit = closingBalance > 0 ? closingBalance : 0,
                ClosingCredit = closingBalance < 0 ? Math.Abs(closingBalance) : 0
            };
        })
        .Where(x => x.OpeningDebit != 0 || x.OpeningCredit != 0 || 
                    x.PeriodDebit != 0 || x.PeriodCredit != 0)
        .ToList();

        return Ok(new TrialBalanceDetailResultDto
        {
            ReportType = "Detail",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalOpeningDebit = items.Sum(x => x.OpeningDebit),
            TotalOpeningCredit = items.Sum(x => x.OpeningCredit),
            TotalPeriodDebit = items.Sum(x => x.PeriodDebit),
            TotalPeriodCredit = items.Sum(x => x.PeriodCredit),
            TotalClosingDebit = items.Sum(x => x.ClosingDebit),
            TotalClosingCredit = items.Sum(x => x.ClosingCredit),
            Items = items
        });
    }

    [HttpPost("grouped")]
    public async Task<ActionResult<TrialBalanceGroupedResultDto>> GetGroupedTrialBalance(
        [FromBody] TrialBalanceRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var (periodStart, periodEnd) = GetPeriodDates(request);

        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive && a.AllowPosting)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountCode)
            .Select(a => new { a.Id, a.AccountCode, a.AccountName, a.AccountType })
            .ToListAsync();

        var journalLines = await _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate <= periodEnd)
            .GroupBy(jel => jel.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                TotalDebit = g.Sum(x => x.Debit),
                TotalCredit = g.Sum(x => x.Credit)
            })
            .ToDictionaryAsync(x => x.AccountId, x => x);

        var accountTypeOrder = new[] { AccountType.Asset, AccountType.Liability, AccountType.Equity, AccountType.Revenue, AccountType.Expense };

        var groups = accountTypeOrder.Select(accountType =>
        {
            var groupAccounts = accounts.Where(a => a.AccountType == accountType).ToList();
            var groupItems = groupAccounts.Select(a =>
            {
                var balance = journalLines.GetValueOrDefault(a.Id);
                var debit = balance?.TotalDebit ?? 0m;
                var credit = balance?.TotalCredit ?? 0m;
                var netBalance = debit - credit;

                return new TrialBalanceItemDto
                {
                    AccountId = a.Id,
                    AccountCode = a.AccountCode ?? "",
                    AccountName = a.AccountName,
                    AccountType = a.AccountType.ToString(),
                    DebitBalance = netBalance > 0 ? netBalance : 0,
                    CreditBalance = netBalance < 0 ? Math.Abs(netBalance) : 0
                };
            })
            .Where(x => x.DebitBalance != 0 || x.CreditBalance != 0)
            .ToList();

            return new TrialBalanceGroupDto
            {
                GroupName = accountType.ToString(),
                AccountType = accountType.ToString(),
                Items = groupItems,
                GroupTotalDebit = groupItems.Sum(x => x.DebitBalance),
                GroupTotalCredit = groupItems.Sum(x => x.CreditBalance)
            };
        })
        .Where(g => g.Items.Count > 0)
        .ToList();

        return Ok(new TrialBalanceGroupedResultDto
        {
            ReportType = "Grouped",
            AsAtDate = periodEnd,
            Groups = groups,
            GrandTotalDebit = groups.Sum(g => g.GroupTotalDebit),
            GrandTotalCredit = groups.Sum(g => g.GroupTotalCredit)
        });
    }

    [HttpPost("monthly")]
    public async Task<ActionResult<TrialBalanceMonthlyResultDto>> GetMonthlyTrialBalance(
        [FromBody] TrialBalanceMonthlyRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var year = request.Year ?? DateTime.UtcNow.Year;

        var startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive && a.AllowPosting)
            .OrderBy(a => a.AccountCode)
            .Select(a => new { a.Id, a.AccountCode, a.AccountName, a.AccountType })
            .ToListAsync();

        var openingBalances = await _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate < startDate)
            .GroupBy(jel => jel.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Balance = g.Sum(x => x.Debit) - g.Sum(x => x.Credit)
            })
            .ToDictionaryAsync(x => x.AccountId, x => x.Balance);

        var monthlyData = await _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate >= startDate
                && jel.JournalEntry.EntryDate <= endDate)
            .GroupBy(jel => new { jel.AccountId, Month = jel.JournalEntry.EntryDate.Month })
            .Select(g => new
            {
                g.Key.AccountId,
                g.Key.Month,
                Debit = g.Sum(x => x.Debit),
                Credit = g.Sum(x => x.Credit)
            })
            .ToListAsync();

        var monthlyDict = monthlyData
            .GroupBy(x => x.AccountId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(x => x.Month, x => x.Debit - x.Credit)
            );

        var items = accounts.Select(a =>
        {
            var openingBalance = openingBalances.GetValueOrDefault(a.Id, 0m);
            var accountMonthly = monthlyDict.GetValueOrDefault(a.Id, new Dictionary<int, decimal>());

            var monthlyBalances = new decimal[12];
            var cumulativeBalance = openingBalance;

            for (int month = 1; month <= 12; month++)
            {
                cumulativeBalance += accountMonthly.GetValueOrDefault(month, 0m);
                monthlyBalances[month - 1] = cumulativeBalance;
            }

            var hasActivity = openingBalance != 0 || accountMonthly.Any(m => m.Value != 0);

            return new TrialBalanceMonthlyItemDto
            {
                AccountId = a.Id,
                AccountCode = a.AccountCode ?? "",
                AccountName = a.AccountName,
                AccountType = a.AccountType.ToString(),
                OpeningBalance = openingBalance,
                Month1 = monthlyBalances[0],
                Month2 = monthlyBalances[1],
                Month3 = monthlyBalances[2],
                Month4 = monthlyBalances[3],
                Month5 = monthlyBalances[4],
                Month6 = monthlyBalances[5],
                Month7 = monthlyBalances[6],
                Month8 = monthlyBalances[7],
                Month9 = monthlyBalances[8],
                Month10 = monthlyBalances[9],
                Month11 = monthlyBalances[10],
                Month12 = monthlyBalances[11],
                HasActivity = hasActivity
            };
        })
        .Where(x => x.HasActivity)
        .ToList();

        var monthlyDebitTotals = new List<decimal>();
        var monthlyCreditTotals = new List<decimal>();
        
        for (int i = 0; i < 12; i++)
        {
            var month = i + 1;
            var debitTotal = items.Sum(x => {
                var val = GetMonthValue(x, month);
                return val > 0 ? val : 0;
            });
            var creditTotal = items.Sum(x => {
                var val = GetMonthValue(x, month);
                return val < 0 ? Math.Abs(val) : 0;
            });
            monthlyDebitTotals.Add(debitTotal);
            monthlyCreditTotals.Add(creditTotal);
        }

        return Ok(new TrialBalanceMonthlyResultDto
        {
            ReportType = "Monthly",
            Year = year,
            Items = items,
            MonthlyTotals = Enumerable.Range(0, 12).Select(i => items.Sum(x => GetMonthValue(x, i + 1))).ToList(),
            MonthlyDebitTotals = monthlyDebitTotals,
            MonthlyCreditTotals = monthlyCreditTotals,
            OpeningDebitTotal = items.Sum(x => x.OpeningBalance > 0 ? x.OpeningBalance : 0),
            OpeningCreditTotal = items.Sum(x => x.OpeningBalance < 0 ? Math.Abs(x.OpeningBalance) : 0)
        });
    }

    private static decimal GetMonthValue(TrialBalanceMonthlyItemDto item, int month)
    {
        return month switch
        {
            1 => item.Month1,
            2 => item.Month2,
            3 => item.Month3,
            4 => item.Month4,
            5 => item.Month5,
            6 => item.Month6,
            7 => item.Month7,
            8 => item.Month8,
            9 => item.Month9,
            10 => item.Month10,
            11 => item.Month11,
            12 => item.Month12,
            _ => 0
        };
    }

    private (DateTime Start, DateTime End) GetPeriodDates(TrialBalanceRequestDto request)
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
            periodEnd = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59, DateTimeKind.Utc);
        }

        return (periodStart, periodEnd);
    }
}

public class TrialBalanceRequestDto
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class TrialBalanceMonthlyRequestDto
{
    public int? Year { get; set; }
}

public class TrialBalanceItemDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string AccountType { get; set; } = "";
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
}

public class TrialBalanceResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime AsAtDate { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public List<TrialBalanceItemDto> Items { get; set; } = new();
}

public class TrialBalanceDetailItemDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string AccountType { get; set; } = "";
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}

public class TrialBalanceDetailResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalOpeningDebit { get; set; }
    public decimal TotalOpeningCredit { get; set; }
    public decimal TotalPeriodDebit { get; set; }
    public decimal TotalPeriodCredit { get; set; }
    public decimal TotalClosingDebit { get; set; }
    public decimal TotalClosingCredit { get; set; }
    public List<TrialBalanceDetailItemDto> Items { get; set; } = new();
}

public class TrialBalanceGroupDto
{
    public string GroupName { get; set; } = "";
    public string AccountType { get; set; } = "";
    public List<TrialBalanceItemDto> Items { get; set; } = new();
    public decimal GroupTotalDebit { get; set; }
    public decimal GroupTotalCredit { get; set; }
}

public class TrialBalanceGroupedResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime AsAtDate { get; set; }
    public List<TrialBalanceGroupDto> Groups { get; set; } = new();
    public decimal GrandTotalDebit { get; set; }
    public decimal GrandTotalCredit { get; set; }
}

public class TrialBalanceMonthlyItemDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string AccountType { get; set; } = "";
    public decimal OpeningBalance { get; set; }
    public decimal Month1 { get; set; }
    public decimal Month2 { get; set; }
    public decimal Month3 { get; set; }
    public decimal Month4 { get; set; }
    public decimal Month5 { get; set; }
    public decimal Month6 { get; set; }
    public decimal Month7 { get; set; }
    public decimal Month8 { get; set; }
    public decimal Month9 { get; set; }
    public decimal Month10 { get; set; }
    public decimal Month11 { get; set; }
    public decimal Month12 { get; set; }
    public bool HasActivity { get; set; }
}

public class TrialBalanceMonthlyResultDto
{
    public string ReportType { get; set; } = "";
    public int Year { get; set; }
    public List<TrialBalanceMonthlyItemDto> Items { get; set; } = new();
    public List<decimal> MonthlyTotals { get; set; } = new();
    public List<decimal> MonthlyDebitTotals { get; set; } = new();
    public List<decimal> MonthlyCreditTotals { get; set; } = new();
    public decimal OpeningDebitTotal { get; set; }
    public decimal OpeningCreditTotal { get; set; }
}
