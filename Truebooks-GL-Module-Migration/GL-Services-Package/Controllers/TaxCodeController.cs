using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Core.Infrastructure;

namespace Truebooks.Platform.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaxCodeController : ControllerBase
{
    private readonly PlatformDbContext _db;

    public TaxCodeController(PlatformDbContext db)
    {
        _db = db;
    }

    private Guid GetTenantId()
    {
        var tenantHeader = Request.Headers["X-Tenant-Id"].FirstOrDefault();
        return Guid.TryParse(tenantHeader, out var tenantId) ? tenantId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<List<TaxCodeDto>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var tenantId = GetTenantId();
        var query = _db.TaxCodes.Where(t => t.TenantId == tenantId);
        
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        var taxCodes = await query
            .OrderBy(t => t.Code)
            .Select(t => new TaxCodeDto
            {
                Id = t.Id,
                Code = t.Code,
                Description = t.Description,
                Rate = t.Rate,
                TaxType = t.TaxType,
                TaxTypeDisplay = t.TaxType.ToString(),
                IsActive = t.IsActive
            })
            .ToListAsync();

        return Ok(taxCodes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaxCodeDto>> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var taxCode = await _db.TaxCodes.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        
        if (taxCode == null)
            return NotFound();

        return Ok(new TaxCodeDto
        {
            Id = taxCode.Id,
            Code = taxCode.Code,
            Description = taxCode.Description,
            Rate = taxCode.Rate,
            TaxType = taxCode.TaxType,
            TaxTypeDisplay = taxCode.TaxType.ToString(),
            IsActive = taxCode.IsActive
        });
    }

    [HttpPost]
    public async Task<ActionResult<TaxCodeDto>> Create([FromBody] CreateTaxCodeRequest request)
    {
        var tenantId = GetTenantId();
        
        if (await _db.TaxCodes.AnyAsync(t => t.TenantId == tenantId && t.Code == request.Code))
            return BadRequest("Tax code already exists");

        var taxCode = new TaxCode
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Description = request.Description,
            Rate = request.Rate,
            TaxType = request.TaxType,
            IsActive = true
        };

        _db.TaxCodes.Add(taxCode);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = taxCode.Id }, new TaxCodeDto
        {
            Id = taxCode.Id,
            Code = taxCode.Code,
            Description = taxCode.Description,
            Rate = taxCode.Rate,
            TaxType = taxCode.TaxType,
            TaxTypeDisplay = taxCode.TaxType.ToString(),
            IsActive = taxCode.IsActive
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxCodeRequest request)
    {
        var tenantId = GetTenantId();
        var taxCode = await _db.TaxCodes.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        
        if (taxCode == null)
            return NotFound();

        if (await _db.TaxCodes.AnyAsync(t => t.TenantId == tenantId && t.Code == request.Code && t.Id != id))
            return BadRequest("Tax code already exists");

        taxCode.Code = request.Code;
        taxCode.Description = request.Description;
        taxCode.Rate = request.Rate;
        taxCode.TaxType = request.TaxType;
        taxCode.IsActive = request.IsActive;
        taxCode.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var taxCode = await _db.TaxCodes.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        
        if (taxCode == null)
            return NotFound();

        taxCode.IsActive = false;
        taxCode.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var tenantId = GetTenantId();
        var taxCode = await _db.TaxCodes.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        
        if (taxCode == null)
            return NotFound();

        taxCode.IsActive = true;
        taxCode.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class TaxCodeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string? Description { get; set; }
    public decimal Rate { get; set; }
    public TaxType TaxType { get; set; }
    public string TaxTypeDisplay { get; set; } = "";
    public bool IsActive { get; set; }
}

public class CreateTaxCodeRequest
{
    public string Code { get; set; } = "";
    public string? Description { get; set; }
    public decimal Rate { get; set; }
    public TaxType TaxType { get; set; } = TaxType.Simple;
}

public class UpdateTaxCodeRequest
{
    public string Code { get; set; } = "";
    public string? Description { get; set; }
    public decimal Rate { get; set; }
    public TaxType TaxType { get; set; }
    public bool IsActive { get; set; }
}
