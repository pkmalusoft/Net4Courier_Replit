using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeZoneController : ControllerBase
{
    private readonly IEmployeeZoneService _employeeZoneService;
    private readonly ICourierZoneService _courierZoneService;
    private readonly ICourierAgentService _courierAgentService;

    public EmployeeZoneController(
        IEmployeeZoneService employeeZoneService,
        ICourierZoneService courierZoneService,
        ICourierAgentService courierAgentService)
    {
        _employeeZoneService = employeeZoneService;
        _courierZoneService = courierZoneService;
        _courierAgentService = courierAgentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeZoneAssignmentDto>>> GetAll()
    {
        var assignments = await _employeeZoneService.GetAllAsync();
        return Ok(assignments.Select(MapToDto).ToList());
    }

    [HttpGet("zone/{zoneId}")]
    public async Task<ActionResult<List<EmployeeZoneAssignmentDto>>> GetByZone(Guid zoneId)
    {
        var assignments = await _employeeZoneService.GetByZoneAsync(zoneId);
        return Ok(assignments.Select(MapToDto).ToList());
    }

    [HttpGet("courier/{courierAgentId}")]
    public async Task<ActionResult<List<EmployeeZoneAssignmentDto>>> GetByCourier(Guid courierAgentId)
    {
        var assignments = await _employeeZoneService.GetByCourierAsync(courierAgentId);
        return Ok(assignments.Select(MapToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeZoneAssignmentDto>> GetById(Guid id)
    {
        var assignment = await _employeeZoneService.GetByIdAsync(id);
        if (assignment == null) return NotFound();
        return Ok(MapToDto(assignment));
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeZoneAssignmentDto>> Create([FromBody] CreateEmployeeZoneAssignmentDto dto)
    {
        var assignment = new EmployeeZoneAssignment
        {
            CourierAgentId = dto.CourierAgentId,
            CourierZoneId = dto.CourierZoneId,
            PriorityRank = dto.PriorityRank,
            IsPrimary = dto.IsPrimary,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            Notes = dto.Notes,
            IsActive = true
        };

        var created = await _employeeZoneService.CreateAsync(assignment);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeZoneAssignmentDto>> Update(Guid id, [FromBody] UpdateEmployeeZoneAssignmentDto dto)
    {
        var existing = await _employeeZoneService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.CourierAgentId = dto.CourierAgentId;
        existing.CourierZoneId = dto.CourierZoneId;
        existing.PriorityRank = dto.PriorityRank;
        existing.IsPrimary = dto.IsPrimary;
        existing.EffectiveFrom = dto.EffectiveFrom;
        existing.EffectiveTo = dto.EffectiveTo;
        existing.Notes = dto.Notes;
        existing.IsActive = dto.IsActive;

        var updated = await _employeeZoneService.UpdateAsync(existing);
        return Ok(MapToDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _employeeZoneService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("couriers-for-zone/{zoneId}")]
    public async Task<ActionResult<List<EmployeeZoneCourierAgentDto>>> GetCouriersForZone(Guid zoneId)
    {
        var couriers = await _employeeZoneService.GetCouriersForZoneAsync(zoneId);
        return Ok(couriers.Select(c => new EmployeeZoneCourierAgentDto
        {
            Id = c.Id,
            AgentCode = c.AgentCode,
            Name = c.Name,
            Mobile = c.Mobile,
            AgentType = c.AgentType.ToString()
        }).ToList());
    }

    [HttpGet("zones-for-courier/{courierAgentId}")]
    public async Task<ActionResult<List<EmployeeZoneCourierZoneDto>>> GetZonesForCourier(Guid courierAgentId)
    {
        var zones = await _employeeZoneService.GetZonesForCourierAsync(courierAgentId);
        return Ok(zones.Select(z => new EmployeeZoneCourierZoneDto
        {
            Id = z.Id,
            ZoneCode = z.ZoneCode,
            ZoneName = z.ZoneName,
            ZoneType = z.ZoneType.ToString()
        }).ToList());
    }

    private static EmployeeZoneAssignmentDto MapToDto(EmployeeZoneAssignment assignment) => new()
    {
        Id = assignment.Id,
        CourierAgentId = assignment.CourierAgentId,
        CourierAgentCode = assignment.CourierAgent?.AgentCode,
        CourierAgentName = assignment.CourierAgent?.Name,
        CourierZoneId = assignment.CourierZoneId,
        ZoneCode = assignment.CourierZone?.ZoneCode,
        ZoneName = assignment.CourierZone?.ZoneName,
        PriorityRank = assignment.PriorityRank,
        IsPrimary = assignment.IsPrimary,
        EffectiveFrom = assignment.EffectiveFrom,
        EffectiveTo = assignment.EffectiveTo,
        Notes = assignment.Notes,
        IsActive = assignment.IsActive
    };
}

public class EmployeeZoneAssignmentDto
{
    public Guid Id { get; set; }
    public Guid CourierAgentId { get; set; }
    public string? CourierAgentCode { get; set; }
    public string? CourierAgentName { get; set; }
    public Guid CourierZoneId { get; set; }
    public string? ZoneCode { get; set; }
    public string? ZoneName { get; set; }
    public int PriorityRank { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateEmployeeZoneAssignmentDto
{
    public Guid CourierAgentId { get; set; }
    public Guid CourierZoneId { get; set; }
    public int PriorityRank { get; set; } = 1;
    public bool IsPrimary { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
}

public class UpdateEmployeeZoneAssignmentDto
{
    public Guid CourierAgentId { get; set; }
    public Guid CourierZoneId { get; set; }
    public int PriorityRank { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class EmployeeZoneCourierAgentDto
{
    public Guid Id { get; set; }
    public string AgentCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? AgentType { get; set; }
}

public class EmployeeZoneCourierZoneDto
{
    public Guid Id { get; set; }
    public string ZoneCode { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string? ZoneType { get; set; }
}
