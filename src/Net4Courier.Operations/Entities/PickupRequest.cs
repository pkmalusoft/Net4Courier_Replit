using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class PickupRequest : AuditableEntity
{
    public string PickupNo { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ScheduledTimeFrom { get; set; }
    public DateTime? ScheduledTimeTo { get; set; }
    public long? PickupScheduleId { get; set; }
    public string? PickupScheduleName { get; set; }
    
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? CustomerId { get; set; }
    public long? CourierId { get; set; }
    public long? AddressId { get; set; }
    
    public string? CustomerName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    
    public string? PickupAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Landmark { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public int? EstimatedPieces { get; set; }
    public decimal? EstimatedWeight { get; set; }
    public int? ActualPieces { get; set; }
    public decimal? ActualWeight { get; set; }
    public string? PackageDescription { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? Remarks { get; set; }
    
    public PickupStatus Status { get; set; } = PickupStatus.PickupRequest;
    public long? StatusId { get; set; }
    public long? StatusGroupId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? CollectedAt { get; set; }
    public DateTime? InscannedAt { get; set; }
    public string? CollectionRemarks { get; set; }
    public string? InscannedBy { get; set; }
    
    public virtual ShipmentStatus? CurrentStatus { get; set; }
    public virtual ShipmentStatusGroup? CurrentStatusGroup { get; set; }
    public virtual ICollection<PickupStatusHistory> StatusHistories { get; set; } = new List<PickupStatusHistory>();
    
    public string? CourierName { get; set; }
    public string? CourierPhone { get; set; }
    
    public string? ReferenceNo { get; set; }
    public string? PONumber { get; set; }
    
    public bool IsConverted { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public string? ConvertedBy { get; set; }
    
    public ICollection<PickupRequestShipment> Shipments { get; set; } = new List<PickupRequestShipment>();
}

public enum PickupStatus
{
    PickupRequest = 1,
    AssignedForCollection = 2,
    Attempted = 3,
    ShipmentCollected = 4,
    Inscanned = 5,
    Cancelled = 6
}
