using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Contracts.DTOs;
using Truebooks.Platform.Contracts.Services;
using Truebooks.Platform.Core.Infrastructure;
using CoreAccountType = Truebooks.Platform.Core.Infrastructure.AccountType;
using DtoAccountType = Truebooks.Platform.Contracts.DTOs.AccountType;

namespace Truebooks.Platform.Host.Services;

public class ChartOfAccountsService : IChartOfAccountsService
{
    private readonly IDbContextFactory<PlatformDbContext> _contextFactory;

    public ChartOfAccountsService(IDbContextFactory<PlatformDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Enumerable.Empty<ChartOfAccountDto>();

        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .Select(c => new ChartOfAccountDto(
                c.Id,
                c.TenantId,
                c.AccountCode,
                c.AccountName,
                MapAccountType(c.AccountType),
                c.ParentAccountId,
                null,
                c.IsActive,
                c.AllowPosting,
                c.AccountClassificationId,
                c.AccountClassification != null ? c.AccountClassification.Name : null,
                c.ControlAccountType,
                c.IsSystemAccount,
                c.CreatedAt,
                c.UpdatedAt
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetByTypeAsync(Guid tenantId, Truebooks.Platform.Contracts.DTOs.AccountType accountType)
    {
        if (tenantId == Guid.Empty)
            return Enumerable.Empty<ChartOfAccountDto>();

        var coreType = MapToCoreAccountType(accountType);

        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => c.TenantId == tenantId && c.AccountType == coreType)
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .Select(c => new ChartOfAccountDto(
                c.Id,
                c.TenantId,
                c.AccountCode,
                c.AccountName,
                MapAccountType(c.AccountType),
                c.ParentAccountId,
                null,
                c.IsActive,
                c.AllowPosting,
                c.AccountClassificationId,
                c.AccountClassification != null ? c.AccountClassification.Name : null,
                c.ControlAccountType,
                c.IsSystemAccount,
                c.CreatedAt,
                c.UpdatedAt
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<ChartOfAccountDto>> GetPostableAccountsAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Enumerable.Empty<ChartOfAccountDto>();

        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => c.TenantId == tenantId && c.AllowPosting && c.IsActive)
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .Select(c => new ChartOfAccountDto(
                c.Id,
                c.TenantId,
                c.AccountCode,
                c.AccountName,
                MapAccountType(c.AccountType),
                c.ParentAccountId,
                null,
                c.IsActive,
                c.AllowPosting,
                c.AccountClassificationId,
                c.AccountClassification != null ? c.AccountClassification.Name : null,
                c.ControlAccountType,
                c.IsSystemAccount,
                c.CreatedAt,
                c.UpdatedAt
            ))
            .ToListAsync();
    }

    public async Task<ChartOfAccountDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        if (tenantId == Guid.Empty)
            return null;

        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.ChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => c.TenantId == tenantId && c.Id == id)
            .FirstOrDefaultAsync();

        if (entity == null)
            return null;

        string? parentName = null;
        if (entity.ParentAccountId.HasValue)
        {
            parentName = await context.ChartOfAccounts
                .Where(c => c.Id == entity.ParentAccountId.Value)
                .Select(c => c.AccountName)
                .FirstOrDefaultAsync();
        }

        return new ChartOfAccountDto(
            entity.Id,
            entity.TenantId,
            entity.AccountCode,
            entity.AccountName,
            MapAccountType(entity.AccountType),
            entity.ParentAccountId,
            parentName,
            entity.IsActive,
            entity.AllowPosting,
            entity.AccountClassificationId,
            entity.AccountClassification?.Name,
            entity.ControlAccountType,
            entity.IsSystemAccount,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public async Task<ChartOfAccountDto> CreateAsync(Guid tenantId, CreateChartOfAccountRequest request)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Invalid tenant ID");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountCode = request.AccountCode,
            AccountName = request.AccountName,
            AccountType = MapToCoreAccountType(request.AccountType),
            ParentAccountId = request.ParentAccountId,
            AllowPosting = request.AllowPosting,
            AccountClassificationId = request.AccountClassificationId,
            ControlAccountType = request.ControlAccountType,
            IsActive = request.IsActive,
            IsSystemAccount = false,
            CreatedAt = DateTime.UtcNow
        };

        context.ChartOfAccounts.Add(entity);
        await context.SaveChangesAsync();

        return await GetByIdAsync(tenantId, entity.Id) ?? throw new InvalidOperationException("Failed to create account");
    }

    public async Task<ChartOfAccountDto> UpdateAsync(Guid tenantId, Guid id, UpdateChartOfAccountRequest request)
    {
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Invalid tenant ID");

        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.ChartOfAccounts
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (entity == null)
            throw new InvalidOperationException("Account not found");

        if (entity.IsSystemAccount)
            throw new InvalidOperationException("Cannot modify system accounts");

        entity.AccountCode = request.AccountCode;
        entity.AccountName = request.AccountName;
        entity.AccountType = MapToCoreAccountType(request.AccountType);
        entity.ParentAccountId = request.ParentAccountId;
        entity.AllowPosting = request.AllowPosting;
        entity.AccountClassificationId = request.AccountClassificationId;
        entity.ControlAccountType = request.ControlAccountType;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return await GetByIdAsync(tenantId, id) ?? throw new InvalidOperationException("Failed to update account");
    }

    public async Task DeleteAsync(Guid tenantId, Guid id)
    {
        if (tenantId == Guid.Empty)
            return;

        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.ChartOfAccounts
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (entity == null)
            return;

        if (entity.IsSystemAccount)
            throw new InvalidOperationException("Cannot delete system accounts");

        if (await HasTransactionsAsync(tenantId, id))
            throw new InvalidOperationException("Cannot delete account with existing transactions");

        context.ChartOfAccounts.Remove(entity);
        await context.SaveChangesAsync();
    }

    public async Task<bool> HasTransactionsAsync(Guid tenantId, Guid accountId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.JournalEntryLines
            .AnyAsync(j => j.TenantId == tenantId && j.AccountId == accountId);
    }

    public async Task<IEnumerable<ChartOfAccountHierarchyDto>> GetHierarchyAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return Enumerable.Empty<ChartOfAccountHierarchyDto>();

        await using var context = await _contextFactory.CreateDbContextAsync();
        var accounts = await context.ChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .ToListAsync();

        var rootAccounts = accounts.Where(a => a.ParentAccountId == null);
        return rootAccounts.Select(a => BuildHierarchy(a, accounts)).ToList();
    }

    private ChartOfAccountHierarchyDto BuildHierarchy(ChartOfAccount account, List<ChartOfAccount> allAccounts)
    {
        var children = allAccounts.Where(a => a.ParentAccountId == account.Id).ToList();
        return new ChartOfAccountHierarchyDto(
            account.Id,
            account.TenantId,
            account.AccountCode,
            account.AccountName,
            MapAccountType(account.AccountType),
            account.ParentAccountId,
            null,
            account.IsActive,
            account.AllowPosting,
            account.AccountClassificationId,
            account.AccountClassification?.Name,
            account.ControlAccountType,
            account.IsSystemAccount,
            account.CreatedAt,
            account.UpdatedAt,
            children.Select(c => BuildHierarchy(c, allAccounts)).ToList()
        );
    }

    public async Task<bool> HasAccountsAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ChartOfAccounts.AnyAsync(c => c.TenantId == tenantId);
    }

    public async Task<bool> CodeExistsAsync(Guid tenantId, string code, Guid? excludeId = null)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(code)) return false;
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.ChartOfAccounts.Where(c => c.TenantId == tenantId && c.AccountCode == code);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId = null)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(name)) return false;
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.ChartOfAccounts.Where(c => c.TenantId == tenantId && c.AccountName == name);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
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

        if (tenantId == Guid.Empty)
            return result;

        await using var context = await _contextFactory.CreateDbContextAsync();
        var controlAccounts = await context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.ControlAccountType > 0)
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
                result[key] = new ControlAccountAssignmentDto(account.Id, account.AccountCode ?? "", account.AccountName);
            }
        }

        return result;
    }

    public async Task SetControlAccountAsync(Guid tenantId, Guid accountId, int controlAccountType)
    {
        if (tenantId == Guid.Empty)
            return;

        await using var context = await _contextFactory.CreateDbContextAsync();
        await context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.ControlAccountType == controlAccountType)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ControlAccountType, 0));

        await context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.Id == accountId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ControlAccountType, controlAccountType));
    }

    public async Task RemoveControlAccountAsync(Guid tenantId, int controlAccountType)
    {
        if (tenantId == Guid.Empty)
            return;

        await using var context = await _contextFactory.CreateDbContextAsync();
        await context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.ControlAccountType == controlAccountType)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.ControlAccountType, 0));
    }

    private static DtoAccountType MapAccountType(CoreAccountType type)
    {
        return (DtoAccountType)(int)type;
    }

    private static CoreAccountType MapToCoreAccountType(Truebooks.Platform.Contracts.DTOs.AccountType type)
    {
        return (CoreAccountType)(int)type;
    }
}
