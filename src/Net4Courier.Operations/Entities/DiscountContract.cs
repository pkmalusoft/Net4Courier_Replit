using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class DiscountContract : BaseEntity
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? PartyId { get; set; }
    public string? PartyName { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumDiscount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool ApplyToDomestic { get; set; } = true;
    public bool ApplyToInternational { get; set; } = true;
    public long? ServiceTypeId { get; set; }
    public string? ServiceTypeName { get; set; }
    public string? Notes { get; set; }
    public new bool IsActive { get; set; } = true;
}
