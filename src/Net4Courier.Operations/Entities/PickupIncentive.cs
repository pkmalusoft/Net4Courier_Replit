using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class IncentiveSchedule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public long? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public IncentiveCalculationType CalculationType { get; set; } = IncentiveCalculationType.PerPiece;
    public decimal IncentiveRate { get; set; }
    public decimal? MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public int? MinPieces { get; set; }
    public int? MaxPieces { get; set; }
    public decimal? BonusAmount { get; set; }
    public int? BonusThreshold { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }
}

public class IncentiveAward : BaseEntity
{
    public long IncentiveScheduleId { get; set; }
    public long CourierId { get; set; }
    public string? CourierName { get; set; }
    public long? PickupRequestId { get; set; }
    public string? PickupNo { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime AwardDate { get; set; }
    public int Pieces { get; set; }
    public decimal Weight { get; set; }
    public decimal IncentiveAmount { get; set; }
    public decimal? BonusAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public IncentiveAwardStatus Status { get; set; } = IncentiveAwardStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public string? PaymentReference { get; set; }
    public long? BranchId { get; set; }
    public string? Remarks { get; set; }
    public virtual IncentiveSchedule IncentiveSchedule { get; set; } = null!;
}

public enum IncentiveCalculationType
{
    PerPiece = 1,
    PerKg = 2,
    FlatRate = 3,
    Percentage = 4
}

public enum IncentiveAwardStatus
{
    Pending = 1,
    Approved = 2,
    Paid = 3,
    Cancelled = 4
}
