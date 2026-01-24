using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public enum ReconciliationAdjustmentType
{
    BankFee = 0,
    BankInterest = 1,
    BankCharge = 2,
    UnrecordedDeposit = 3,
    UnrecordedWithdrawal = 4,
    Error = 5,
    Other = 6
}

public class ReconciliationAdjustment : AuditableEntity
{
    public long BankReconciliationId { get; set; }
    public long? BankStatementLineId { get; set; }
    
    public ReconciliationAdjustmentType AdjustmentType { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    
    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow.Date;
    
    public long? JournalId { get; set; }
    
    public bool IsPosted { get; set; } = false;
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    
    public string? Notes { get; set; }
    
    public virtual BankReconciliation BankReconciliation { get; set; } = null!;
    public virtual BankStatementLine? BankStatementLine { get; set; }
    public virtual Journal? Journal { get; set; }
}
