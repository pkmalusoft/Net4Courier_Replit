using System.ComponentModel.DataAnnotations;
using Server.Core.Common;

namespace Server.Modules.Courier.Models;

public enum PickupStatus
{
    Requested,
    Confirmed,
    Assigned,
    InProgress,
    Completed,
    Cancelled,
    Failed
}

public class PickupRequest : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string RequestNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? AWBNumber { get; set; }

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    [System.ComponentModel.DataAnnotations.Schema.Column("ScheduledPickupDate")]
    public DateTime ScheduledDate { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.Column("PreferredTimeFrom")]
    public TimeSpan? PreferredTimeFrom { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.Column("PreferredTimeTo")]
    public TimeSpan? PreferredTimeTo { get; set; }

    public DateTime? ActualPickupTime { get; set; }

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [Required]
    [MaxLength(200)]
    [System.ComponentModel.DataAnnotations.Schema.Column("ContactPerson")]
    public string ContactName { get; set; } = string.Empty;

    [MaxLength(20)]
    [System.ComponentModel.DataAnnotations.Schema.Column("Phone")]
    public string? ContactPhone { get; set; }

    [MaxLength(100)]
    [System.ComponentModel.DataAnnotations.Schema.Column("Email")]
    public string? ContactEmail { get; set; }

    [Required]
    [MaxLength(500)]
    public string PickupAddress { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public int ExpectedPieces { get; set; } = 1;

    public decimal? ExpectedWeight { get; set; }

    public PickupStatus Status { get; set; } = PickupStatus.Requested;

    public Guid? AssignedAgentId { get; set; }
    public CourierAgent? AssignedAgent { get; set; }

    public Guid? CourierZoneId { get; set; }
    public CourierZone? CourierZone { get; set; }

    public int ActualPieces { get; set; }

    public decimal? ActualWeight { get; set; }

    [MaxLength(1000)]
    public string? SpecialInstructions { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    [MaxLength(500)]
    public string? FailureReason { get; set; }

    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public bool IsVoided { get; set; } = false;

    [MaxLength(500)]
    public string? CollectionPhotoUrl { get; set; }

    [MaxLength(500)]
    public string? CollectionPhotoFileName { get; set; }

    public DateTime? CollectionPhotoTakenAt { get; set; }

    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    
    public ICollection<PickupCommitment> Commitments { get; set; } = new List<PickupCommitment>();
}
