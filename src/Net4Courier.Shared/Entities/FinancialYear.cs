namespace Net4Courier.Shared.Entities;

public class FinancialYear
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
}
