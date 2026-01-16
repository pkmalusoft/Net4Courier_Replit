using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class FinancialPeriod : BaseEntity
{
    public long FinancialYearId { get; set; }
    public int PeriodNumber { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PeriodStatus Status { get; set; } = PeriodStatus.Open;
    public DateTime? ClosedAt { get; set; }
    public long? ClosedBy { get; set; }
    public DateTime? ReopenedAt { get; set; }
    public long? ReopenedBy { get; set; }
    
    public virtual FinancialYear? FinancialYear { get; set; }
}

public enum PeriodStatus
{
    Open = 1,
    Closed = 2
}
