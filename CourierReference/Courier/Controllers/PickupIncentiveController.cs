using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PickupIncentiveController : ControllerBase
{
    private readonly IPickupIncentiveService _incentiveService;

    public PickupIncentiveController(IPickupIncentiveService incentiveService)
    {
        _incentiveService = incentiveService;
    }

    [HttpGet("schedules")]
    public async Task<ActionResult<List<PickupIncentiveScheduleDto>>> GetAllSchedules([FromQuery] string? type = null)
    {
        IncentiveType? incentiveType = null;
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<IncentiveType>(type, true, out var parsedType))
        {
            incentiveType = parsedType;
        }
        var schedules = await _incentiveService.GetAllSchedulesAsync(incentiveType);
        return Ok(schedules.Select(MapToDto).ToList());
    }

    [HttpGet("schedules/zone/{zoneId}")]
    public async Task<ActionResult<List<PickupIncentiveScheduleDto>>> GetSchedulesByZone(Guid zoneId, [FromQuery] string? type = null)
    {
        IncentiveType? incentiveType = null;
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<IncentiveType>(type, true, out var parsedType))
        {
            incentiveType = parsedType;
        }
        var schedules = await _incentiveService.GetSchedulesByZoneAsync(zoneId, incentiveType);
        return Ok(schedules.Select(MapToDto).ToList());
    }

    [HttpGet("schedules/{id}")]
    public async Task<ActionResult<PickupIncentiveScheduleDto>> GetScheduleById(Guid id)
    {
        var schedule = await _incentiveService.GetScheduleByIdAsync(id);
        if (schedule == null) return NotFound();
        return Ok(MapToDto(schedule));
    }

    [HttpPost("schedules")]
    public async Task<ActionResult<PickupIncentiveScheduleDto>> CreateSchedule([FromBody] CreateIncentiveScheduleDto dto)
    {
        var incentiveType = IncentiveType.Pickup;
        if (!string.IsNullOrEmpty(dto.IncentiveType) && Enum.TryParse<IncentiveType>(dto.IncentiveType, true, out var parsedType))
        {
            incentiveType = parsedType;
        }

        var calculationMode = IncentiveCalculationMode.FlatAmount;
        if (!string.IsNullOrEmpty(dto.CalculationMode) && Enum.TryParse<IncentiveCalculationMode>(dto.CalculationMode, true, out var parsedCalcMode))
        {
            calculationMode = parsedCalcMode;
        }

        var schedule = new PickupIncentiveSchedule
        {
            IncentiveType = incentiveType,
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            CourierZoneId = dto.CourierZoneId,
            CustomerId = dto.CustomerId,
            CalculationMode = calculationMode,
            Amount = dto.Amount,
            Currency = dto.Currency,
            MinimumAmount = dto.MinimumAmount,
            MaximumAmount = dto.MaximumAmount,
            SLACommitMinutes = dto.SLACommitMinutes,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            Priority = dto.Priority,
            IsActive = true
        };

        var created = await _incentiveService.CreateScheduleAsync(schedule);
        return CreatedAtAction(nameof(GetScheduleById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPut("schedules/{id}")]
    public async Task<ActionResult<PickupIncentiveScheduleDto>> UpdateSchedule(Guid id, [FromBody] UpdateIncentiveScheduleDto dto)
    {
        var existing = await _incentiveService.GetScheduleByIdAsync(id);
        if (existing == null) return NotFound();

        if (!string.IsNullOrEmpty(dto.IncentiveType) && Enum.TryParse<IncentiveType>(dto.IncentiveType, true, out var parsedType))
        {
            existing.IncentiveType = parsedType;
        }

        if (!string.IsNullOrEmpty(dto.CalculationMode) && Enum.TryParse<IncentiveCalculationMode>(dto.CalculationMode, true, out var parsedCalcMode))
        {
            existing.CalculationMode = parsedCalcMode;
        }

        existing.Code = dto.Code;
        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.CourierZoneId = dto.CourierZoneId;
        existing.CustomerId = dto.CustomerId;
        existing.Amount = dto.Amount;
        existing.Currency = dto.Currency;
        existing.MinimumAmount = dto.MinimumAmount;
        existing.MaximumAmount = dto.MaximumAmount;
        existing.SLACommitMinutes = dto.SLACommitMinutes;
        existing.ValidFrom = dto.ValidFrom;
        existing.ValidTo = dto.ValidTo;
        existing.Priority = dto.Priority;
        existing.IsActive = dto.IsActive;

        var updated = await _incentiveService.UpdateScheduleAsync(existing);
        return Ok(MapToDto(updated));
    }

    [HttpDelete("schedules/{id}")]
    public async Task<ActionResult> DeleteSchedule(Guid id)
    {
        var result = await _incentiveService.DeleteScheduleAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("awards/courier/{courierAgentId}")]
    public async Task<ActionResult<List<PickupIncentiveAwardDto>>> GetAwardsByCourier(Guid courierAgentId)
    {
        var awards = await _incentiveService.GetAwardsByCourierAsync(courierAgentId);
        return Ok(awards.Select(MapAwardToDto).ToList());
    }

    [HttpGet("awards/courier/{courierAgentId}/total")]
    public async Task<ActionResult<decimal>> GetTotalIncentives(
        Guid courierAgentId, 
        [FromQuery] DateTime? fromDate = null, 
        [FromQuery] DateTime? toDate = null)
    {
        var total = await _incentiveService.GetTotalIncentivesForCourierAsync(courierAgentId, fromDate, toDate);
        return Ok(total);
    }

    private static PickupIncentiveScheduleDto MapToDto(PickupIncentiveSchedule schedule) => new()
    {
        Id = schedule.Id,
        IncentiveType = schedule.IncentiveType.ToString(),
        Code = schedule.Code,
        Name = schedule.Name,
        Description = schedule.Description,
        CourierZoneId = schedule.CourierZoneId,
        ZoneName = schedule.CourierZone?.ZoneName,
        CustomerId = schedule.CustomerId,
        CustomerName = schedule.Customer?.Name,
        CalculationMode = schedule.CalculationMode.ToString(),
        Amount = schedule.Amount,
        Currency = schedule.Currency,
        MinimumAmount = schedule.MinimumAmount,
        MaximumAmount = schedule.MaximumAmount,
        SLACommitMinutes = schedule.SLACommitMinutes,
        ValidFrom = schedule.ValidFrom,
        ValidTo = schedule.ValidTo,
        Priority = schedule.Priority,
        IsActive = schedule.IsActive
    };

    private static PickupIncentiveAwardDto MapAwardToDto(PickupIncentiveAward award) => new()
    {
        Id = award.Id,
        PickupCommitmentId = award.PickupCommitmentId,
        PickupRequestNumber = award.PickupCommitment?.PickupRequest?.RequestNumber,
        ScheduleCode = award.PickupIncentiveSchedule?.Code,
        ScheduleName = award.PickupIncentiveSchedule?.Name,
        Amount = award.Amount,
        Currency = award.Currency,
        Status = award.Status.ToString(),
        AwardedAt = award.AwardedAt,
        ApprovedAt = award.ApprovedAt,
        PaidAt = award.PaidAt
    };
}

public class PickupIncentiveScheduleDto
{
    public Guid Id { get; set; }
    public string IncentiveType { get; set; } = "Pickup";
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CourierZoneId { get; set; }
    public string? ZoneName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string CalculationMode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public int? SLACommitMinutes { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
}

public class CreateIncentiveScheduleDto
{
    public string IncentiveType { get; set; } = "Pickup";
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CourierZoneId { get; set; }
    public Guid? CustomerId { get; set; }
    public string CalculationMode { get; set; } = "FlatAmount";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public int? SLACommitMinutes { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
    public DateTime? ValidTo { get; set; }
    public int Priority { get; set; }
}

public class UpdateIncentiveScheduleDto
{
    public string IncentiveType { get; set; } = "Pickup";
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CourierZoneId { get; set; }
    public Guid? CustomerId { get; set; }
    public string CalculationMode { get; set; } = "FlatAmount";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public int? SLACommitMinutes { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
}

public class PickupIncentiveAwardDto
{
    public Guid Id { get; set; }
    public Guid PickupCommitmentId { get; set; }
    public string? PickupRequestNumber { get; set; }
    public string? ScheduleCode { get; set; }
    public string? ScheduleName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AwardedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
