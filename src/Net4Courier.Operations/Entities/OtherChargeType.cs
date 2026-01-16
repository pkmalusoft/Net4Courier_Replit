using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class OtherChargeType : BaseEntity
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultAmount { get; set; }
    public bool IsPercentage { get; set; }
    public int SortOrder { get; set; }
}
