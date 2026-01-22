using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public enum CommitmentStatus
{
    Committed,
    Released,
    Completed,
    Expired
}

public class PickupCommitment : BaseEntity
{
    public Guid PickupRequestId { get; set; }
    public PickupRequest? PickupRequest { get; set; }

    public Guid CourierAgentId { get; set; }
    public CourierAgent? CourierAgent { get; set; }

    public CommitmentStatus Status { get; set; } = CommitmentStatus.Committed;

    public DateTime CommittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReleasedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    [MaxLength(500)]
    public string? ReleaseReason { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
