using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Core.Infrastructure;
using Truebooks.Platform.Core.MultiTenancy;
using IndustryType = Truebooks.Platform.Contracts.Enums.IndustryType;

namespace Truebooks.Platform.Host.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChartOfAccountController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ChartOfAccountController(PlatformDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context not available");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChartOfAccountDto>>> GetAll()
    {
        var tenantId = GetCurrentTenantId();
        var accounts = await _context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .Include(c => c.AccountClassification)
            .OrderBy(c => c.AccountCode)
            .AsNoTracking()
            .Select(c => MapToDto(c))
            .ToListAsync();
        return Ok(accounts);
    }

    [HttpGet("hierarchy")]
    public async Task<ActionResult<IEnumerable<ChartOfAccountDto>>> GetHierarchy()
    {
        var tenantId = GetCurrentTenantId();
        var allAccounts = await _context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .Include(c => c.AccountClassification)
            .OrderBy(c => c.AccountCode)
            .AsNoTracking()
            .ToListAsync();

        var dtoDict = allAccounts.ToDictionary(a => a.Id, a => MapToDto(a));
        
        foreach (var account in allAccounts)
        {
            if (account.ParentAccountId.HasValue && dtoDict.ContainsKey(account.ParentAccountId.Value))
            {
                var parent = dtoDict[account.ParentAccountId.Value];
                parent.SubAccounts.Add(dtoDict[account.Id]);
            }
        }

        var rootAccounts = allAccounts
            .Where(c => c.ParentAccountId == null)
            .Select(c => dtoDict[c.Id])
            .ToList();
        return Ok(rootAccounts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChartOfAccountDto>> GetById(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var account = await _context.ChartOfAccounts
            .Include(c => c.AccountClassification)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        
        if (account == null)
            return NotFound();

        return Ok(MapToDto(account));
    }

    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<ChartOfAccountDto>>> FilterAccounts(
        [FromQuery] string? accountType = null,
        [FromQuery] Guid? classificationId = null,
        [FromQuery] bool onlyAllowPosting = true)
    {
        var tenantId = GetCurrentTenantId();
        
        var query = _context.ChartOfAccounts
            .Include(c => c.AccountClassification)
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.IsActive);

        if (onlyAllowPosting)
            query = query.Where(a => a.AllowPosting);

        if (!string.IsNullOrEmpty(accountType) && Enum.TryParse<AccountType>(accountType, true, out var parsedAccountType))
            query = query.Where(a => a.AccountType == parsedAccountType);

        if (classificationId.HasValue)
            query = query.Where(a => a.AccountClassificationId == classificationId.Value);

        var accounts = await query
            .OrderBy(a => a.AccountCode)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("validate/code-exists")]
    public async Task<ActionResult<bool>> CodeExists([FromQuery] string code, [FromQuery] Guid? excludeId = null)
    {
        var tenantId = GetCurrentTenantId();
        var exists = await _context.ChartOfAccounts
            .AnyAsync(c => c.TenantId == tenantId && c.AccountCode == code && c.Id != (excludeId ?? Guid.Empty));
        return Ok(exists);
    }

    [HttpGet("validate/name-exists")]
    public async Task<ActionResult<bool>> NameExists([FromQuery] string name, [FromQuery] Guid? excludeId = null)
    {
        var tenantId = GetCurrentTenantId();
        var exists = await _context.ChartOfAccounts
            .AnyAsync(c => c.TenantId == tenantId && c.AccountName.ToLower() == name.ToLower() && c.Id != (excludeId ?? Guid.Empty));
        return Ok(exists);
    }

    [HttpGet("{id}/has-children")]
    public async Task<ActionResult<bool>> HasChildren(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var hasChildren = await _context.ChartOfAccounts
            .AnyAsync(c => c.TenantId == tenantId && c.ParentAccountId == id && c.IsActive);
        return Ok(hasChildren);
    }

    [HttpGet("has-accounts")]
    public async Task<ActionResult<bool>> HasAccounts()
    {
        var tenantId = GetCurrentTenantId();
        var hasAccounts = await _context.ChartOfAccounts
            .AnyAsync(c => c.TenantId == tenantId && c.IsActive);
        return Ok(hasAccounts);
    }

    [HttpPost]
    public async Task<ActionResult<ChartOfAccountDto>> Create([FromBody] CreateChartOfAccountRequest request)
    {
        var tenantId = GetCurrentTenantId();
        
        var account = new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountCode = request.AccountCode,
            AccountName = request.AccountName,
            AccountType = Enum.Parse<AccountType>(request.AccountType, true),
            ParentAccountId = request.ParentAccountId,
            AllowPosting = request.AllowPosting,
            AccountClassificationId = request.AccountClassificationId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChartOfAccounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, MapToDto(account));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChartOfAccountRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var account = await _context.ChartOfAccounts
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

        if (account == null)
            return NotFound();

        account.AccountCode = request.AccountCode;
        account.AccountName = request.AccountName;
        account.AccountType = Enum.Parse<AccountType>(request.AccountType, true);
        account.ParentAccountId = request.ParentAccountId;
        account.AllowPosting = request.AllowPosting;
        account.AccountClassificationId = request.AccountClassificationId;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var account = await _context.ChartOfAccounts
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

        if (account == null)
            return NotFound();

        account.IsActive = false;
        account.DeactivatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("delete-all")]
    public async Task<IActionResult> DeleteAll()
    {
        var tenantId = GetCurrentTenantId();
        
        var accounts = await _context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .ToListAsync();
        
        if (!accounts.Any())
        {
            return Ok(new { message = "No active accounts to delete", count = 0 });
        }
        
        var now = DateTime.UtcNow;
        foreach (var account in accounts)
        {
            account.IsActive = false;
            account.DeactivatedDate = now;
        }
        
        await _context.SaveChangesAsync();
        return Ok(new { message = $"Deleted {accounts.Count} accounts", count = accounts.Count });
    }

    [HttpPost("reset-from-template")]
    public async Task<IActionResult> ResetFromTemplate([FromBody] SeedFromTemplateRequest request)
    {
        var tenantId = GetCurrentTenantId();
        
        // Hard delete ALL existing accounts (active and inactive) to keep database clean
        var existingAccounts = await _context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();
        
        // Clear parent references first to avoid FK constraint violation
        foreach (var account in existingAccounts)
        {
            account.ParentAccountId = null;
        }
        await _context.SaveChangesAsync();
        
        _context.ChartOfAccounts.RemoveRange(existingAccounts);
        await _context.SaveChangesAsync();
        
        // Now seed the new template
        var (accounts, parentMappings) = GetTemplateAccountsWithMappings(request.IndustryType, tenantId);
        
        // First pass: Insert all accounts without parent references
        _context.ChartOfAccounts.AddRange(accounts);
        await _context.SaveChangesAsync();
        
        // Second pass: Update parent references
        foreach (var account in accounts)
        {
            if (parentMappings.TryGetValue(account.AccountCode!, out var parentCode))
            {
                var parent = accounts.FirstOrDefault(a => a.AccountCode == parentCode);
                if (parent != null)
                {
                    account.ParentAccountId = parent.Id;
                }
            }
        }
        await _context.SaveChangesAsync();
        
        return Ok(new { 
            message = $"Successfully deleted {existingAccounts.Count} existing accounts and seeded {accounts.Count} new accounts from {request.IndustryType} template",
            deletedCount = existingAccounts.Count,
            seededCount = accounts.Count
        });
    }

    [HttpPost("seed-from-template")]
    public async Task<IActionResult> SeedFromTemplate([FromBody] SeedFromTemplateRequest request)
    {
        var tenantId = GetCurrentTenantId();
        
        var hasActiveAccounts = await _context.ChartOfAccounts
            .AnyAsync(c => c.TenantId == tenantId && c.IsActive);
        
        if (hasActiveAccounts)
        {
            return BadRequest("Cannot seed template when accounts already exist. Please delete existing accounts first.");
        }

        // Hard delete any previously soft-deleted accounts to avoid unique constraint violations
        var inactiveAccounts = await _context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && !c.IsActive)
            .ToListAsync();
        
        if (inactiveAccounts.Any())
        {
            // Clear parent references first to avoid FK constraint violation
            foreach (var account in inactiveAccounts)
            {
                account.ParentAccountId = null;
            }
            await _context.SaveChangesAsync();
            
            _context.ChartOfAccounts.RemoveRange(inactiveAccounts);
            await _context.SaveChangesAsync();
        }

        var (accounts, parentMappings) = GetTemplateAccountsWithMappings(request.IndustryType, tenantId);
        
        // First pass: Insert all accounts without parent references
        _context.ChartOfAccounts.AddRange(accounts);
        await _context.SaveChangesAsync();
        
        // Second pass: Update parent references
        foreach (var account in accounts)
        {
            if (parentMappings.TryGetValue(account.AccountCode!, out var parentCode))
            {
                var parent = accounts.FirstOrDefault(a => a.AccountCode == parentCode);
                if (parent != null)
                {
                    account.ParentAccountId = parent.Id;
                }
            }
        }
        await _context.SaveChangesAsync();
        
        return Ok(new { message = $"Successfully seeded {accounts.Count} accounts from {request.IndustryType} template" });
    }

    private (List<ChartOfAccount> accounts, Dictionary<string, string> parentMappings) GetTemplateAccountsWithMappings(IndustryType industryType, Guid tenantId)
    {
        var accounts = new List<ChartOfAccount>();
        var parentMappings = new Dictionary<string, string>();
        var now = DateTime.UtcNow;

        var templateAccounts = industryType == IndustryType.SoftwareCompany 
            ? GetSoftwareCompanyAccounts() 
            : GetCommonAccounts();

        foreach (var (code, name, type, allowPosting, parentCode) in templateAccounts)
        {
            accounts.Add(new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AccountCode = code,
                AccountName = name,
                AccountType = type,
                ParentAccountId = null,
                AllowPosting = allowPosting,
                IsActive = true,
                CreatedAt = now
            });

            if (parentCode != null)
            {
                parentMappings[code] = parentCode;
            }
        }

        return (accounts, parentMappings);
    }

    private static List<(string code, string name, AccountType type, bool allowPosting, string? parentCode)> GetSoftwareCompanyAccounts()
    {
        return new List<(string code, string name, AccountType type, bool allowPosting, string? parentCode)>
        {
            // ============ ASSETS ============
            ("1000", "Assets", AccountType.Asset, false, null),
            
            // Current Assets
            ("1100", "Current Assets", AccountType.Asset, false, "1000"),
            ("1101", "Cash in Hand", AccountType.Asset, true, "1100"),
            ("1102", "Bank - Current Account", AccountType.Asset, true, "1100"),
            ("1103", "Bank - Foreign Currency Account", AccountType.Asset, true, "1100"),
            ("1110", "Accounts Receivable (Trade Debtors)", AccountType.Asset, true, "1100"),
            ("1111", "Subscription Receivable", AccountType.Asset, true, "1100"),
            ("1112", "Unbilled Revenue", AccountType.Asset, true, "1100"),
            ("1120", "Prepaid Expenses", AccountType.Asset, true, "1100"),
            ("1121", "Advance to Vendors", AccountType.Asset, true, "1100"),
            ("1130", "Input GST / Input VAT", AccountType.Asset, true, "1100"),
            
            // Non-Current Assets
            ("1200", "Non-Current Assets", AccountType.Asset, false, "1000"),
            ("1201", "Computer Hardware", AccountType.Asset, true, "1200"),
            ("1202", "Servers & Networking Equipment", AccountType.Asset, true, "1200"),
            ("1203", "Office Furniture & Fixtures", AccountType.Asset, true, "1200"),
            ("1210", "Capitalized Software Development Cost", AccountType.Asset, true, "1200"),
            ("1211", "Website & Platform Development Cost", AccountType.Asset, true, "1200"),
            ("1220", "Security Deposits", AccountType.Asset, true, "1200"),
            ("1230", "Intangible Assets - Licenses / IP", AccountType.Asset, true, "1200"),
            
            // ============ LIABILITIES ============
            ("2000", "Liabilities", AccountType.Liability, false, null),
            
            // Current Liabilities
            ("2100", "Current Liabilities", AccountType.Liability, false, "2000"),
            ("2101", "Accounts Payable (Trade Creditors)", AccountType.Liability, true, "2100"),
            ("2102", "Accrued Expenses", AccountType.Liability, true, "2100"),
            ("2103", "Salaries & Wages Payable", AccountType.Liability, true, "2100"),
            ("2110", "Statutory Payables - GST / VAT", AccountType.Liability, true, "2100"),
            ("2111", "TDS / Withholding Tax Payable", AccountType.Liability, true, "2100"),
            ("2120", "Subscription Revenue Received in Advance", AccountType.Liability, true, "2100"),
            ("2121", "Unearned AMC Income", AccountType.Liability, true, "2100"),
            
            // Long-Term Liabilities
            ("2200", "Long-Term Liabilities", AccountType.Liability, false, "2000"),
            ("2201", "Term Loans", AccountType.Liability, true, "2200"),
            ("2202", "Working Capital Loan", AccountType.Liability, true, "2200"),
            ("2203", "Lease Liabilities", AccountType.Liability, true, "2200"),
            
            // ============ EQUITY ============
            ("3000", "Equity", AccountType.Equity, false, null),
            ("3100", "Capital Account / Share Capital", AccountType.Equity, true, "3000"),
            ("3101", "Partner's Capital (if LLP)", AccountType.Equity, true, "3000"),
            ("3200", "Retained Earnings", AccountType.Equity, true, "3000"),
            ("3300", "Current Year Profit & Loss", AccountType.Equity, true, "3000"),
            ("3500", "Opening Balance Equity", AccountType.Equity, true, "3000"),
            
            // ============ REVENUE (Operating Income) ============
            ("4000", "Revenue", AccountType.Revenue, false, null),
            
            // SaaS & Software Revenue
            ("4100", "SaaS & Software Revenue", AccountType.Revenue, false, "4000"),
            ("4101", "SaaS Subscription Income", AccountType.Revenue, true, "4100"),
            ("4102", "Annual License Fee Income", AccountType.Revenue, true, "4100"),
            ("4103", "One-Time Software License Sales", AccountType.Revenue, true, "4100"),
            ("4104", "Module Upgrade Charges", AccountType.Revenue, true, "4100"),
            ("4105", "AMC / Support & Maintenance Income", AccountType.Revenue, true, "4100"),
            
            // Services Revenue
            ("4200", "Services Revenue", AccountType.Revenue, false, "4000"),
            ("4201", "Custom Software Development Income", AccountType.Revenue, true, "4200"),
            ("4202", "Implementation & Deployment Charges", AccountType.Revenue, true, "4200"),
            ("4203", "Training & Onboarding Income", AccountType.Revenue, true, "4200"),
            ("4204", "Consulting / Professional Services Income", AccountType.Revenue, true, "4200"),
            
            // Marketing Revenue
            ("4300", "Marketing Revenue", AccountType.Revenue, false, "4000"),
            ("4301", "Digital Marketing Services Income", AccountType.Revenue, true, "4300"),
            ("4302", "Campaign Management Fees", AccountType.Revenue, true, "4300"),
            ("4303", "Lead Generation Services Income", AccountType.Revenue, true, "4300"),
            
            // ============ OTHER INCOME (Non-Operating) ============
            ("4500", "Other Income", AccountType.Revenue, false, "4000"),
            ("4501", "Interest Income", AccountType.Revenue, true, "4500"),
            ("4502", "Forex Gain", AccountType.Revenue, true, "4500"),
            ("4503", "Miscellaneous Income", AccountType.Revenue, true, "4500"),
            
            // ============ COST OF SALES / DIRECT COSTS ============
            ("5000", "Cost of Sales / Direct Costs", AccountType.Expense, false, null),
            
            // Technology & Delivery Costs
            ("5100", "Technology & Delivery Costs", AccountType.Expense, false, "5000"),
            ("5101", "Developer Salaries (Direct)", AccountType.Expense, true, "5100"),
            ("5102", "Cloud Hosting Charges (AWS / Azure / GCP)", AccountType.Expense, true, "5100"),
            ("5103", "Server & API Usage Charges", AccountType.Expense, true, "5100"),
            ("5104", "Software Licenses (Development Tools)", AccountType.Expense, true, "5100"),
            ("5105", "Payment Gateway Charges", AccountType.Expense, true, "5100"),
            
            // Support Costs
            ("5200", "Support Costs", AccountType.Expense, false, "5000"),
            ("5201", "Customer Support Salaries", AccountType.Expense, true, "5200"),
            ("5202", "AMC Service Costs", AccountType.Expense, true, "5200"),
            
            // ============ OPERATING EXPENSES ============
            ("6000", "Operating Expenses", AccountType.Expense, false, null),
            
            // Employee & Admin Expenses
            ("6100", "Employee & Admin Expenses", AccountType.Expense, false, "6000"),
            ("6101", "Salaries & Wages", AccountType.Expense, true, "6100"),
            ("6102", "Recruitment Expenses", AccountType.Expense, true, "6100"),
            ("6103", "Staff Welfare Expenses", AccountType.Expense, true, "6100"),
            ("6104", "Training & Certification Costs", AccountType.Expense, true, "6100"),
            
            // Sales & Marketing Expenses
            ("6200", "Sales & Marketing Expenses", AccountType.Expense, false, "6000"),
            ("6201", "Digital Advertising (Google, Meta, LinkedIn)", AccountType.Expense, true, "6200"),
            ("6202", "SEO & Content Marketing Expenses", AccountType.Expense, true, "6200"),
            ("6203", "Sales Commission", AccountType.Expense, true, "6200"),
            ("6204", "CRM Subscription Charges", AccountType.Expense, true, "6200"),
            ("6205", "Promotional Expenses", AccountType.Expense, true, "6200"),
            
            // Office & General Expenses
            ("6300", "Office & General Expenses", AccountType.Expense, false, "6000"),
            ("6301", "Rent & Maintenance", AccountType.Expense, true, "6300"),
            ("6302", "Internet & Communication", AccountType.Expense, true, "6300"),
            ("6303", "Electricity & Utilities", AccountType.Expense, true, "6300"),
            ("6304", "Office Supplies", AccountType.Expense, true, "6300"),
            ("6305", "Travel & Conveyance", AccountType.Expense, true, "6300"),
            
            // Professional & Compliance
            ("6400", "Professional & Compliance", AccountType.Expense, false, "6000"),
            ("6401", "Audit Fees", AccountType.Expense, true, "6400"),
            ("6402", "Legal & Professional Charges", AccountType.Expense, true, "6400"),
            ("6403", "Accounting & ERP Subscription", AccountType.Expense, true, "6400"),
            ("6404", "ROC / MCA / Compliance Fees", AccountType.Expense, true, "6400"),
            
            // Depreciation & Amortization
            ("6500", "Depreciation & Amortization", AccountType.Expense, false, "6000"),
            ("6501", "Depreciation - Hardware", AccountType.Expense, true, "6500"),
            ("6502", "Amortization - Software Development Cost", AccountType.Expense, true, "6500"),
            
            // ============ TAX & DUTIES ============
            ("7000", "Tax & Duties", AccountType.Expense, false, null),
            ("7001", "Income Tax Expense", AccountType.Expense, true, "7000"),
            ("7002", "Deferred Tax", AccountType.Expense, true, "7000"),
            ("7003", "Penalties & Interest", AccountType.Expense, true, "7000")
        };
    }

    private static List<(string code, string name, AccountType type, bool allowPosting, string? parentCode)> GetCommonAccounts()
    {
        return new List<(string code, string name, AccountType type, bool allowPosting, string? parentCode)>
        {
            // Assets - Level 1
            ("1000", "Assets", AccountType.Asset, false, null),
            // Assets - Level 2
            ("1100", "Current Assets", AccountType.Asset, false, "1000"),
            ("1200", "Non-Current Assets", AccountType.Asset, false, "1000"),
            // Current Assets - Level 3
            ("1110", "Cash and Cash Equivalents", AccountType.Asset, false, "1100"),
            ("1120", "Bank Accounts", AccountType.Asset, false, "1100"),
            ("1130", "Accounts Receivable", AccountType.Asset, false, "1100"),
            ("1140", "Inventory", AccountType.Asset, false, "1100"),
            ("1150", "Prepaid Expenses", AccountType.Asset, false, "1100"),
            // Non-Current Assets - Level 3
            ("1210", "Property, Plant & Equipment", AccountType.Asset, false, "1200"),
            ("1220", "Accumulated Depreciation", AccountType.Asset, false, "1200"),
            // Cash - Level 4
            ("1111", "Cash on Hand", AccountType.Asset, true, "1110"),
            ("1112", "Petty Cash", AccountType.Asset, true, "1110"),
            // Bank - Level 4
            ("1121", "Operating Bank Account", AccountType.Asset, true, "1120"),
            ("1122", "Savings Account", AccountType.Asset, true, "1120"),
            // Receivables - Level 4
            ("1131", "Trade Receivables", AccountType.Asset, true, "1130"),
            ("1132", "Other Receivables", AccountType.Asset, true, "1130"),
            // Inventory - Level 4
            ("1141", "Merchandise Inventory", AccountType.Asset, true, "1140"),
            ("1142", "Raw Materials", AccountType.Asset, true, "1140"),
            // Prepaid - Level 4
            ("1151", "Prepaid Insurance", AccountType.Asset, true, "1150"),
            ("1152", "Prepaid Rent", AccountType.Asset, true, "1150"),
            // PPE - Level 4
            ("1211", "Land", AccountType.Asset, true, "1210"),
            ("1212", "Buildings", AccountType.Asset, true, "1210"),
            ("1213", "Furniture & Fixtures", AccountType.Asset, true, "1210"),
            ("1214", "Office Equipment", AccountType.Asset, true, "1210"),
            ("1215", "Vehicles", AccountType.Asset, true, "1210"),
            // Depreciation - Level 4
            ("1221", "Accum. Depr. - Buildings", AccountType.Asset, true, "1220"),
            ("1222", "Accum. Depr. - Equipment", AccountType.Asset, true, "1220"),
            ("1223", "Accum. Depr. - Vehicles", AccountType.Asset, true, "1220"),

            // Liabilities - Level 1
            ("2000", "Liabilities", AccountType.Liability, false, null),
            // Liabilities - Level 2
            ("2100", "Current Liabilities", AccountType.Liability, false, "2000"),
            ("2200", "Non-Current Liabilities", AccountType.Liability, false, "2000"),
            // Current Liabilities - Level 3
            ("2110", "Accounts Payable", AccountType.Liability, false, "2100"),
            ("2120", "Accrued Expenses", AccountType.Liability, false, "2100"),
            ("2130", "Taxes Payable", AccountType.Liability, false, "2100"),
            ("2140", "Short-term Loans", AccountType.Liability, true, "2100"),
            // Non-Current Liabilities - Level 3
            ("2210", "Long-term Loans", AccountType.Liability, true, "2200"),
            ("2220", "Mortgage Payable", AccountType.Liability, true, "2200"),
            // Payables - Level 4
            ("2111", "Trade Payables", AccountType.Liability, true, "2110"),
            ("2112", "Other Payables", AccountType.Liability, true, "2110"),
            // Accrued - Level 4
            ("2121", "Accrued Salaries", AccountType.Liability, true, "2120"),
            ("2122", "Accrued Taxes", AccountType.Liability, true, "2120"),
            // Taxes - Level 4
            ("2131", "Sales Tax Payable", AccountType.Liability, true, "2130"),
            ("2132", "Income Tax Payable", AccountType.Liability, true, "2130"),

            // Equity - Level 1
            ("3000", "Equity", AccountType.Equity, false, null),
            // Equity - Level 2
            ("3100", "Owner's Capital", AccountType.Equity, true, "3000"),
            ("3200", "Retained Earnings", AccountType.Equity, true, "3000"),
            ("3300", "Drawings", AccountType.Equity, true, "3000"),
            ("3400", "Current Year Earnings", AccountType.Equity, true, "3000"),
            ("3500", "Opening Balance Equity", AccountType.Equity, true, "3000"),

            // Revenue - Level 1
            ("4000", "Revenue", AccountType.Revenue, false, null),
            // Revenue - Level 2
            ("4100", "Sales Revenue", AccountType.Revenue, false, "4000"),
            ("4200", "Other Income", AccountType.Revenue, false, "4000"),
            // Sales - Level 3
            ("4110", "Product Sales", AccountType.Revenue, true, "4100"),
            ("4120", "Service Revenue", AccountType.Revenue, true, "4100"),
            // Other Income - Level 3
            ("4210", "Interest Income", AccountType.Revenue, true, "4200"),
            ("4220", "Discount Received", AccountType.Revenue, true, "4200"),

            // COGS - Level 1
            ("5000", "Cost of Goods Sold", AccountType.Expense, false, null),
            // COGS - Level 2
            ("5100", "Cost of Sales", AccountType.Expense, true, "5000"),
            ("5200", "Freight In", AccountType.Expense, true, "5000"),
            ("5300", "Purchase Discounts", AccountType.Expense, true, "5000"),

            // Operating Expenses - Level 1
            ("6000", "Operating Expenses", AccountType.Expense, false, null),
            // Operating Expenses - Level 2
            ("6100", "Salaries & Wages", AccountType.Expense, false, "6000"),
            ("6200", "Rent Expense", AccountType.Expense, true, "6000"),
            ("6300", "Utilities", AccountType.Expense, false, "6000"),
            ("6400", "Office Expenses", AccountType.Expense, false, "6000"),
            ("6500", "Professional Fees", AccountType.Expense, false, "6000"),
            ("6600", "Insurance Expense", AccountType.Expense, true, "6000"),
            ("6700", "Depreciation Expense", AccountType.Expense, true, "6000"),
            ("6800", "Travel & Entertainment", AccountType.Expense, false, "6000"),
            ("6900", "Miscellaneous Expense", AccountType.Expense, true, "6000"),
            // Salaries - Level 3
            ("6110", "Salaries Expense", AccountType.Expense, true, "6100"),
            ("6120", "Wages Expense", AccountType.Expense, true, "6100"),
            // Utilities - Level 3
            ("6310", "Electricity", AccountType.Expense, true, "6300"),
            ("6320", "Water", AccountType.Expense, true, "6300"),
            ("6330", "Internet & Phone", AccountType.Expense, true, "6300"),
            // Office - Level 3
            ("6410", "Office Supplies", AccountType.Expense, true, "6400"),
            ("6420", "Printing & Stationery", AccountType.Expense, true, "6400"),
            // Professional Fees - Level 3
            ("6510", "Legal Fees", AccountType.Expense, true, "6500"),
            ("6520", "Accounting Fees", AccountType.Expense, true, "6500"),
            // Travel - Level 3
            ("6810", "Travel Expense", AccountType.Expense, true, "6800"),
            ("6820", "Meals & Entertainment", AccountType.Expense, true, "6800"),

            // Other Expenses - Level 1
            ("7000", "Other Expenses", AccountType.Expense, false, null),
            // Other Expenses - Level 2
            ("7100", "Interest Expense", AccountType.Expense, true, "7000"),
            ("7200", "Bank Charges", AccountType.Expense, true, "7000"),
            ("7300", "Bad Debt Expense", AccountType.Expense, true, "7000")
        };
    }

    private static ChartOfAccountDto MapToDto(ChartOfAccount account)
    {
        return new ChartOfAccountDto
        {
            Id = account.Id,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            AccountType = account.AccountType.ToString(),
            ParentAccountId = account.ParentAccountId,
            IsActive = account.IsActive,
            AllowPosting = account.AllowPosting,
            AccountClassificationId = account.AccountClassificationId,
            AccountClassification = account.AccountClassification != null ? new ChartOfAccountClassificationDto
            {
                Id = account.AccountClassification.Id,
                Name = account.AccountClassification.Name,
                Description = account.AccountClassification.Description
            } : null
        };
    }
}

public class ChartOfAccountDto
{
    public Guid Id { get; set; }
    public string? AccountCode { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public Guid? ParentAccountId { get; set; }
    public bool IsActive { get; set; }
    public bool AllowPosting { get; set; }
    public Guid? AccountClassificationId { get; set; }
    public ChartOfAccountClassificationDto? AccountClassification { get; set; }
    public List<ChartOfAccountDto> SubAccounts { get; set; } = new();
}

public class ChartOfAccountClassificationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateChartOfAccountRequest
{
    public string? AccountCode { get; set; }
    public required string AccountName { get; set; }
    public required string AccountType { get; set; }
    public Guid? ParentAccountId { get; set; }
    public bool AllowPosting { get; set; } = true;
    public Guid? AccountClassificationId { get; set; }
}

public class UpdateChartOfAccountRequest
{
    public string? AccountCode { get; set; }
    public required string AccountName { get; set; }
    public required string AccountType { get; set; }
    public Guid? ParentAccountId { get; set; }
    public bool AllowPosting { get; set; } = true;
    public Guid? AccountClassificationId { get; set; }
}

public class SeedFromTemplateRequest
{
    public IndustryType IndustryType { get; set; }
}
