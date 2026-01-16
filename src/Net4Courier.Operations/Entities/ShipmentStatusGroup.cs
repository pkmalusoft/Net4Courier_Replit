using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class ShipmentStatusGroup : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SequenceNo { get; set; }
    public string? IconName { get; set; }
    public string? ColorCode { get; set; }
    
    public virtual ICollection<ShipmentStatus> Statuses { get; set; } = new List<ShipmentStatus>();
}
