using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/zone-categories")]
[Authorize]
public class ZoneCategoryController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<ZoneCategoryController> _logger;

    public ZoneCategoryController(
        AppDbContext context, 
        ITenantProvider tenantProvider,
        ILogger<ZoneCategoryController> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ZoneCategoryDto>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required");

        var query = _context.ZoneCategories
            .Where(z => z.TenantId == tenantId.Value);
        
        if (!includeInactive)
            query = query.Where(z => z.IsActive);

        var categories = await query
            .OrderBy(z => z.SortOrder)
            .ThenBy(z => z.Name)
            .Select(z => new ZoneCategoryDto
            {
                Id = z.Id,
                Code = z.Code,
                Name = z.Name,
                Description = z.Description,
                CarrierName = z.CarrierName,
                LogoUrl = z.LogoUrl,
                SortOrder = z.SortOrder,
                IsActive = z.IsActive,
                ZoneCount = z.CourierZones.Count(cz => cz.IsActive)
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ZoneCategoryDto>> GetById(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required");

        var category = await _context.ZoneCategories
            .Where(z => z.Id == id && z.TenantId == tenantId.Value)
            .Select(z => new ZoneCategoryDto
            {
                Id = z.Id,
                Code = z.Code,
                Name = z.Name,
                Description = z.Description,
                CarrierName = z.CarrierName,
                LogoUrl = z.LogoUrl,
                SortOrder = z.SortOrder,
                IsActive = z.IsActive,
                ZoneCount = z.CourierZones.Count(cz => cz.IsActive)
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound("Zone category not found");

        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<ZoneCategoryDto>> Create([FromBody] CreateZoneCategoryRequest request)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required");

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Code and Name are required");

        var exists = await _context.ZoneCategories
            .AnyAsync(z => z.TenantId == tenantId.Value && z.Code == request.Code);
        
        if (exists)
            return BadRequest($"Zone category with code '{request.Code}' already exists");

        var category = new ZoneCategory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Description = request.Description,
            CarrierName = request.CarrierName,
            LogoUrl = request.LogoUrl,
            SortOrder = request.SortOrder ?? 0,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.ZoneCategories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, new ZoneCategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            CarrierName = category.CarrierName,
            LogoUrl = category.LogoUrl,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            ZoneCount = 0
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ZoneCategoryDto>> Update(Guid id, [FromBody] UpdateZoneCategoryRequest request)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required");

        var category = await _context.ZoneCategories
            .FirstOrDefaultAsync(z => z.Id == id && z.TenantId == tenantId.Value);

        if (category == null)
            return NotFound("Zone category not found");

        if (!string.IsNullOrWhiteSpace(request.Code) && request.Code != category.Code)
        {
            var exists = await _context.ZoneCategories
                .AnyAsync(z => z.TenantId == tenantId.Value && z.Code == request.Code && z.Id != id);
            if (exists)
                return BadRequest($"Zone category with code '{request.Code}' already exists");
            category.Code = request.Code.ToUpper();
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) category.Name = request.Name;
        if (request.Description != null) category.Description = request.Description;
        if (request.CarrierName != null) category.CarrierName = request.CarrierName;
        if (request.LogoUrl != null) category.LogoUrl = request.LogoUrl;
        if (request.SortOrder.HasValue) category.SortOrder = request.SortOrder.Value;
        if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var zoneCount = await _context.CourierZones
            .CountAsync(z => z.ZoneCategoryId == id && z.IsActive);

        return Ok(new ZoneCategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            CarrierName = category.CarrierName,
            LogoUrl = category.LogoUrl,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            ZoneCount = zoneCount
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required");

        var category = await _context.ZoneCategories
            .FirstOrDefaultAsync(z => z.Id == id && z.TenantId == tenantId.Value);

        if (category == null)
            return NotFound("Zone category not found");

        var hasZones = await _context.CourierZones
            .AnyAsync(z => z.ZoneCategoryId == id);

        if (hasZones)
            return BadRequest("Cannot delete zone category with existing zones. Deactivate it instead or reassign the zones first.");

        _context.ZoneCategories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Zone category deleted successfully" });
    }

    [HttpPost("{id:guid}/toggle-status")]
    public async Task<ActionResult<ZoneCategoryDto>> ToggleStatus(Guid id)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required");

        var category = await _context.ZoneCategories
            .FirstOrDefaultAsync(z => z.Id == id && z.TenantId == tenantId.Value);

        if (category == null)
            return NotFound("Zone category not found");

        category.IsActive = !category.IsActive;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var zoneCount = await _context.CourierZones
            .CountAsync(z => z.ZoneCategoryId == id && z.IsActive);

        return Ok(new ZoneCategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            CarrierName = category.CarrierName,
            LogoUrl = category.LogoUrl,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            ZoneCount = zoneCount
        });
    }
}

public class ZoneCategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CarrierName { get; set; }
    public string? LogoUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int ZoneCount { get; set; }
}

public class CreateZoneCategoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CarrierName { get; set; }
    public string? LogoUrl { get; set; }
    public int? SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateZoneCategoryRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CarrierName { get; set; }
    public string? LogoUrl { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}
