using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public enum ReconciliationStatus
{
    Draft = 0,
    InProgress = 1,
    Completed = 2,
    Locked = 3
}

public class BankReconciliation : AuditableEntity
{
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long BankAccountId { get; set; }
    
    public string ReconciliationNumber { get; set; } = string.Empty;
    public DateTime StatementDate { get; set; }
    
    public decimal StatementOpeningBalance { get; set; }
    public decimal StatementClosingBalance { get; set; }
    
    public decimal BookOpeningBalance { get; set; }
    public decimal BookClosingBalance { get; set; }
    
    public decimal DifferenceAmount { get; set; }
    
    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.Draft;
    
    public DateTime? CompletedDate { get; set; }
    public int? CompletedByUserId { get; set; }
    
    public DateTime? LockedAt { get; set; }
    public int? LockedByUserId { get; set; }
    
    public string? Notes { get; set; }
    
    public virtual AccountHead BankAccount { get; set; } = null!;
    public virtual ICollection<BankStatementImport> StatementImports { get; set; } = new List<BankStatementImport>();
    public virtual ICollection<ReconciliationMatch> Matches { get; set; } = new List<ReconciliationMatch>();
    public virtual ICollection<ReconciliationAdjustment> Adjustments { get; set; } = new List<ReconciliationAdjustment>();
}
