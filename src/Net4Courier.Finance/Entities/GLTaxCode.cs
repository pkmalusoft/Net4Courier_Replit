using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class GLTaxCode : BaseEntity
{
    public long? CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Rate { get; set; } = 0;
    public string? TaxType { get; set; }
    public string? CreatedByUser { get; set; }
    public string? UpdatedByUser { get; set; }
}
