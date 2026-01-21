using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class PickupStatusHistory : BaseEntity
{
    public long PickupRequestId { get; set; }
    public long StatusId { get; set; }
    public long StatusGroupId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public long? EventRefId { get; set; }
    public string? EventRefType { get; set; }
    public long? BranchId { get; set; }
    public string? LocationName { get; set; }
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Remarks { get; set; }
    public DateTime ChangedAt { get; set; }
    public bool IsAutomatic { get; set; }
    public string? DeviceInfo { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    public virtual PickupRequest PickupRequest { get; set; } = null!;
    public virtual ShipmentStatus Status { get; set; } = null!;
    public virtual ShipmentStatusGroup StatusGroup { get; set; } = null!;
}
