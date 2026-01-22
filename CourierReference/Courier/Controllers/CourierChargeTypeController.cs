using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/charge-types")]
[Authorize]
public class CourierChargeTypeController : ControllerBase
{
    private readonly CourierChargeTypeService _chargeTypeService;

    public CourierChargeTypeController(CourierChargeTypeService chargeTypeService)
    {
        _chargeTypeService = chargeTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CourierChargeTypeDto>>> GetAll()
    {
        var chargeTypes = await _chargeTypeService.GetAllAsync();
        return Ok(chargeTypes.Select(MapToDto).ToList());
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<CourierChargeTypeDto>>> GetActive()
    {
        var chargeTypes = await _chargeTypeService.GetActiveAsync();
        return Ok(chargeTypes.Select(MapToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourierChargeTypeDto>> GetById(Guid id)
    {
        var chargeType = await _chargeTypeService.GetByIdAsync(id);
        if (chargeType == null)
        {
            return NotFound();
        }
        return Ok(MapToDto(chargeType));
    }

    [HttpPost]
    public async Task<ActionResult<CourierChargeTypeDto>> Create([FromBody] CreateCourierChargeTypeRequest request)
    {
        try
        {
            var chargeType = new CourierChargeType
            {
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                DefaultAmount = request.DefaultAmount,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder
            };

            var created = await _chargeTypeService.CreateAsync(chargeType);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CourierChargeTypeDto>> Update(Guid id, [FromBody] UpdateCourierChargeTypeRequest request)
    {
        try
        {
            var chargeType = new CourierChargeType
            {
                Id = id,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                DefaultAmount = request.DefaultAmount,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder
            };

            var updated = await _chargeTypeService.UpdateAsync(chargeType);
            return Ok(MapToDto(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _chargeTypeService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("seed-defaults")]
    public async Task<ActionResult> SeedDefaults()
    {
        await _chargeTypeService.EnsureDefaultChargeTypesAsync();
        return Ok(new { message = "Default charge types seeded successfully" });
    }

    private static CourierChargeTypeDto MapToDto(CourierChargeType chargeType)
    {
        return new CourierChargeTypeDto
        {
            Id = chargeType.Id,
            Code = chargeType.Code,
            Name = chargeType.Name,
            Description = chargeType.Description,
            DefaultAmount = chargeType.DefaultAmount,
            IsActive = chargeType.IsActive,
            SortOrder = chargeType.SortOrder,
            IsSystemDefault = chargeType.IsSystemDefault
        };
    }
}

public class CourierChargeTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultAmount { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public bool IsSystemDefault { get; set; }
}

public class CreateCourierChargeTypeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultAmount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpdateCourierChargeTypeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultAmount { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
