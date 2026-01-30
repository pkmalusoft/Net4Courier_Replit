using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Truebooks.Platform.Core.Infrastructure;

namespace Truebooks.Platform.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VoucherNumberingController : ControllerBase
{
    private readonly PlatformDbContext _db;

    public VoucherNumberingController(PlatformDbContext db)
    {
        _db = db;
    }

    private Guid GetTenantId()
    {
        var tenantHeader = Request.Headers["X-Tenant-Id"].FirstOrDefault();
        return Guid.TryParse(tenantHeader, out var tenantId) ? tenantId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<List<VoucherNumberingDto>>> GetAll()
    {
        var tenantId = GetTenantId();
        var configs = await _db.VoucherNumberings
            .Where(v => v.TenantId == tenantId)
            .OrderBy(v => v.TransactionType)
            .Select(v => new VoucherNumberingDto
            {
                Id = v.Id,
                TransactionType = v.TransactionType,
                TransactionTypeName = v.TransactionType.ToString(),
                Prefix = v.Prefix,
                NextNumber = v.NextNumber,
                NumberLength = v.NumberLength,
                Separator = v.Separator,
                IsLocked = v.IsLocked,
                IsActive = v.IsActive,
                NextVoucherPreview = v.Prefix + v.Separator + v.NextNumber.ToString().PadLeft(v.NumberLength, '0')
            })
            .ToListAsync();

        return Ok(configs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VoucherNumberingDto>> GetById(Guid id)
    {
        var tenantId = GetTenantId();
        var config = await _db.VoucherNumberings.FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId);
        
        if (config == null)
            return NotFound();

        return Ok(new VoucherNumberingDto
        {
            Id = config.Id,
            TransactionType = config.TransactionType,
            TransactionTypeName = config.TransactionType.ToString(),
            Prefix = config.Prefix,
            NextNumber = config.NextNumber,
            NumberLength = config.NumberLength,
            Separator = config.Separator,
            IsLocked = config.IsLocked,
            IsActive = config.IsActive,
            NextVoucherPreview = config.Prefix + config.Separator + config.NextNumber.ToString().PadLeft(config.NumberLength, '0')
        });
    }

    [HttpPost]
    public async Task<ActionResult<VoucherNumberingDto>> Create([FromBody] CreateVoucherNumberingRequest request)
    {
        var tenantId = GetTenantId();
        
        if (await _db.VoucherNumberings.AnyAsync(v => v.TenantId == tenantId && v.TransactionType == request.TransactionType))
            return BadRequest("Configuration for this transaction type already exists");

        var config = new VoucherNumbering
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TransactionType = request.TransactionType,
            Prefix = request.Prefix,
            NextNumber = request.NextNumber,
            NumberLength = request.NumberLength,
            Separator = request.Separator,
            IsLocked = false,
            IsActive = true
        };

        _db.VoucherNumberings.Add(config);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = config.Id }, new VoucherNumberingDto
        {
            Id = config.Id,
            TransactionType = config.TransactionType,
            TransactionTypeName = config.TransactionType.ToString(),
            Prefix = config.Prefix,
            NextNumber = config.NextNumber,
            NumberLength = config.NumberLength,
            Separator = config.Separator,
            IsLocked = config.IsLocked,
            IsActive = config.IsActive,
            NextVoucherPreview = config.Prefix + config.Separator + config.NextNumber.ToString().PadLeft(config.NumberLength, '0')
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVoucherNumberingRequest request)
    {
        var tenantId = GetTenantId();
        var config = await _db.VoucherNumberings.FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId);
        
        if (config == null)
            return NotFound();

        if (config.IsLocked)
            return BadRequest("Cannot modify locked configuration");

        config.Prefix = request.Prefix;
        config.NextNumber = request.NextNumber;
        config.NumberLength = request.NumberLength;
        config.Separator = request.Separator;
        config.IsActive = request.IsActive;
        config.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = GetTenantId();
        var config = await _db.VoucherNumberings.FirstOrDefaultAsync(v => v.Id == id && v.TenantId == tenantId);
        
        if (config == null)
            return NotFound();

        if (config.IsLocked)
            return BadRequest("Cannot delete locked configuration");

        _db.VoucherNumberings.Remove(config);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class VoucherNumberingDto
{
    public Guid Id { get; set; }
    public VoucherTransactionType TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = "";
    public string Prefix { get; set; } = "";
    public int NextNumber { get; set; }
    public int NumberLength { get; set; }
    public string Separator { get; set; } = "";
    public bool IsLocked { get; set; }
    public bool IsActive { get; set; }
    public string NextVoucherPreview { get; set; } = "";
    public string StatusText => IsLocked ? "Locked" : "Editable";
}

public class CreateVoucherNumberingRequest
{
    public VoucherTransactionType TransactionType { get; set; }
    public string Prefix { get; set; } = "";
    public int NextNumber { get; set; } = 1;
    public int NumberLength { get; set; } = 6;
    public string Separator { get; set; } = "-";
}

public class UpdateVoucherNumberingRequest
{
    public string Prefix { get; set; } = "";
    public int NextNumber { get; set; }
    public int NumberLength { get; set; }
    public string Separator { get; set; } = "";
    public bool IsActive { get; set; }
}
