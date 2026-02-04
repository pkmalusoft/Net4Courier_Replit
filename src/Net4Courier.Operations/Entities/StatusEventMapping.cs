using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class StatusEventMapping : BaseEntity
{
    public string EventCode { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long StatusId { get; set; }
    public int SequenceNo { get; set; }
    public bool IsAutoApply { get; set; } = true;
    
    public virtual ShipmentStatus Status { get; set; } = null!;
}
