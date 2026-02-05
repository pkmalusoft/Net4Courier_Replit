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
        var companyId = GuidToLong(tenantId);
        return await context.GLChartOfAccounts.AnyAsync(c => !c.IsDeleted && c.CompanyId == companyId);
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

    public async Task SeedFromTemplateAsync(Guid tenantId, string industryType)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();
        var companyId = GuidToLong(tenantId);
        
        // Check if any accounts already exist for this tenant
        if (await context.GLChartOfAccounts.AnyAsync(c => !c.IsDeleted && c.CompanyId == companyId))
        {
            throw new InvalidOperationException("Chart of accounts already has data. Cannot seed sample accounts.");
        }
        
        var now = DateTime.UtcNow;
        var accounts = GetCourierChartOfAccountsTemplate(now, companyId);
        
        // Add all accounts
        context.GLChartOfAccounts.AddRange(accounts);
        await context.SaveChangesAsync();
        
        // Update parent IDs based on account codes (scoped to this tenant)
        await UpdateParentIdsAsync(context, companyId);
    }
    
    private async Task UpdateParentIdsAsync(ApplicationDbContext context, long companyId)
    {
        var accounts = await context.GLChartOfAccounts.Where(c => !c.IsDeleted && c.CompanyId == companyId).ToListAsync();
        var codeToId = accounts.ToDictionary(a => a.AccountCode, a => a.Id);
        
        // Map child codes to parent codes
        var parentMapping = new Dictionary<string, string>
        {
            // Current Assets children
            { "1100", "1000" }, { "1110", "1100" }, { "1120", "1100" }, { "1130", "1100" }, { "1140", "1100" },
            { "1200", "1000" }, { "1300", "1000" }, { "1310", "1300" }, { "1320", "1300" },
            // Fixed Assets children
            { "1500", "1000" }, { "1510", "1500" }, { "1520", "1500" }, { "1530", "1500" }, { "1540", "1500" }, { "1550", "1500" },
            // Current Liabilities children
            { "2100", "2000" }, { "2110", "2100" }, { "2120", "2100" }, { "2130", "2100" }, { "2140", "2100" }, { "2150", "2100" }, { "2160", "2100" },
            // Equity children
            { "3100", "3000" }, { "3200", "3000" }, { "3300", "3000" },
            // Revenue children
            { "4100", "4000" }, { "4110", "4100" }, { "4120", "4100" }, { "4130", "4100" }, { "4140", "4100" }, { "4150", "4100" }, { "4160", "4100" },
            { "4200", "4000" }, { "4210", "4200" }, { "4220", "4200" }, { "4230", "4200" },
            // Direct Expenses children
            { "5100", "5000" }, { "5110", "5100" }, { "5120", "5100" }, { "5130", "5100" }, { "5140", "5100" }, { "5150", "5100" }, { "5160", "5100" },
            // Operating Expenses children
            { "5200", "5000" }, { "5210", "5200" }, { "5220", "5200" }, { "5230", "5200" }, { "5240", "5200" }, { "5250", "5200" }, { "5260", "5200" }, { "5270", "5200" }, { "5280", "5200" },
            // Administrative Expenses children
            { "5300", "5000" }, { "5310", "5300" }, { "5320", "5300" }, { "5330", "5300" }, { "5340", "5300" }, { "5350", "5300" }, { "5360", "5300" },
            // Financial Expenses children
            { "5400", "5000" }, { "5410", "5400" }, { "5420", "5400" }
        };
        
        foreach (var account in accounts)
        {
            if (parentMapping.TryGetValue(account.AccountCode, out var parentCode) && codeToId.TryGetValue(parentCode, out var parentId))
            {
                account.ParentId = parentId;
            }
        }
        
        await context.SaveChangesAsync();
    }
    
    private List<GLChartOfAccount> GetCourierChartOfAccountsTemplate(DateTime now, long companyId)
    {
        return new List<GLChartOfAccount>
        {
            // ASSETS (1000-1999)
            new() { CompanyId = companyId, AccountCode = "1000", AccountName = "Assets", AccountType = "Asset", Level = 0, AllowPosting = false, IsActive = true, CreatedAt = now },
            
            // Current Assets
            new() { CompanyId = companyId, AccountCode = "1100", AccountName = "Cash & Bank", AccountType = "Asset", Level = 1, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1110", AccountName = "Petty Cash", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1120", AccountName = "Cash in Hand", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1130", AccountName = "Bank Account - Operating", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1140", AccountName = "Bank Account - COD Collection", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            
            new() { CompanyId = companyId, AccountCode = "1200", AccountName = "Accounts Receivable", AccountType = "Asset", Level = 1, AllowPosting = true, IsActive = true, ControlAccountType = 1, CreatedAt = now },
            
            new() { CompanyId = companyId, AccountCode = "1300", AccountName = "Other Current Assets", AccountType = "Asset", Level = 1, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1310", AccountName = "Prepaid Expenses", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1320", AccountName = "Advances to Staff", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            
            // Fixed Assets
            new() { CompanyId = companyId, AccountCode = "1500", AccountName = "Fixed Assets", AccountType = "Asset", Level = 1, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1510", AccountName = "Vehicles", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1520", AccountName = "Accumulated Depreciation - Vehicles", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1530", AccountName = "Office Equipment", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1540", AccountName = "Accumulated Depreciation - Equipment", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "1550", AccountName = "Furniture & Fixtures", AccountType = "Asset", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            
            // LIABILITIES (2000-2999)
            new() { CompanyId = companyId, AccountCode = "2000", AccountName = "Liabilities", AccountType = "Liability", Level = 0, AllowPosting = false, IsActive = true, CreatedAt = now },
            
            new() { CompanyId = companyId, AccountCode = "2100", AccountName = "Current Liabilities", AccountType = "Liability", Level = 1, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "2110", AccountName = "Accounts Payable", AccountType = "Liability", Level = 2, AllowPosting = true, IsActive = true, ControlAccountType = 2, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "2120", AccountName = "COD Payable to Customers", AccountType = "Liability", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "2130", AccountName = "VAT Payable", AccountType = "Liability", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "2140", AccountName = "Accrued Expenses", AccountType = "Liability", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "2150", AccountName = "Salaries Payable", AccountType = "Liability", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "2160", AccountName = "Customs Duty Payable", AccountType = "Liability", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            
            // EQUITY (3000-3999)
            new() { CompanyId = companyId, AccountCode = "3000", AccountName = "Equity", AccountType = "Equity", Level = 0, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "3100", AccountName = "Owner's Capital", AccountType = "Equity", Level = 1, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "3200", AccountName = "Retained Earnings", AccountType = "Equity", Level = 1, AllowPosting = true, IsActive = true, ControlAccountType = 5, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "3300", AccountName = "Opening Balance Equity", AccountType = "Equity", Level = 1, AllowPosting = true, IsActive = true, ControlAccountType = 6, CreatedAt = now },
            
            // REVENUE (4000-4999)
            new() { CompanyId = companyId, AccountCode = "4000", AccountName = "Revenue", AccountType = "Revenue", Level = 0, AllowPosting = false, IsActive = true, CreatedAt = now },
            
            new() { CompanyId = companyId, AccountCode = "4100", AccountName = "Courier Service Revenue", AccountType = "Revenue", Level = 1, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "4110", AccountName = "Domestic Delivery Revenue", AccountType = "Revenue", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "4120", AccountName = "International Export Revenue", AccountType = "Revenue", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "4130", AccountName = "International Import Revenue", AccountType = "Revenue", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "4140", AccountName = "Express Delivery Revenue", AccountType = "Revenue", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "4150", AccountName = "COD Collection Fee", AccountType = "Revenue", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "4160", AccountName = "Fuel Surcharge Revenue", AccountType = "Revenue", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            
            new() { CompanyId = companyId, AccountCode = "4200", AccountName = "Other Revenue", AccountType = "Revenue", Level = 1, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "4210", AccountName = "Packaging Revenue", AccountType = "Revenue", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "4220", AccountName = "Insurance Revenue", AccountType = "Revenue", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "4230", AccountName = "Miscellaneous Revenue", AccountType = "Revenue", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            
            // EXPENSES (5000-5999)
            new() { CompanyId = companyId, AccountCode = "5000", AccountName = "Expenses", AccountType = "Expense", Level = 0, AllowPosting = false, IsActive = true, CreatedAt = now },
            
            // Direct Expenses
            new() { CompanyId = companyId, AccountCode = "5100", AccountName = "Direct Operating Costs", AccountType = "Expense", Level = 1, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5110", AccountName = "Fuel Expenses", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5120", AccountName = "Vehicle Maintenance", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5130", AccountName = "Courier Partner Costs", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5140", AccountName = "Packaging Supplies", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5150", AccountName = "Customs & Duty Expenses", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5160", AccountName = "Delivery Staff Salaries", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            
            // Operating Expenses
            new() { CompanyId = companyId, AccountCode = "5200", AccountName = "Operating Expenses", AccountType = "Expense", Level = 1, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5210", AccountName = "Rent Expense", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5220", AccountName = "Utilities", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5230", AccountName = "Telephone & Internet", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5240", AccountName = "Insurance Expense", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5250", AccountName = "Vehicle Insurance", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5260", AccountName = "Depreciation Expense", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5270", AccountName = "Warehouse Expenses", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5280", AccountName = "Equipment Rental", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            
            // Administrative Expenses
            new() { CompanyId = companyId, AccountCode = "5300", AccountName = "Administrative Expenses", AccountType = "Expense", Level = 1, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5310", AccountName = "Office Salaries", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5320", AccountName = "Office Supplies", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5330", AccountName = "Professional Fees", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5340", AccountName = "Software & Subscriptions", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5350", AccountName = "Marketing & Advertising", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5360", AccountName = "Travel & Entertainment", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            
            // Financial Expenses
            new() { CompanyId = companyId, AccountCode = "5400", AccountName = "Financial Expenses", AccountType = "Expense", Level = 1, AllowPosting = false, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5410", AccountName = "Bank Charges", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now },
            new() { CompanyId = companyId, AccountCode = "5420", AccountName = "Interest Expense", AccountType = "Expense", Level = 2, AllowPosting = true, IsActive = true, CreatedAt = now }
        };
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
