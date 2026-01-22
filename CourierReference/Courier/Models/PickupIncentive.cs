using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Modules.Courier.Models;

public enum IncentiveType
{
    Pickup,
    Delivery,
    Collection
}

public enum IncentiveCalculationMode
{
    FlatAmount,
    PerPiece,
    PerKg,
    PercentOfCOD,
    PercentOfFreight
}

public class PickupIncentiveSchedule : BaseEntity
{
    public IncentiveType IncentiveType { get; set; } = IncentiveType.Pickup;
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? CourierZoneId { get; set; }
    public CourierZone? CourierZone { get; set; }

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public IncentiveCalculationMode CalculationMode { get; set; } = IncentiveCalculationMode.FlatAmount;

    [Column(TypeName = "decimal(18,4)")]
    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "AED";

    [Column(TypeName = "decimal(18,4)")]
    public decimal? MinimumAmount { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? MaximumAmount { get; set; }

    public int? SLACommitMinutes { get; set; }

    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

    public DateTime? ValidTo { get; set; }

    public bool IsActive { get; set; } = true;

    public int Priority { get; set; } = 0;

    public ICollection<PickupIncentiveAward> Awards { get; set; } = new List<PickupIncentiveAward>();
}

public enum IncentiveAwardStatus
{
    Pending,
    Approved,
    Paid,
    Cancelled
}

public class PickupIncentiveAward : BaseEntity
{
    public Guid PickupCommitmentId { get; set; }
    public PickupCommitment? PickupCommitment { get; set; }

    public Guid PickupIncentiveScheduleId { get; set; }
    public PickupIncentiveSchedule? PickupIncentiveSchedule { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "AED";

    public IncentiveAwardStatus Status { get; set; } = IncentiveAwardStatus.Pending;

    [MaxLength(2000)]
    public string? CalculationSnapshot { get; set; }

    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ApprovedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public Guid? ApprovedById { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
