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
public class FundFlowController : ControllerBase
{
    private readonly AppDbContext _context;

    public FundFlowController(AppDbContext context)
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

    [HttpPost("statement")]
    public async Task<ActionResult<FundFlowResultDto>> GetFundFlowStatement(
        [FromBody] FundFlowRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var (periodStart, periodEnd) = GetPeriodDates(request);
        var priorPeriodEnd = periodStart.AddDays(-1);

        var fundFromOperations = await CalculateFundFromOperations(tenantId, periodStart, periodEnd);

        var sources = new List<FundFlowItemDto>();
        var applications = new List<FundFlowItemDto>();

        if (fundFromOperations > 0)
        {
            sources.Add(new FundFlowItemDto
            {
                Category = "Operations",
                Description = "Fund from Operations",
                Amount = fundFromOperations
            });
        }
        else if (fundFromOperations < 0)
        {
            applications.Add(new FundFlowItemDto
            {
                Category = "Operations",
                Description = "Fund used in Operations",
                Amount = Math.Abs(fundFromOperations)
            });
        }

        var equityAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Equity
                && !a.AccountName.ToLower().Contains("retained"))
            .Select(a => new { a.Id, a.AccountName })
            .ToListAsync();

        foreach (var account in equityAccounts)
        {
            var change = await CalculateAccountChange(tenantId, account.Id, AccountType.Equity, priorPeriodEnd, periodEnd);
            if (change > 0)
            {
                sources.Add(new FundFlowItemDto
                {
                    Category = "Equity",
                    Description = $"Increase in {account.AccountName}",
                    Amount = change
                });
            }
            else if (change < 0)
            {
                applications.Add(new FundFlowItemDto
                {
                    Category = "Equity",
                    Description = $"Decrease in {account.AccountName}",
                    Amount = Math.Abs(change)
                });
            }
        }

        var longTermLiabilities = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Liability
                && (a.AccountName.ToLower().Contains("long-term")
                    || a.AccountName.ToLower().Contains("long term")
                    || a.AccountName.ToLower().Contains("loan")
                    || a.AccountName.ToLower().Contains("debenture")
                    || a.AccountName.ToLower().Contains("bond")))
            .Select(a => new { a.Id, a.AccountName })
            .ToListAsync();

        foreach (var account in longTermLiabilities)
        {
            var change = await CalculateAccountChange(tenantId, account.Id, AccountType.Liability, priorPeriodEnd, periodEnd);
            if (change > 0)
            {
                sources.Add(new FundFlowItemDto
                {
                    Category = "Long-term Borrowings",
                    Description = $"Increase in {account.AccountName}",
                    Amount = change
                });
            }
            else if (change < 0)
            {
                applications.Add(new FundFlowItemDto
                {
                    Category = "Long-term Borrowings",
                    Description = $"Repayment of {account.AccountName}",
                    Amount = Math.Abs(change)
                });
            }
        }

        var fixedAssets = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Asset
                && (a.AccountName.ToLower().Contains("fixed")
                    || a.AccountName.ToLower().Contains("equipment")
                    || a.AccountName.ToLower().Contains("property")
                    || a.AccountName.ToLower().Contains("plant")
                    || a.AccountName.ToLower().Contains("vehicle")
                    || a.AccountCode!.StartsWith("15")
                    || a.AccountCode.StartsWith("16")))
            .Select(a => new { a.Id, a.AccountName })
            .ToListAsync();

        foreach (var account in fixedAssets)
        {
            var change = await CalculateAccountChange(tenantId, account.Id, AccountType.Asset, priorPeriodEnd, periodEnd);
            if (change > 0)
            {
                applications.Add(new FundFlowItemDto
                {
                    Category = "Fixed Assets",
                    Description = $"Purchase of {account.AccountName}",
                    Amount = change
                });
            }
            else if (change < 0)
            {
                sources.Add(new FundFlowItemDto
                {
                    Category = "Fixed Assets",
                    Description = $"Sale of {account.AccountName}",
                    Amount = Math.Abs(change)
                });
            }
        }

        var workingCapitalChanges = await CalculateWorkingCapitalChanges(tenantId, priorPeriodEnd, periodEnd);

        var totalSources = sources.Sum(s => s.Amount);
        var totalApplications = applications.Sum(a => a.Amount);
        var netChangeInWorkingCapital = workingCapitalChanges.Sum(w => w.Change);

        return Ok(new FundFlowResultDto
        {
            ReportType = "FundFlow",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Sources = sources,
            TotalSources = totalSources,
            Applications = applications,
            TotalApplications = totalApplications,
            WorkingCapitalChanges = workingCapitalChanges,
            NetChangeInWorkingCapital = netChangeInWorkingCapital,
            BalanceCheck = totalSources - totalApplications - netChangeInWorkingCapital
        });
    }

    private async Task<decimal> CalculateFundFromOperations(Guid tenantId, DateTime periodStart, DateTime periodEnd)
    {
        var revenueExpenseAccounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && (a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense))
            .Select(a => new { a.Id, a.AccountType, a.AccountName })
            .ToListAsync();

        var accountIds = revenueExpenseAccounts.Select(a => a.Id).ToList();

        var journalLines = await _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && accountIds.Contains(jel.AccountId)
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate >= periodStart
                && jel.JournalEntry.EntryDate <= periodEnd)
            .GroupBy(jel => jel.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                TotalDebit = g.Sum(x => x.Debit),
                TotalCredit = g.Sum(x => x.Credit)
            })
            .ToListAsync();

        var balanceDict = journalLines.ToDictionary(x => x.AccountId, x => x);

        decimal netIncome = 0;
        decimal depreciation = 0;

        foreach (var account in revenueExpenseAccounts)
        {
            var balance = balanceDict.GetValueOrDefault(account.Id);
            if (balance == null) continue;

            if (account.AccountType == AccountType.Revenue)
            {
                netIncome += balance.TotalCredit - balance.TotalDebit;
            }
            else
            {
                var expense = balance.TotalDebit - balance.TotalCredit;
                netIncome -= expense;

                if (account.AccountName.ToLower().Contains("depreciation")
                    || account.AccountName.ToLower().Contains("amortization"))
                {
                    depreciation += expense;
                }
            }
        }

        return netIncome + depreciation;
    }

    private async Task<decimal> CalculateAccountChange(Guid tenantId, Guid accountId, AccountType accountType, DateTime priorEnd, DateTime currentEnd)
    {
        var priorBalance = await _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.AccountId == accountId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate <= priorEnd)
            .SumAsync(jel => jel.Debit - jel.Credit);

        var currentBalance = await _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.AccountId == accountId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate <= currentEnd)
            .SumAsync(jel => jel.Debit - jel.Credit);

        if (accountType == AccountType.Liability || accountType == AccountType.Equity)
        {
            return -(currentBalance - priorBalance);
        }

        return currentBalance - priorBalance;
    }

    private async Task<List<FundFlowWorkingCapitalDto>> CalculateWorkingCapitalChanges(Guid tenantId, DateTime priorEnd, DateTime currentEnd)
    {
        var changes = new List<FundFlowWorkingCapitalDto>();

        var currentAssets = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Asset
                && !a.AccountName.ToLower().Contains("fixed")
                && !a.AccountName.ToLower().Contains("equipment")
                && !a.AccountName.ToLower().Contains("property")
                && !a.AccountName.ToLower().Contains("plant")
                && !a.AccountName.ToLower().Contains("vehicle")
                && !(a.AccountCode!.StartsWith("15") || a.AccountCode.StartsWith("16")))
            .Select(a => new { a.Id, a.AccountName })
            .ToListAsync();

        foreach (var account in currentAssets)
        {
            var change = await CalculateAccountChange(tenantId, account.Id, AccountType.Asset, priorEnd, currentEnd);
            if (change != 0)
            {
                changes.Add(new FundFlowWorkingCapitalDto
                {
                    AccountName = account.AccountName,
                    OpeningBalance = await GetAccountBalance(tenantId, account.Id, AccountType.Asset, priorEnd),
                    ClosingBalance = await GetAccountBalance(tenantId, account.Id, AccountType.Asset, currentEnd),
                    Change = change,
                    Category = "Current Asset"
                });
            }
        }

        var currentLiabilities = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && a.AccountType == AccountType.Liability
                && !a.AccountName.ToLower().Contains("long-term")
                && !a.AccountName.ToLower().Contains("long term")
                && !a.AccountName.ToLower().Contains("loan")
                && !a.AccountName.ToLower().Contains("debenture")
                && !a.AccountName.ToLower().Contains("bond"))
            .Select(a => new { a.Id, a.AccountName })
            .ToListAsync();

        foreach (var account in currentLiabilities)
        {
            var change = await CalculateAccountChange(tenantId, account.Id, AccountType.Liability, priorEnd, currentEnd);
            if (change != 0)
            {
                changes.Add(new FundFlowWorkingCapitalDto
                {
                    AccountName = account.AccountName,
                    OpeningBalance = await GetAccountBalance(tenantId, account.Id, AccountType.Liability, priorEnd),
                    ClosingBalance = await GetAccountBalance(tenantId, account.Id, AccountType.Liability, currentEnd),
                    Change = -change,
                    Category = "Current Liability"
                });
            }
        }

        return changes;
    }

    private async Task<decimal> GetAccountBalance(Guid tenantId, Guid accountId, AccountType accountType, DateTime asAtDate)
    {
        var balance = await _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.AccountId == accountId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate <= asAtDate)
            .SumAsync(jel => jel.Debit - jel.Credit);

        if (accountType == AccountType.Liability || accountType == AccountType.Equity)
        {
            return -balance;
        }

        return balance;
    }

    private (DateTime periodStart, DateTime periodEnd) GetPeriodDates(FundFlowRequestDto request)
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

public class FundFlowRequestDto
{
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public int Month { get; set; } = 0;
}

public class FundFlowItemDto
{
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
}

public class FundFlowWorkingCapitalDto
{
    public string AccountName { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal Change { get; set; }
}

public class FundFlowResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<FundFlowItemDto> Sources { get; set; } = new();
    public decimal TotalSources { get; set; }
    public List<FundFlowItemDto> Applications { get; set; } = new();
    public decimal TotalApplications { get; set; }
    public List<FundFlowWorkingCapitalDto> WorkingCapitalChanges { get; set; } = new();
    public decimal NetChangeInWorkingCapital { get; set; }
    public decimal BalanceCheck { get; set; }
}
