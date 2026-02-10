using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public enum MatchType
{
    Automatic = 0,
    Manual = 1,
    System = 2
}

public class ReconciliationMatch : BaseEntity
{
    public long BankReconciliationId { get; set; }
    public long BankStatementLineId { get; set; }
    public long? JournalId { get; set; }
    public long? CashBankTransactionId { get; set; }
    
    public decimal MatchedAmount { get; set; }
    public MatchType MatchType { get; set; } = MatchType.Manual;
    
    public string? MatchGroup { get; set; }
    
    public DateTime MatchedAt { get; set; } = DateTime.UtcNow;
    public int MatchedByUserId { get; set; }
    
    public string? MatchNotes { get; set; }
    
    public bool IsReversed { get; set; } = false;
    public DateTime? ReversedAt { get; set; }
    public int? ReversedByUserId { get; set; }
    public string? ReversalReason { get; set; }
    
    public virtual BankReconciliation BankReconciliation { get; set; } = null!;
    public virtual BankStatementLine BankStatementLine { get; set; } = null!;
    public virtual Journal? Journal { get; set; }
}
