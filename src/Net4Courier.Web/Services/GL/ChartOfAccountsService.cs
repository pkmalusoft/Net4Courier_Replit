using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Contracts.DTOs;
using Net4Courier.Web.Interfaces;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Finance.Entities;

namespace Net4Courier.Web.Services.GL;

public class ChartOfAccountsService : IChartOfAccountsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public ChartOfAccountsService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetAllAsync(Guid tenantId)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        return await context.GLChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .Select(c => new ChartOfAccountDto(
                LongToGuid(c.Id),
                tenantId,
                c.AccountCode,
                c.AccountName,
                MapAccountType(c.AccountType),
                c.ParentId.HasValue ? LongToGuid(c.ParentId.Value) : null,
                null,
                c.IsActive,
                c.AllowPosting,
                c.AccountClassificationId.HasValue ? LongToGuid(c.AccountClassificationId.Value) : null,
                c.AccountClassification != null ? c.AccountClassification.Name : null,
                c.ControlAccountType ?? 0,
                c.IsSystemAccount,
                c.CreatedAt,
                c.ModifiedAt
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetByTypeAsync(Guid tenantId, Truebooks.Platform.Contracts.DTOs.AccountType accountType)
    {
        var typeStr = accountType.ToString();
        await using var context = await _dbFactory.CreateDbContextAsync();
        return await context.GLChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => !c.IsDeleted && c.AccountType == typeStr)
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .Select(c => new ChartOfAccountDto(
                LongToGuid(c.Id),
                tenantId,
                c.AccountCode,
                c.AccountName,
                MapAccountType(c.AccountType),
                c.ParentId.HasValue ? LongToGuid(c.ParentId.Value) : null,
                null,
                c.IsActive,
                c.AllowPosting,
                c.AccountClassificationId.HasValue ? LongToGuid(c.AccountClassificationId.Value) : null,
                c.AccountClassification != null ? c.AccountClassification.Name : null,
                c.ControlAccountType ?? 0,
                c.IsSystemAccount,
                c.CreatedAt,
                c.ModifiedAt
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetPostableAccountsAsync(Guid tenantId)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        return await context.GLChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => !c.IsDeleted && c.AllowPosting && c.IsActive)
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .Select(c => new ChartOfAccountDto(
                LongToGuid(c.Id),
                tenantId,
                c.AccountCode,
                c.AccountName,
                MapAccountType(c.AccountType),
                c.ParentId.HasValue ? LongToGuid(c.ParentId.Value) : null,
                null,
                c.IsActive,
                c.AllowPosting,
                c.AccountClassificationId.HasValue ? LongToGuid(c.AccountClassificationId.Value) : null,
                c.AccountClassification != null ? c.AccountClassification.Name : null,
                c.ControlAccountType ?? 0,
                c.IsSystemAccount,
                c.CreatedAt,
                c.ModifiedAt
            ))
            .ToListAsync();
    }

    public async Task<ChartOfAccountDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();
        var entity = await context.GLChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => c.Id == longId && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (entity == null)
            return null;

        string? parentName = null;
        if (entity.ParentId.HasValue)
        {
            parentName = await context.GLChartOfAccounts
                .Where(c => c.Id == entity.ParentId.Value)
                .Select(c => c.AccountName)
                .FirstOrDefaultAsync();
        }

        return new ChartOfAccountDto(
            LongToGuid(entity.Id),
            tenantId,
            entity.AccountCode,
            entity.AccountName,
            MapAccountType(entity.AccountType),
            entity.ParentId.HasValue ? LongToGuid(entity.ParentId.Value) : null,
            parentName,
            entity.IsActive,
            entity.AllowPosting,
            entity.AccountClassificationId.HasValue ? LongToGuid(entity.AccountClassificationId.Value) : null,
            entity.AccountClassification?.Name,
            entity.ControlAccountType ?? 0,
            entity.IsSystemAccount,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task<ChartOfAccountDto> CreateAsync(Guid tenantId, CreateChartOfAccountRequest request)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var entity = new GLChartOfAccount
        {
            AccountCode = request.AccountCode,
            AccountName = request.AccountName,
            AccountType = request.AccountType.ToString(),
            ParentId = request.ParentAccountId.HasValue ? GuidToLong(request.ParentAccountId.Value) : null,
            AllowPosting = request.AllowPosting,
            AccountClassificationId = request.AccountClassificationId.HasValue ? GuidToLong(request.AccountClassificationId.Value) : null,
            ControlAccountType = request.ControlAccountType,
            IsActive = request.IsActive,
            IsSystemAccount = false,
            CreatedAt = DateTime.UtcNow
        };

        context.GLChartOfAccounts.Add(entity);
        await context.SaveChangesAsync();

        return await GetByIdAsync(tenantId, LongToGuid(entity.Id)) ?? throw new InvalidOperationException("Failed to create account");
    }

    public async Task<ChartOfAccountDto> UpdateAsync(Guid tenantId, Guid id, UpdateChartOfAccountRequest request)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();
        var entity = await context.GLChartOfAccounts
            .FirstOrDefaultAsync(c => c.Id == longId && !c.IsDeleted);

        if (entity == null)
            throw new InvalidOperationException("Account not found");

        if (entity.IsSystemAccount)
            throw new InvalidOperationException("Cannot modify system accounts");

        entity.AccountCode = request.AccountCode;
        entity.AccountName = request.AccountName;
        entity.AccountType = request.AccountType.ToString();
        entity.ParentId = request.ParentAccountId.HasValue ? GuidToLong(request.ParentAccountId.Value) : null;
        entity.AllowPosting = request.AllowPosting;
        entity.AccountClassificationId = request.AccountClassificationId.HasValue ? GuidToLong(request.AccountClassificationId.Value) : null;
        entity.ControlAccountType = request.ControlAccountType;
        entity.IsActive = request.IsActive;
        entity.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return await GetByIdAsync(tenantId, id) ?? throw new InvalidOperationException("Failed to update account");
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        var longId = GuidToLong(id);
        await using var context = await _dbFactory.CreateDbContextAsync();
        var entity = await context.GLChartOfAccounts
            .FirstOrDefaultAsync(c => c.Id == longId && !c.IsDeleted);

        if (entity == null)
            return;

        if (entity.IsSystemAccount)
            throw new InvalidOperationException("Cannot delete system accounts");

        if (await HasTransactionsAsync(tenantId, id))
            throw new InvalidOperationException("Cannot delete account with existing transactions");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.ModifiedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
    }

    public async Task<bool> HasTransactionsAsync(Guid tenantId, Guid accountId)
    {
        var longId = GuidToLong(accountId);
        await using var context = await _dbFactory.CreateDbContextAsync();
        return await context.JournalEntries.AnyAsync(j => j.AccountHeadId == longId);
    }

    public async Task<IEnumerable<ChartOfAccountHierarchyDto>> GetHierarchyAsync(Guid tenantId)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var accounts = await context.GLChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .ToListAsync();

        var rootAccounts = accounts.Where(a => a.ParentId == null);
        return rootAccounts.Select(a => BuildHierarchy(a, accounts, tenantId)).ToList();
    }

    private ChartOfAccountHierarchyDto BuildHierarchy(GLChartOfAccount account, List<GLChartOfAccount> allAccounts, Guid tenantId)
    {
        var children = allAccounts.Where(a => a.ParentId == account.Id).ToList();
        return new ChartOfAccountHierarchyDto(
            LongToGuid(account.Id),
            tenantId,
            account.AccountCode,
            account.AccountName,
            MapAccountType(account.AccountType),
            account.ParentId.HasValue ? LongToGuid(account.ParentId.Value) : null,
            null,
            account.IsActive,
            account.AllowPosting,
            account.AccountClassificationId.HasValue ? LongToGuid(account.AccountClassificationId.Value) : null,
            account.AccountClassification?.Name,
            account.ControlAccountType ?? 0,
            account.IsSystemAccount,
            account.CreatedAt,
            account.ModifiedAt,
            children.Select(c => BuildHierarchy(c, allAccounts, tenantId)).ToList()
        );
    }

    public async Task<bool> HasAccountsAsync(Guid tenantId)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        return await context.GLChartOfAccounts.AnyAsync(c => !c.IsDeleted);
    }

    public async Task<bool> CodeExistsAsync(Guid tenantId, string code, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        await using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.GLChartOfAccounts.Where(c => c.AccountCode == code && !c.IsDeleted);
        if (excludeId.HasValue)
        {
            var longId = GuidToLong(excludeId.Value);
            query = query.Where(c => c.Id != longId);
        }
        return await query.AnyAsync();
    }

    public async Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        await using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.GLChartOfAccounts.Where(c => c.AccountName == name && !c.IsDeleted);
        if (excludeId.HasValue)
        {
            var longId = GuidToLong(excludeId.Value);
            query = query.Where(c => c.Id != longId);
        }
        return await query.AnyAsync();
    }

    public Task SeedFromTemplateAsync(Guid tenantId, string industryType)
    {
        return Task.CompletedTask;
    }

    public Task ResetFromTemplateAsync(Guid tenantId, string industryType)
    {
        return Task.CompletedTask;
    }

    public Task<byte[]?> ExportPdfAsync(Guid tenantId)
    {
        return Task.FromResult<byte[]?>(null);
    }

    public Task<byte[]?> ExportExcelAsync(Guid tenantId)
    {
        return Task.FromResult<byte[]?>(null);
    }

    public async Task<Dictionary<string, ControlAccountAssignmentDto?>> GetControlAccountsAsync(Guid tenantId)
    {
        var result = new Dictionary<string, ControlAccountAssignmentDto?>
        {
            { "AccountsReceivable", null },
            { "AccountsPayable", null },
            { "Inventory", null },
            { "CostOfGoodsSold", null },
            { "RetainedEarnings", null },
            { "OpeningBalanceEquity", null }
        };

        await using var context = await _dbFactory.CreateDbContextAsync();
        var controlAccounts = await context.GLChartOfAccounts
            .Where(c => !c.IsDeleted && c.ControlAccountType > 0)
            .Select(c => new { c.Id, c.AccountCode, c.AccountName, c.ControlAccountType })
            .ToListAsync();

        foreach (var account in controlAccounts)
        {
            var key = account.ControlAccountType switch
            {
                1 => "AccountsReceivable",
                2 => "AccountsPayable",
                3 => "Inventory",
                4 => "CostOfGoodsSold",
                5 => "RetainedEarnings",
                6 => "OpeningBalanceEquity",
                _ => null
            };

            if (key != null)
            {
                result[key] = new ControlAccountAssignmentDto(LongToGuid(account.Id), account.AccountCode ?? "", account.AccountName);
            }
        }

        return result;
    }

    public async Task SetControlAccountAsync(Guid tenantId, Guid accountId, int controlAccountType)
    {
        var longId = GuidToLong(accountId);
        await using var context = await _dbFactory.CreateDbContextAsync();
        
        var existingAccounts = await context.GLChartOfAccounts
            .Where(c => !c.IsDeleted && c.ControlAccountType == controlAccountType)
            .ToListAsync();
        foreach (var acc in existingAccounts)
        {
            acc.ControlAccountType = 0;
        }

        var targetAccount = await context.GLChartOfAccounts
            .FirstOrDefaultAsync(c => c.Id == longId && !c.IsDeleted);
        if (targetAccount != null)
        {
            targetAccount.ControlAccountType = controlAccountType;
        }

        await context.SaveChangesAsync();
    }

    public async Task RemoveControlAccountAsync(Guid tenantId, int controlAccountType)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var accounts = await context.GLChartOfAccounts
            .Where(c => !c.IsDeleted && c.ControlAccountType == controlAccountType)
            .ToListAsync();
        foreach (var acc in accounts)
        {
            acc.ControlAccountType = 0;
        }
        await context.SaveChangesAsync();
    }

    private static Truebooks.Platform.Contracts.DTOs.AccountType MapAccountType(string? type)
    {
        return type?.ToLower() switch
        {
            "asset" => Truebooks.Platform.Contracts.DTOs.AccountType.Asset,
            "liability" => Truebooks.Platform.Contracts.DTOs.AccountType.Liability,
            "equity" => Truebooks.Platform.Contracts.DTOs.AccountType.Equity,
            "revenue" => Truebooks.Platform.Contracts.DTOs.AccountType.Revenue,
            "expense" => Truebooks.Platform.Contracts.DTOs.AccountType.Expense,
            _ => Truebooks.Platform.Contracts.DTOs.AccountType.Asset
        };
    }

    private static Guid LongToGuid(long id)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(id).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    private static long GuidToLong(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToInt64(bytes, 0);
    }
}
