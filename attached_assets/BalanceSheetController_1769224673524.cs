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
public class BalanceSheetController : ControllerBase
{
    private readonly AppDbContext _context;

    public BalanceSheetController(AppDbContext context)
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

    [HttpPost("vertical")]
    public async Task<ActionResult<BalanceSheetResultDto>> GetVerticalBalanceSheet(
        [FromBody] BalanceSheetRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var asAtDate = GetAsAtDate(request);

        var (assets, liabilities, equity, retainedEarnings) = await GetBalanceSheetData(tenantId, asAtDate, request.BranchId, request.DepartmentId);

        return Ok(new BalanceSheetResultDto
        {
            ReportType = "Vertical",
            AsAtDate = asAtDate,
            Assets = assets,
            Liabilities = liabilities,
            Equity = equity,
            RetainedEarnings = retainedEarnings,
            TotalAssets = assets.Sum(x => x.Balance),
            TotalLiabilities = liabilities.Sum(x => x.Balance),
            TotalEquity = equity.Sum(x => x.Balance) + retainedEarnings,
            TotalLiabilitiesAndEquity = liabilities.Sum(x => x.Balance) + equity.Sum(x => x.Balance) + retainedEarnings
        });
    }

    [HttpPost("horizontal")]
    public async Task<ActionResult<BalanceSheetHorizontalResultDto>> GetHorizontalBalanceSheet(
        [FromBody] BalanceSheetRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var asAtDate = GetAsAtDate(request);

        var (assets, liabilities, equity, retainedEarnings) = await GetBalanceSheetData(tenantId, asAtDate, request.BranchId, request.DepartmentId);

        var currentAssets = assets.Where(a => a.IsCurrentAsset).ToList();
        var nonCurrentAssets = assets.Where(a => !a.IsCurrentAsset).ToList();
        var currentLiabilities = liabilities.Where(l => l.IsCurrentLiability).ToList();
        var nonCurrentLiabilities = liabilities.Where(l => !l.IsCurrentLiability).ToList();

        return Ok(new BalanceSheetHorizontalResultDto
        {
            ReportType = "Horizontal",
            AsAtDate = asAtDate,
            CurrentAssets = currentAssets,
            NonCurrentAssets = nonCurrentAssets,
            CurrentLiabilities = currentLiabilities,
            NonCurrentLiabilities = nonCurrentLiabilities,
            Equity = equity,
            RetainedEarnings = retainedEarnings,
            TotalCurrentAssets = currentAssets.Sum(x => x.Balance),
            TotalNonCurrentAssets = nonCurrentAssets.Sum(x => x.Balance),
            TotalAssets = assets.Sum(x => x.Balance),
            TotalCurrentLiabilities = currentLiabilities.Sum(x => x.Balance),
            TotalNonCurrentLiabilities = nonCurrentLiabilities.Sum(x => x.Balance),
            TotalLiabilities = liabilities.Sum(x => x.Balance),
            TotalEquity = equity.Sum(x => x.Balance) + retainedEarnings,
            TotalLiabilitiesAndEquity = liabilities.Sum(x => x.Balance) + equity.Sum(x => x.Balance) + retainedEarnings
        });
    }

    [HttpPost("groupwise")]
    public async Task<ActionResult<BalanceSheetGroupWiseResultDto>> GetGroupWiseBalanceSheet(
        [FromBody] BalanceSheetRequestDto request)
    {
        var tenantId = GetCurrentTenantId();
        var asAtDate = GetAsAtDate(request);

        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive
                && (a.AccountType == AccountType.Asset
                    || a.AccountType == AccountType.Liability
                    || a.AccountType == AccountType.Equity))
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountCode)
            .Select(a => new { a.Id, a.AccountCode, a.AccountName, a.AccountType, a.ParentAccountId })
            .ToListAsync();

        var journalLinesQuery = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate <= asAtDate);

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

        var retainedEarnings = await CalculateRetainedEarnings(tenantId, asAtDate, request.BranchId, request.DepartmentId);

        var groups = new List<BalanceSheetGroupDto>();

        var accountsByType = accounts.GroupBy(a => a.AccountType);
        foreach (var typeGroup in accountsByType.OrderBy(g => GetAccountTypeOrder(g.Key)))
        {
            var parentAccounts = typeGroup.Where(a => a.ParentAccountId == null).ToList();
            var childAccounts = typeGroup.Where(a => a.ParentAccountId != null).ToList();

            var groupItems = new List<BalanceSheetGroupItemDto>();

            foreach (var parent in parentAccounts)
            {
                var children = childAccounts.Where(c => c.ParentAccountId == parent.Id).ToList();
                var parentBalance = balanceDict.GetValueOrDefault(parent.Id);
                var parentNetBalance = CalculateNetBalance(parent.AccountType, parentBalance?.TotalDebit ?? 0, parentBalance?.TotalCredit ?? 0);

                var childItems = children.Select(child =>
                {
                    var childBalance = balanceDict.GetValueOrDefault(child.Id);
                    var childNetBalance = CalculateNetBalance(child.AccountType, childBalance?.TotalDebit ?? 0, childBalance?.TotalCredit ?? 0);
                    return new BalanceSheetLineItemDto
                    {
                        AccountId = child.Id,
                        AccountCode = child.AccountCode ?? "",
                        AccountName = child.AccountName,
                        Balance = childNetBalance
                    };
                }).Where(c => c.Balance != 0).ToList();

                var totalBalance = parentNetBalance + childItems.Sum(c => c.Balance);

                if (totalBalance != 0 || childItems.Any())
                {
                    groupItems.Add(new BalanceSheetGroupItemDto
                    {
                        ParentAccountId = parent.Id,
                        ParentAccountCode = parent.AccountCode ?? "",
                        ParentAccountName = parent.AccountName,
                        ParentBalance = parentNetBalance,
                        Children = childItems,
                        TotalBalance = totalBalance
                    });
                }
            }

            var ungroupedAccounts = childAccounts
                .Where(c => !parentAccounts.Any(p => p.Id == c.ParentAccountId))
                .ToList();

            foreach (var account in ungroupedAccounts)
            {
                var balance = balanceDict.GetValueOrDefault(account.Id);
                var netBalance = CalculateNetBalance(account.AccountType, balance?.TotalDebit ?? 0, balance?.TotalCredit ?? 0);

                if (netBalance != 0)
                {
                    groupItems.Add(new BalanceSheetGroupItemDto
                    {
                        ParentAccountId = account.Id,
                        ParentAccountCode = account.AccountCode ?? "",
                        ParentAccountName = account.AccountName,
                        ParentBalance = netBalance,
                        Children = new List<BalanceSheetLineItemDto>(),
                        TotalBalance = netBalance
                    });
                }
            }

            if (groupItems.Any())
            {
                groups.Add(new BalanceSheetGroupDto
                {
                    AccountType = typeGroup.Key.ToString(),
                    Items = groupItems,
                    GroupTotal = groupItems.Sum(x => x.TotalBalance)
                });
            }
        }

        var assetGroup = groups.FirstOrDefault(g => g.AccountType == "Asset");
        var liabilityGroup = groups.FirstOrDefault(g => g.AccountType == "Liability");
        var equityGroup = groups.FirstOrDefault(g => g.AccountType == "Equity");

        return Ok(new BalanceSheetGroupWiseResultDto
        {
            ReportType = "GroupWise",
            AsAtDate = asAtDate,
            Groups = groups,
            RetainedEarnings = retainedEarnings,
            TotalAssets = assetGroup?.GroupTotal ?? 0,
            TotalLiabilities = liabilityGroup?.GroupTotal ?? 0,
            TotalEquity = (equityGroup?.GroupTotal ?? 0) + retainedEarnings,
            TotalLiabilitiesAndEquity = (liabilityGroup?.GroupTotal ?? 0) + (equityGroup?.GroupTotal ?? 0) + retainedEarnings
        });
    }

    private async Task<(List<BalanceSheetLineItemDto> assets, List<BalanceSheetLineItemDto> liabilities, 
        List<BalanceSheetLineItemDto> equity, decimal retainedEarnings)> GetBalanceSheetData(
            Guid tenantId, DateTime asAtDate, Guid? branchId = null, Guid? departmentId = null)
    {
        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive && a.AllowPosting
                && (a.AccountType == AccountType.Asset
                    || a.AccountType == AccountType.Liability
                    || a.AccountType == AccountType.Equity))
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountCode)
            .Select(a => new { a.Id, a.AccountCode, a.AccountName, a.AccountType })
            .ToListAsync();

        var journalLinesQuery = _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Where(jel => jel.TenantId == tenantId
                && jel.JournalEntry.Status == JournalEntryStatus.Posted
                && !jel.JournalEntry.IsVoided
                && jel.JournalEntry.EntryDate <= asAtDate);

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

        var assets = new List<BalanceSheetLineItemDto>();
        var liabilities = new List<BalanceSheetLineItemDto>();
        var equity = new List<BalanceSheetLineItemDto>();

        foreach (var account in accounts)
        {
            var balance = balanceDict.GetValueOrDefault(account.Id);
            var debit = balance?.TotalDebit ?? 0m;
            var credit = balance?.TotalCredit ?? 0m;

            decimal netBalance = CalculateNetBalance(account.AccountType, debit, credit);

            if (netBalance == 0) continue;

            var item = new BalanceSheetLineItemDto
            {
                AccountId = account.Id,
                AccountCode = account.AccountCode ?? "",
                AccountName = account.AccountName,
                Balance = netBalance,
                IsCurrentAsset = account.AccountCode?.StartsWith("1") == true,
                IsCurrentLiability = account.AccountCode?.StartsWith("2") == true && account.AccountCode?.Length > 1 && account.AccountCode[1] == '1'
            };

            switch (account.AccountType)
            {
                case AccountType.Asset:
                    assets.Add(item);
                    break;
                case AccountType.Liability:
                    liabilities.Add(item);
                    break;
                case AccountType.Equity:
                    equity.Add(item);
                    break;
            }
        }

        var retainedEarnings = await CalculateRetainedEarnings(tenantId, asAtDate, branchId, departmentId);

        return (assets, liabilities, equity, retainedEarnings);
    }

    private async Task<decimal> CalculateRetainedEarnings(Guid tenantId, DateTime asAtDate, 
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
                && jel.JournalEntry.EntryDate <= asAtDate);

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

    private static decimal CalculateNetBalance(AccountType accountType, decimal debit, decimal credit)
    {
        return accountType switch
        {
            AccountType.Asset => debit - credit,
            AccountType.Liability => credit - debit,
            AccountType.Equity => credit - debit,
            _ => 0
        };
    }

    private static int GetAccountTypeOrder(AccountType type)
    {
        return type switch
        {
            AccountType.Asset => 1,
            AccountType.Liability => 2,
            AccountType.Equity => 3,
            _ => 4
        };
    }

    private DateTime GetAsAtDate(BalanceSheetRequestDto request)
    {
        if (request.Month == 0)
        {
            return new DateTime(request.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }
        return new DateTime(request.Year, request.Month,
            DateTime.DaysInMonth(request.Year, request.Month), 23, 59, 59, DateTimeKind.Utc);
    }
}

public class BalanceSheetRequestDto
{
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public int Month { get; set; } = DateTime.UtcNow.Month;
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class BalanceSheetLineItemDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public decimal Balance { get; set; }
    public bool IsCurrentAsset { get; set; }
    public bool IsCurrentLiability { get; set; }
}

public class BalanceSheetResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime AsAtDate { get; set; }
    public List<BalanceSheetLineItemDto> Assets { get; set; } = new();
    public List<BalanceSheetLineItemDto> Liabilities { get; set; } = new();
    public List<BalanceSheetLineItemDto> Equity { get; set; } = new();
    public decimal RetainedEarnings { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal TotalLiabilitiesAndEquity { get; set; }
}

public class BalanceSheetHorizontalResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime AsAtDate { get; set; }
    public List<BalanceSheetLineItemDto> CurrentAssets { get; set; } = new();
    public List<BalanceSheetLineItemDto> NonCurrentAssets { get; set; } = new();
    public List<BalanceSheetLineItemDto> CurrentLiabilities { get; set; } = new();
    public List<BalanceSheetLineItemDto> NonCurrentLiabilities { get; set; } = new();
    public List<BalanceSheetLineItemDto> Equity { get; set; } = new();
    public decimal RetainedEarnings { get; set; }
    public decimal TotalCurrentAssets { get; set; }
    public decimal TotalNonCurrentAssets { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalCurrentLiabilities { get; set; }
    public decimal TotalNonCurrentLiabilities { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal TotalLiabilitiesAndEquity { get; set; }
}

public class BalanceSheetGroupDto
{
    public string AccountType { get; set; } = "";
    public List<BalanceSheetGroupItemDto> Items { get; set; } = new();
    public decimal GroupTotal { get; set; }
}

public class BalanceSheetGroupItemDto
{
    public Guid ParentAccountId { get; set; }
    public string ParentAccountCode { get; set; } = "";
    public string ParentAccountName { get; set; } = "";
    public decimal ParentBalance { get; set; }
    public List<BalanceSheetLineItemDto> Children { get; set; } = new();
    public decimal TotalBalance { get; set; }
}

public class BalanceSheetGroupWiseResultDto
{
    public string ReportType { get; set; } = "";
    public DateTime AsAtDate { get; set; }
    public List<BalanceSheetGroupDto> Groups { get; set; } = new();
    public decimal RetainedEarnings { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal TotalLiabilitiesAndEquity { get; set; }
}
