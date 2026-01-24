using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class TaxRate : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Rate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public long? AccountHeadId { get; set; }
    public virtual AccountHead? AccountHead { get; set; }
}
