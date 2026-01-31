using Microsoft.EntityFrameworkCore;
using Net4Courier.Web.Interfaces;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Finance.Entities;

namespace Net4Courier.Web.Services.GL;

public class GLChartOfAccountsService : IGLChartOfAccountsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public GLChartOfAccountsService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<GLChartOfAccountDto>> GetAllAsync(long? companyId = null)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.GLChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => !c.IsDeleted);

        if (companyId.HasValue)
            query = query.Where(c => c.CompanyId == companyId.Value);

        return await query
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .Select(c => new GLChartOfAccountDto(
                c.Id,
                c.CompanyId,
                c.AccountCode,
                c.AccountName,
                c.AccountType,
                c.ParentId,
                null,
                c.IsActive,
                c.AllowPosting,
                c.AccountClassificationId,
                c.AccountClassification != null ? c.AccountClassification.Name : null,
                c.ControlAccountType,
                c.IsSystemAccount,
                c.CreatedAt,
                c.ModifiedAt
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<GLChartOfAccountDto>> GetByTypeAsync(long? companyId, string? accountType)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.GLChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => !c.IsDeleted);

        if (companyId.HasValue)
            query = query.Where(c => c.CompanyId == companyId.Value);

        if (!string.IsNullOrEmpty(accountType))
            query = query.Where(c => c.AccountType == accountType);

        return await query
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .Select(c => new GLChartOfAccountDto(
                c.Id,
                c.CompanyId,
                c.AccountCode,
                c.AccountName,
                c.AccountType,
                c.ParentId,
                null,
                c.IsActive,
                c.AllowPosting,
                c.AccountClassificationId,
                c.AccountClassification != null ? c.AccountClassification.Name : null,
                c.ControlAccountType,
                c.IsSystemAccount,
                c.CreatedAt,
                c.ModifiedAt
            ))
            .ToListAsync();
    }

    public async Task<IEnumerable<GLChartOfAccountDto>> GetPostableAccountsAsync(long? companyId = null)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.GLChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => !c.IsDeleted && c.AllowPosting && c.IsActive);

        if (companyId.HasValue)
            query = query.Where(c => c.CompanyId == companyId.Value);

        return await query
            .OrderBy(c => c.AccountCode)
            .ThenBy(c => c.AccountName)
            .Select(c => new GLChartOfAccountDto(
                c.Id,
                c.CompanyId,
                c.AccountCode,
                c.AccountName,
                c.AccountType,
                c.ParentId,
                null,
                c.IsActive,
                c.AllowPosting,
                c.AccountClassificationId,
                c.AccountClassification != null ? c.AccountClassification.Name : null,
                c.ControlAccountType,
                c.IsSystemAccount,
                c.CreatedAt,
                c.ModifiedAt
            ))
            .ToListAsync();
    }

    public async Task<GLChartOfAccountDto?> GetByIdAsync(long id)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var entity = await context.GLChartOfAccounts
            .Include(c => c.AccountClassification)
            .Where(c => c.Id == id && !c.IsDeleted)
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

        return new GLChartOfAccountDto(
            entity.Id,
            entity.CompanyId,
            entity.AccountCode,
            entity.AccountName,
            entity.AccountType,
            entity.ParentId,
            parentName,
            entity.IsActive,
            entity.AllowPosting,
            entity.AccountClassificationId,
            entity.AccountClassification?.Name,
            entity.ControlAccountType,
            entity.IsSystemAccount,
            entity.CreatedAt,
            entity.ModifiedAt
        );
    }

    public async Task<GLChartOfAccountDto> CreateAsync(CreateGLChartOfAccountRequest request)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var entity = new GLChartOfAccount
        {
            CompanyId = request.CompanyId,
            AccountCode = request.AccountCode,
            AccountName = request.AccountName,
            AccountType = request.AccountType,
            ParentId = request.ParentId,
            AllowPosting = request.AllowPosting,
            AccountClassificationId = request.AccountClassificationId,
            ControlAccountType = request.ControlAccountType,
            IsActive = request.IsActive,
            IsSystemAccount = false,
            CreatedAt = DateTime.UtcNow
        };

        context.GLChartOfAccounts.Add(entity);
        await context.SaveChangesAsync();

        return await GetByIdAsync(entity.Id) ?? throw new InvalidOperationException("Failed to create account");
    }

    public async Task<GLChartOfAccountDto> UpdateAsync(long id, UpdateGLChartOfAccountRequest request)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var entity = await context.GLChartOfAccounts
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (entity == null)
            throw new InvalidOperationException("Account not found");

        if (entity.IsSystemAccount)
            throw new InvalidOperationException("Cannot modify system accounts");

        entity.AccountCode = request.AccountCode;
        entity.AccountName = request.AccountName;
        entity.AccountType = request.AccountType;
        entity.ParentId = request.ParentId;
        entity.AllowPosting = request.AllowPosting;
        entity.AccountClassificationId = request.AccountClassificationId;
        entity.ControlAccountType = request.ControlAccountType;
        entity.IsActive = request.IsActive;
        entity.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Failed to update account");
    }

    public async Task DeleteAsync(long id)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var entity = await context.GLChartOfAccounts
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (entity == null)
            return;

        if (entity.IsSystemAccount)
            throw new InvalidOperationException("Cannot delete system accounts");

        if (await HasTransactionsAsync(id))
            throw new InvalidOperationException("Cannot delete account with existing transactions");

        entity.IsDeleted = true;
        entity.IsActive = false;
        entity.ModifiedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
    }

    public async Task<bool> HasTransactionsAsync(long accountId)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        return await context.JournalEntries.AnyAsync(j => j.AccountHeadId == accountId);
    }

    public async Task<bool> HasAccountsAsync(long? companyId = null)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.GLChartOfAccounts.Where(c => !c.IsDeleted);
        if (companyId.HasValue)
            query = query.Where(c => c.CompanyId == companyId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> CodeExistsAsync(string code, long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        await using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.GLChartOfAccounts.Where(c => c.AccountCode == code && !c.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> NameExistsAsync(string name, long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        await using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.GLChartOfAccounts.Where(c => c.AccountName == name && !c.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<GLAccountClassificationDto>> GetClassificationsAsync(long? companyId = null)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var query = context.GLAccountClassifications.Where(c => !c.IsDeleted && c.IsActive);
        if (companyId.HasValue)
            query = query.Where(c => c.CompanyId == companyId.Value || c.CompanyId == null);
            
        return await query
            .OrderBy(c => c.Name)
            .Select(c => new GLAccountClassificationDto(
                c.Id,
                c.CompanyId,
                c.Name,
                c.Description,
                c.IsActive
            ))
            .ToListAsync();
    }
}
