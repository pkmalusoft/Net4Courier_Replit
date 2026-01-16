using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class AWBOtherCharge : BaseEntity
{
    public long InscanId { get; set; }
    public long OtherChargeTypeId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    
    public virtual InscanMaster? Inscan { get; set; }
    public virtual OtherChargeType? OtherChargeType { get; set; }
}
