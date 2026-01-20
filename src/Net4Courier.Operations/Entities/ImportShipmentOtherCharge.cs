using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class ImportShipmentOtherCharge : BaseEntity
{
    public long ImportShipmentId { get; set; }
    public long OtherChargeTypeId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    
    public virtual ImportShipment? ImportShipment { get; set; }
    public virtual OtherChargeType? OtherChargeType { get; set; }
}
