using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class FuelSurcharge : BaseEntity
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool ApplyToDomestic { get; set; } = true;
    public bool ApplyToInternational { get; set; } = true;
    public string? Notes { get; set; }
    public new bool IsActive { get; set; } = true;
}
