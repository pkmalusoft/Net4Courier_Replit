using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class FinancialYear : BaseEntity
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; } = false;
    public bool IsClosed { get; set; } = false;
    public DateTime? ClosedAt { get; set; }
    public int? ClosedBy { get; set; }
    
    public virtual Company Company { get; set; } = null!;
}
