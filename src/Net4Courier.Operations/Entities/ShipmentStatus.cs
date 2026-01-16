using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Operations.Entities;

public class ShipmentStatus : BaseEntity
{
    public long StatusGroupId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TimelineDescription { get; set; }
    public int SequenceNo { get; set; }
    public bool IsTerminal { get; set; }
    public bool IsException { get; set; }
    public CourierStatus? MapsToCourierStatus { get; set; }
    public string? IconName { get; set; }
    public string? ColorCode { get; set; }
    public bool RequiresPOD { get; set; }
    public bool RequiresLocation { get; set; }
    public bool RequiresRemarks { get; set; }
    
    public virtual ShipmentStatusGroup StatusGroup { get; set; } = null!;
    public virtual ICollection<ShipmentStatusHistory> History { get; set; } = new List<ShipmentStatusHistory>();
}
