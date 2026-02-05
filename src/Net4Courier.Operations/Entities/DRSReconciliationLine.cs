using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public enum ReconciliationLineStatus
{
    Pending = 1,
    Reconciled = 2,
    DiscountPending = 3,
    DiscountApproved = 4,
    DiscountRejected = 5
}

public class DRSReconciliationLine : BaseEntity
{
    public long DRSId { get; set; }
    public long CashSubmissionId { get; set; }
    public long InscanId { get; set; }
    public string? AWBNo { get; set; }
    
    public decimal MaterialCost { get; set; }
    public decimal CODAmount { get; set; }
    public decimal OtherCharges { get; set; }
    public decimal TotalCollectible { get; set; }
    
    public decimal AmountCollected { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? DiscountReason { get; set; }
    public bool DiscountApproved { get; set; }
    public int? DiscountApprovedById { get; set; }
    public string? DiscountApprovedByName { get; set; }
    public DateTime? DiscountApprovedAt { get; set; }
    
    public ReconciliationLineStatus Status { get; set; } = ReconciliationLineStatus.Pending;
    public string? Remarks { get; set; }
    
    public virtual DRS DRS { get; set; } = null!;
    public virtual CourierCashSubmission CashSubmission { get; set; } = null!;
    public virtual InscanMaster Inscan { get; set; } = null!;
}

public class DRSReconciliationStatement : BaseEntity
{
    public long DRSId { get; set; }
    public string? DRSNo { get; set; }
    public long CashSubmissionId { get; set; }
    public string? ReceiptNo { get; set; }
    public DateTime StatementDate { get; set; }
    
    public int CourierId { get; set; }
    public string? CourierName { get; set; }
    
    public decimal TotalCollectible { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal CashSubmitted { get; set; }
    public decimal ExpenseBills { get; set; }
    public decimal Balance { get; set; }
    
    public bool IsSettled { get; set; }
    public DateTime? SettledAt { get; set; }
    public int? SettledById { get; set; }
    public string? SettledByName { get; set; }
    
    public string? Remarks { get; set; }
    
    public virtual DRS DRS { get; set; } = null!;
    public virtual CourierCashSubmission CashSubmission { get; set; } = null!;
    public virtual ICollection<DRSReconciliationLine> Lines { get; set; } = new List<DRSReconciliationLine>();
}
