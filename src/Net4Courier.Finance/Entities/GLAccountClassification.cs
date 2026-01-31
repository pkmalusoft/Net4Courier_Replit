using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class GLAccountClassification : BaseEntity
{
    public long? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public virtual ICollection<GLChartOfAccount> ChartOfAccounts { get; set; } = new List<GLChartOfAccount>();
}
