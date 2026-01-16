using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Operations.Entities;

public class DRS : BaseEntity
{
    public string DRSNo { get; set; } = string.Empty;
    public DateTime DRSDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    public int? DeliveryEmployeeId { get; set; }
    public string? DeliveryEmployeeName { get; set; }
    public int? VehicleId { get; set; }
    public string? VehicleNo { get; set; }
    public int? TotalAWBs { get; set; }
    public int? DeliveredCount { get; set; }
    public int? PendingCount { get; set; }
    public int? ReturnedCount { get; set; }
    public decimal? TotalCOD { get; set; }
    public decimal? CollectedCOD { get; set; }
    public string? Remarks { get; set; }
    public DRSStatus Status { get; set; } = DRSStatus.Open;
    public DateTime? ClosedAt { get; set; }
    public int? ClosedBy { get; set; }
    
    public decimal? TotalCourierCharges { get; set; }
    public decimal? TotalMaterialCost { get; set; }
    public decimal? PickupCash { get; set; }
    public decimal? OutstandingCollected { get; set; }
    public decimal? ExpectedTotal { get; set; }
    public decimal? ActualReceived { get; set; }
    public decimal? ApprovedExpenses { get; set; }
    public decimal? Variance { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public int? ReconciledById { get; set; }
    public string? ReconciledByName { get; set; }
    
    public virtual ICollection<DRSDetail> Details { get; set; } = new List<DRSDetail>();
    public virtual ICollection<CourierCashSubmission> CashSubmissions { get; set; } = new List<CourierCashSubmission>();
    public virtual ICollection<CourierExpense> Expenses { get; set; } = new List<CourierExpense>();
}

public class DRSDetail : BaseEntity
{
    public long DRSId { get; set; }
    public long InscanId { get; set; }
    public int? Sequence { get; set; }
    public int? AttemptNo { get; set; }
    public string? Status { get; set; }
    public string? Remarks { get; set; }
    public DateTime? AttemptedAt { get; set; }
    public decimal? CODAmount { get; set; }
    public decimal? CollectedAmount { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Relation { get; set; }
    
    public virtual DRS DRS { get; set; } = null!;
    public virtual InscanMaster Inscan { get; set; } = null!;
}
