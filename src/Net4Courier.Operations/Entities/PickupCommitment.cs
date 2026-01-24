using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class PickupCommitment : BaseEntity
{
    public long PickupRequestId { get; set; }
    public long CourierId { get; set; }
    public string? CourierName { get; set; }
    public DateTime CommittedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public PickupCommitmentStatus Status { get; set; } = PickupCommitmentStatus.Active;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public string? ReleaseReason { get; set; }
    public long? ReleasedByUserId { get; set; }
    public string? ReleasedByUserName { get; set; }
    public string? Remarks { get; set; }
    public long? BranchId { get; set; }
    public virtual PickupRequest PickupRequest { get; set; } = null!;
}

public enum PickupCommitmentStatus
{
    Active = 1,
    Confirmed = 2,
    Released = 3,
    Expired = 4,
    Completed = 5
}
