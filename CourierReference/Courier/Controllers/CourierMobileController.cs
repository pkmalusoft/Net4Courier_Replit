using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier/mobile")]
[Authorize]
public class CourierMobileController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmployeeZoneService _employeeZoneService;
    private readonly IPickupCommitmentService _commitmentService;
    private readonly IPickupIncentiveService _incentiveService;

    public CourierMobileController(
        AppDbContext context,
        IEmployeeZoneService employeeZoneService,
        IPickupCommitmentService commitmentService,
        IPickupIncentiveService incentiveService)
    {
        _context = context;
        _employeeZoneService = employeeZoneService;
        _commitmentService = commitmentService;
        _incentiveService = incentiveService;
    }

    [HttpGet("zones/{courierAgentId}")]
    public async Task<ActionResult<List<MobileZoneDto>>> GetAssignedZones(Guid courierAgentId)
    {
        var zones = await _employeeZoneService.GetZonesForCourierAsync(courierAgentId);
        return Ok(zones.Select(z => new MobileZoneDto
        {
            Id = z.Id,
            ZoneCode = z.ZoneCode,
            ZoneName = z.ZoneName,
            ZoneType = z.ZoneType.ToString()
        }).ToList());
    }

    [HttpGet("pickups/{courierAgentId}")]
    public async Task<ActionResult<List<MobilePickupDto>>> GetAvailablePickups(
        Guid courierAgentId,
        [FromQuery] Guid? zoneId = null)
    {
        var assignedZones = await _employeeZoneService.GetZonesForCourierAsync(courierAgentId);
        var zoneIds = zoneId.HasValue 
            ? new List<Guid> { zoneId.Value } 
            : assignedZones.Select(z => z.Id).ToList();

        var pickups = await _context.PickupRequests
            .Include(p => p.Customer)
            .Include(p => p.CourierZone)
            .Include(p => p.Commitments.Where(c => c.Status == CommitmentStatus.Committed))
                .ThenInclude(c => c.CourierAgent)
            .Where(p => p.CourierZoneId != null && zoneIds.Contains(p.CourierZoneId.Value))
            .Where(p => p.Status == PickupStatus.Requested || 
                       p.Status == PickupStatus.Confirmed || 
                       p.Status == PickupStatus.Assigned)
            .Where(p => !p.IsVoided)
            .OrderBy(p => p.ScheduledDate)
            .ToListAsync();

        var result = new List<MobilePickupDto>();
        foreach (var p in pickups)
        {
            var activeCommitment = p.Commitments.FirstOrDefault(c => c.Status == CommitmentStatus.Committed);
            var isCommittedByMe = activeCommitment?.CourierAgentId == courierAgentId;
            
            var schedule = await _incentiveService.GetApplicableScheduleAsync(p.CourierZoneId, p.CustomerId);

            result.Add(new MobilePickupDto
            {
                Id = p.Id,
                RequestNumber = p.RequestNumber,
                RequestDate = p.RequestDate,
                ScheduledDate = p.ScheduledDate,
                PreferredTimeFrom = p.PreferredTimeFrom,
                PreferredTimeTo = p.PreferredTimeTo,
                CustomerName = p.Customer?.Name,
                ContactName = p.ContactName,
                ContactPhone = p.ContactPhone,
                PickupAddress = p.PickupAddress,
                City = p.City,
                PostalCode = p.PostalCode,
                ExpectedPieces = p.ExpectedPieces,
                ExpectedWeight = p.ExpectedWeight,
                SpecialInstructions = p.SpecialInstructions,
                ZoneCode = p.CourierZone?.ZoneCode,
                ZoneName = p.CourierZone?.ZoneName,
                Status = p.Status.ToString(),
                IsCommitted = activeCommitment != null,
                IsCommittedByMe = isCommittedByMe,
                CommittedByName = activeCommitment?.CourierAgent?.Name,
                CommitmentId = activeCommitment?.Id,
                IncentiveAmount = schedule?.Amount,
                IncentiveCurrency = schedule?.Currency,
                IncentiveCalculationMode = schedule?.CalculationMode.ToString()
            });
        }

        return Ok(result);
    }

    [HttpPost("pickups/{pickupRequestId}/commit")]
    public async Task<ActionResult<CommitmentResultDto>> CommitToPickup(
        Guid pickupRequestId, 
        [FromBody] CommitPickupDto dto)
    {
        var result = await _commitmentService.CommitAsync(pickupRequestId, dto.CourierAgentId, dto.ExpiryMinutes);

        return Ok(new CommitmentResultDto
        {
            Success = result.Success,
            Message = result.Message,
            CommitmentId = result.Commitment?.Id
        });
    }

    [HttpPost("commitments/{commitmentId}/release")]
    public async Task<ActionResult<CommitmentResultDto>> ReleaseCommitment(
        Guid commitmentId,
        [FromBody] ReleaseCommitmentDto dto)
    {
        var result = await _commitmentService.ReleaseAsync(commitmentId, dto.CourierAgentId, dto.Reason);

        return Ok(new CommitmentResultDto
        {
            Success = result.Success,
            Message = result.Message
        });
    }

    [HttpPost("commitments/{commitmentId}/complete")]
    public async Task<ActionResult<CommitmentResultDto>> CompleteCommitment(
        Guid commitmentId,
        [FromBody] CompleteCommitmentDto dto)
    {
        var result = await _commitmentService.CompleteAsync(commitmentId, dto.CourierAgentId);

        return Ok(new CommitmentResultDto
        {
            Success = result.Success,
            Message = result.Message
        });
    }

    [HttpGet("incentives/{courierAgentId}")]
    public async Task<ActionResult<CourierIncentiveSummaryDto>> GetIncentiveSummary(
        Guid courierAgentId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var awards = await _incentiveService.GetAwardsByCourierAsync(courierAgentId);
        var total = await _incentiveService.GetTotalIncentivesForCourierAsync(courierAgentId, fromDate, toDate);

        var recentAwards = awards.Take(10).Select(a => new MobileIncentiveAwardDto
        {
            Id = a.Id,
            PickupRequestNumber = a.PickupCommitment?.PickupRequest?.RequestNumber,
            Amount = a.Amount,
            Currency = a.Currency,
            Status = a.Status.ToString(),
            AwardedAt = a.AwardedAt
        }).ToList();

        return Ok(new CourierIncentiveSummaryDto
        {
            TotalEarned = total,
            Currency = "AED",
            TotalPickupsCompleted = awards.Count(a => a.Status != IncentiveAwardStatus.Cancelled),
            PendingAmount = awards.Where(a => a.Status == IncentiveAwardStatus.Pending).Sum(a => a.Amount),
            ApprovedAmount = awards.Where(a => a.Status == IncentiveAwardStatus.Approved).Sum(a => a.Amount),
            PaidAmount = awards.Where(a => a.Status == IncentiveAwardStatus.Paid).Sum(a => a.Amount),
            RecentAwards = recentAwards
        });
    }

    [HttpGet("my-commitments/{courierAgentId}")]
    public async Task<ActionResult<List<MobileCommitmentDto>>> GetMyCommitments(Guid courierAgentId)
    {
        var commitments = await _commitmentService.GetByCourierAsync(courierAgentId);
        
        return Ok(commitments.Select(c => new MobileCommitmentDto
        {
            Id = c.Id,
            PickupRequestId = c.PickupRequestId,
            PickupRequestNumber = c.PickupRequest?.RequestNumber,
            Status = c.Status.ToString(),
            CommittedAt = c.CommittedAt,
            ExpiresAt = c.ExpiresAt,
            ReleasedAt = c.ReleasedAt,
            CompletedAt = c.CompletedAt
        }).ToList());
    }
}

public class MobileZoneDto
{
    public Guid Id { get; set; }
    public string ZoneCode { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string? ZoneType { get; set; }
}

public class MobilePickupDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime ScheduledDate { get; set; }
    public TimeSpan? PreferredTimeFrom { get; set; }
    public TimeSpan? PreferredTimeTo { get; set; }
    public string? CustomerName { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public int ExpectedPieces { get; set; }
    public decimal? ExpectedWeight { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? ZoneCode { get; set; }
    public string? ZoneName { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsCommitted { get; set; }
    public bool IsCommittedByMe { get; set; }
    public string? CommittedByName { get; set; }
    public Guid? CommitmentId { get; set; }
    public decimal? IncentiveAmount { get; set; }
    public string? IncentiveCurrency { get; set; }
    public string? IncentiveCalculationMode { get; set; }
}

public class CommitPickupDto
{
    public Guid CourierAgentId { get; set; }
    public int? ExpiryMinutes { get; set; }
}

public class ReleaseCommitmentDto
{
    public Guid CourierAgentId { get; set; }
    public string? Reason { get; set; }
}

public class CompleteCommitmentDto
{
    public Guid CourierAgentId { get; set; }
}

public class CommitmentResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? CommitmentId { get; set; }
}

public class CourierIncentiveSummaryDto
{
    public decimal TotalEarned { get; set; }
    public string Currency { get; set; } = "AED";
    public int TotalPickupsCompleted { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public List<MobileIncentiveAwardDto> RecentAwards { get; set; } = new();
}

public class MobileIncentiveAwardDto
{
    public Guid Id { get; set; }
    public string? PickupRequestNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AwardedAt { get; set; }
}

public class MobileCommitmentDto
{
    public Guid Id { get; set; }
    public Guid PickupRequestId { get; set; }
    public string? PickupRequestNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CommittedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
