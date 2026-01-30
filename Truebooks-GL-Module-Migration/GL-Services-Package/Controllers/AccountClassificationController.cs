using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Core.Infrastructure;
using Truebooks.Platform.Core.MultiTenancy;

namespace Truebooks.Platform.Host.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountClassificationController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly ITenantContext _tenantContext;

    public AccountClassificationController(PlatformDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context not available");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountClassificationDto>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var tenantId = GetCurrentTenantId();
        var query = _context.AccountClassifications
            .Where(a => a.TenantId == tenantId);

        if (!includeInactive)
            query = query.Where(a => a.IsActive);

        var classifications = await query
            .OrderBy(a => a.Name)
            .AsNoTracking()
            .Select(a => MapToDto(a))
            .ToListAsync();

        return Ok(classifications);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<AccountClassificationDto>>> GetActive()
    {
        var tenantId = GetCurrentTenantId();
        var classifications = await _context.AccountClassifications
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .OrderBy(a => a.Name)
            .AsNoTracking()
            .Select(a => MapToDto(a))
            .ToListAsync();

        return Ok(classifications);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountClassificationDto>> GetById(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var classification = await _context.AccountClassifications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (classification == null)
            return NotFound();

        return Ok(MapToDto(classification));
    }

    [HttpPost]
    [Authorize(Roles = "owner,admin")]
    public async Task<ActionResult<AccountClassificationDto>> Create([FromBody] CreateAccountClassificationRequest request)
    {
        var tenantId = GetCurrentTenantId();

        var nameExists = await _context.AccountClassifications
            .AnyAsync(a => a.TenantId == tenantId && a.Name == request.Name);

        if (nameExists)
            return BadRequest("An account classification with this name already exists.");

        var classification = new AccountClassification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.AccountClassifications.Add(classification);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = classification.Id }, MapToDto(classification));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "owner,admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountClassificationRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var classification = await _context.AccountClassifications
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (classification == null)
            return NotFound();

        var nameExists = await _context.AccountClassifications
            .AnyAsync(a => a.TenantId == tenantId && a.Name == request.Name && a.Id != id);

        if (nameExists)
            return BadRequest("An account classification with this name already exists.");

        classification.Name = request.Name;
        classification.Description = request.Description;
        classification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "owner,admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var classification = await _context.AccountClassifications
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (classification == null)
            return NotFound();

        classification.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static AccountClassificationDto MapToDto(AccountClassification classification)
    {
        return new AccountClassificationDto
        {
            Id = classification.Id,
            Name = classification.Name,
            Description = classification.Description,
            IsActive = classification.IsActive
        };
    }
}

public class AccountClassificationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAccountClassificationRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class UpdateAccountClassificationRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
