using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Shared.Enums;

namespace Server.Modules.GeneralLedger.Controllers;

public class SeedTemplateRequest
{
    public Shared.Enums.IndustryType IndustryType { get; set; }
}

public class ControlAccountAssignment
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public ControlAccountType ControlAccountType { get; set; }
    public bool IsSystemAccount { get; set; }
}

public class SetControlAccountRequest
{
    public Guid? AccountId { get; set; }
    public ControlAccountType ControlAccountType { get; set; }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChartOfAccountController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly PdfExportService _pdfExportService;
    private readonly ExcelExportService _excelExportService;

    public ChartOfAccountController(
        AppDbContext context, 
        ITenantProvider tenantProvider,
        PdfExportService pdfExportService,
        ExcelExportService excelExportService)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _pdfExportService = pdfExportService;
        _excelExportService = excelExportService;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.CurrentTenantId ?? throw new UnauthorizedAccessException("Tenant context not available");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChartOfAccount>>> GetAll()
    {
        var tenantId = GetCurrentTenantId();
        // Load without navigation properties to avoid circular references
        return await _context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.AccountCode)
            .AsNoTracking()
            .ToListAsync();
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ChartOfAccount>>> GetActive()
    {
        var tenantId = GetCurrentTenantId();
        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive && a.AllowPosting)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();
        return Ok(accounts);
    }

    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<ChartOfAccount>>> FilterAccounts(
        [FromQuery] string? accountType = null,
        [FromQuery] Guid? classificationId = null,
        [FromQuery] bool onlyAllowPosting = true)
    {
        var tenantId = GetCurrentTenantId();
        
        var query = _context.ChartOfAccounts
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.IsActive);

        if (onlyAllowPosting)
        {
            query = query.Where(a => a.AllowPosting);
        }

        if (!string.IsNullOrEmpty(accountType) && Enum.TryParse<AccountType>(accountType, true, out var parsedAccountType))
        {
            query = query.Where(a => a.AccountType == parsedAccountType);
        }

        if (classificationId.HasValue)
        {
            query = query.Where(a => a.AccountClassificationId == classificationId.Value);
        }

        var accounts = await query
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("hierarchy")]
    public async Task<ActionResult<IEnumerable<ChartOfAccount>>> GetHierarchy()
    {
        var tenantId = GetCurrentTenantId();
        
        // Load all accounts without navigation properties to avoid circular references
        var allAccounts = await _context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.AccountCode)
            .AsNoTracking()
            .ToListAsync();

        // Build hierarchy manually
        var accountDict = allAccounts.ToDictionary(a => a.Id);
        
        foreach (var account in allAccounts)
        {
            if (account.ParentAccountId.HasValue && accountDict.ContainsKey(account.ParentAccountId.Value))
            {
                var parent = accountDict[account.ParentAccountId.Value];
                parent.SubAccounts.Add(account);
            }
        }

        // Return only root accounts (those without parents)
        var rootAccounts = allAccounts.Where(c => c.ParentAccountId == null).ToList();
        return rootAccounts;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChartOfAccount>> GetById(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var account = await _context.ChartOfAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        
        if (account == null)
        {
            return NotFound();
        }

        return account;
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
        var hasChildren = await _context.ChartOfAccounts.AnyAsync(c => c.TenantId == tenantId && c.ParentAccountId == id);
        return Ok(hasChildren);
    }

    [HttpGet("{id}/is-descendant/{possibleParentId}")]
    public async Task<ActionResult<bool>> IsDescendant(Guid id, Guid possibleParentId)
    {
        var tenantId = GetCurrentTenantId();
        var isDescendant = await IsDescendantOf(id, possibleParentId, tenantId);
        return Ok(isDescendant);
    }

    [HttpPost]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<ChartOfAccount>> Create([FromBody] ChartOfAccount account)
    {
        var tenantId = GetCurrentTenantId();
        
        // CRITICAL: Force TenantId to authenticated tenant (prevent tenant-escape)
        account.TenantId = tenantId;
        
        // Check for duplicate code within tenant
        if (await _context.ChartOfAccounts.AnyAsync(c => c.TenantId == tenantId && c.AccountCode == account.AccountCode))
        {
            return BadRequest("Account code already exists");
        }
        
        // Check for duplicate name within tenant
        if (await _context.ChartOfAccounts.AnyAsync(c => c.TenantId == tenantId && c.AccountName.ToLower() == account.AccountName.ToLower()))
        {
            return BadRequest("Account name already exists");
        }
        
        if (account.ParentAccountId.HasValue)
        {
            // Verify parent exists in same tenant
            var parent = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.Id == account.ParentAccountId.Value && c.TenantId == tenantId);
            if (parent == null)
            {
                return BadRequest("Parent account not found");
            }
        }

        _context.ChartOfAccounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ChartOfAccount account)
    {
        if (id != account.Id)
        {
            return BadRequest();
        }

        var tenantId = GetCurrentTenantId();
        var existing = await _context.ChartOfAccounts
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (existing == null)
        {
            return NotFound();
        }

        // Check for duplicate code within tenant
        if (await _context.ChartOfAccounts.AnyAsync(c => c.TenantId == tenantId && c.AccountCode == account.AccountCode && c.Id != id))
        {
            return BadRequest("Account code already exists");
        }
        
        // Check for duplicate name within tenant
        if (await _context.ChartOfAccounts.AnyAsync(c => c.TenantId == tenantId && c.AccountName.ToLower() == account.AccountName.ToLower() && c.Id != id))
        {
            return BadRequest("Account name already exists");
        }

        // Prevent self-reference and cycles
        if (account.ParentAccountId.HasValue)
        {
            // Reject self-reference
            if (account.ParentAccountId.Value == id)
            {
                return BadRequest("Cannot set account as its own parent");
            }

            // Verify parent exists in same tenant
            var parent = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.Id == account.ParentAccountId.Value && c.TenantId == tenantId);
            if (parent == null)
            {
                return BadRequest("Parent account not found");
            }

            // Check if new parent is a descendant
            var isDescendant = await IsDescendantOf(id, account.ParentAccountId.Value, tenantId);
            if (isDescendant)
            {
                return BadRequest("Cannot set a descendant account as parent");
            }
        }

        existing.AccountCode = account.AccountCode;
        existing.AccountName = account.AccountName;
        existing.AccountType = account.AccountType;
        existing.ParentAccountId = account.ParentAccountId;
        existing.AllowPosting = account.AllowPosting;
        existing.IsActive = account.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("has-accounts")]
    public async Task<ActionResult<bool>> HasAccounts()
    {
        var tenantId = GetCurrentTenantId();
        var hasAccounts = await _context.ChartOfAccounts.AnyAsync(c => c.TenantId == tenantId);
        return Ok(hasAccounts);
    }

    [HttpPost("seed-from-template")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult> SeedFromTemplate([FromBody] SeedTemplateRequest request)
    {
        var tenantId = GetCurrentTenantId();
        
        // Check if accounts already exist
        var hasAccounts = await _context.ChartOfAccounts.AnyAsync(c => c.TenantId == tenantId);
        if (hasAccounts)
        {
            return BadRequest("Chart of accounts already has data. Template seeding is only allowed for empty accounts.");
        }

        // Get templates for the selected industry
        var templates = await _context.ChartOfAccountTemplates
            .Where(t => t.IndustryType == request.IndustryType)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

        if (!templates.Any())
        {
            return BadRequest("No templates found for the selected industry type.");
        }

        // Create a mapping from template code to created account ID
        var codeToIdMap = new Dictionary<string, Guid>();
        
        // Identify which account codes are parent accounts (have children)
        var parentCodes = templates
            .Where(t => !string.IsNullOrEmpty(t.ParentCode))
            .Select(t => t.ParentCode)
            .Distinct()
            .ToHashSet();

        // First pass: Create all accounts without parent references
        var accountsToCreate = new List<ChartOfAccount>();
        foreach (var template in templates)
        {
            var isParentAccount = !string.IsNullOrEmpty(template.AccountCode) && parentCodes.Contains(template.AccountCode);
            
            var account = new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AccountCode = template.AccountCode,
                AccountName = template.AccountName,
                AccountType = template.AccountType,
                IsActive = true,
                AllowPosting = !isParentAccount, // Headers (parents) cannot have postings
                ParentAccountId = null // Will be set in second pass
            };
            
            accountsToCreate.Add(account);
            
            if (!string.IsNullOrEmpty(template.AccountCode))
            {
                codeToIdMap[template.AccountCode] = account.Id;
            }
        }

        // Second pass: Set parent references
        for (int i = 0; i < templates.Count; i++)
        {
            if (!string.IsNullOrEmpty(templates[i].ParentCode) && codeToIdMap.ContainsKey(templates[i].ParentCode))
            {
                accountsToCreate[i].ParentAccountId = codeToIdMap[templates[i].ParentCode];
            }
        }

        // Add all accounts to database
        _context.ChartOfAccounts.AddRange(accountsToCreate);
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Successfully created {accountsToCreate.Count} accounts from {request.IndustryType} template." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "owner,admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var account = await _context.ChartOfAccounts
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        
        if (account == null)
        {
            return NotFound();
        }

        var hasSubAccounts = await _context.ChartOfAccounts.AnyAsync(c => c.TenantId == tenantId && c.ParentAccountId == id);
        if (hasSubAccounts)
        {
            return BadRequest("Cannot delete account with sub-accounts");
        }

        account.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("export-pdf")]
    public async Task<IActionResult> ExportPdf()
    {
        var tenantId = GetCurrentTenantId();
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Unknown";
        
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null)
        {
            return NotFound("Tenant not found");
        }

        var accounts = await _context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.AccountCode)
            .AsNoTracking()
            .ToListAsync();

        var reportId = $"COA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        
        var pdfBytes = _pdfExportService.GenerateChartOfAccountsPdf(
            accounts,
            tenant.Name,
            tenant.Address,
            userEmail,
            reportId
        );

        var fileName = $"Chart_of_Accounts_{tenant.Name.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportExcel()
    {
        var tenantId = GetCurrentTenantId();
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Unknown";
        
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null)
        {
            return NotFound("Tenant not found");
        }

        var accounts = await _context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.AccountCode)
            .AsNoTracking()
            .ToListAsync();

        var reportId = $"COA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        
        var excelBytes = _excelExportService.GenerateChartOfAccountsExcel(
            accounts,
            tenant.Name,
            tenant.Address,
            userEmail,
            reportId
        );

        var fileName = $"Chart_of_Accounts_{tenant.Name.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("control-accounts")]
    [Authorize(Roles = "owner,admin,accountant")]
    public async Task<ActionResult<Dictionary<string, ControlAccountAssignment>>> GetControlAccounts()
    {
        var tenantId = GetCurrentTenantId();
        
        var controlAccounts = await _context.ChartOfAccounts
            .Where(c => c.TenantId == tenantId && c.ControlAccountType != ControlAccountType.None)
            .Select(c => new ControlAccountAssignment
            {
                AccountId = c.Id,
                AccountCode = c.AccountCode,
                AccountName = c.AccountName,
                ControlAccountType = c.ControlAccountType,
                IsSystemAccount = c.IsSystemAccount
            })
            .ToListAsync();

        var result = new Dictionary<string, ControlAccountAssignment?>();
        foreach (ControlAccountType type in Enum.GetValues(typeof(ControlAccountType)))
        {
            if (type == ControlAccountType.None) continue;
            var assignment = controlAccounts.FirstOrDefault(c => c.ControlAccountType == type);
            result[type.ToString()] = assignment;
        }

        return Ok(result);
    }

    [HttpPost("control-accounts")]
    [Authorize(Roles = "owner,admin")]
    public async Task<IActionResult> SetControlAccount([FromBody] SetControlAccountRequest request)
    {
        var tenantId = GetCurrentTenantId();

        if (request.ControlAccountType == ControlAccountType.None)
        {
            return BadRequest("Cannot set control account type to None");
        }

        if (!request.AccountId.HasValue)
        {
            return BadRequest("Account ID is required");
        }

        var newControlAccount = await _context.ChartOfAccounts
            .FirstOrDefaultAsync(c => c.Id == request.AccountId.Value && c.TenantId == tenantId);
        if (newControlAccount == null)
        {
            return NotFound("Account not found");
        }

        var validAccountType = GetValidAccountTypeForControlAccount(request.ControlAccountType);
        if (validAccountType.HasValue && newControlAccount.AccountType != validAccountType.Value)
        {
            return BadRequest($"Account type must be '{validAccountType.Value}' for {request.ControlAccountType}");
        }

        if (newControlAccount.ControlAccountType != ControlAccountType.None && 
            newControlAccount.ControlAccountType != request.ControlAccountType)
        {
            return BadRequest($"Account is already assigned as {newControlAccount.ControlAccountType}");
        }

        var existingControlAccount = await _context.ChartOfAccounts
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && 
                                     c.ControlAccountType == request.ControlAccountType &&
                                     c.Id != request.AccountId.Value);
        if (existingControlAccount != null)
        {
            existingControlAccount.ControlAccountType = ControlAccountType.None;
            existingControlAccount.IsSystemAccount = false;
        }

        newControlAccount.ControlAccountType = request.ControlAccountType;
        newControlAccount.IsSystemAccount = true;

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("control-accounts/{controlAccountType}")]
    [Authorize(Roles = "owner,admin")]
    public async Task<IActionResult> RemoveControlAccount(ControlAccountType controlAccountType)
    {
        var tenantId = GetCurrentTenantId();

        if (controlAccountType == ControlAccountType.None)
        {
            return BadRequest("Invalid control account type");
        }

        var existingControlAccount = await _context.ChartOfAccounts
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && 
                                     c.ControlAccountType == controlAccountType);
        if (existingControlAccount != null)
        {
            existingControlAccount.ControlAccountType = ControlAccountType.None;
            existingControlAccount.IsSystemAccount = false;
            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    private static AccountType? GetValidAccountTypeForControlAccount(ControlAccountType controlType)
    {
        return controlType switch
        {
            ControlAccountType.AccountsReceivable => AccountType.Asset,
            ControlAccountType.AccountsPayable => AccountType.Liability,
            ControlAccountType.Inventory => AccountType.Asset,
            ControlAccountType.CostOfGoodsSold => AccountType.Expense,
            ControlAccountType.RetainedEarnings => AccountType.Equity,
            ControlAccountType.OpeningBalanceEquity => AccountType.Equity,
            _ => null
        };
    }

    private async Task<bool> IsDescendantOf(Guid accountId, Guid possibleParentId, Guid tenantId)
    {
        var currentParentId = possibleParentId;
        while (currentParentId != Guid.Empty)
        {
            if (currentParentId == accountId)
                return true;
                
            var parent = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.Id == currentParentId && c.TenantId == tenantId);
            if (parent?.ParentAccountId == null)
                break;
            currentParentId = parent.ParentAccountId.Value;
        }
        return false;
    }
}
