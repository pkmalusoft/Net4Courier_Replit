using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class TransactionPageType : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresAutoPosting { get; set; }
    public int SortOrder { get; set; }
    public long? CompanyId { get; set; }
}
