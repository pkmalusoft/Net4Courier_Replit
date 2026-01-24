using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class BankStatementLine : BaseEntity
{
    public long BankStatementImportId { get; set; }
    
    public DateTime TransactionDate { get; set; }
    public DateTime? ValueDate { get; set; }
    
    public string? Description { get; set; }
    public string? ChequeNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal Balance { get; set; }
    
    public decimal NetAmount => CreditAmount - DebitAmount;
    
    public bool IsMatched { get; set; } = false;
    public bool IsAdjustment { get; set; } = false;
    
    public string? MatchNotes { get; set; }
    public string? LineHash { get; set; }
    
    public virtual BankStatementImport BankStatementImport { get; set; } = null!;
    public virtual ICollection<ReconciliationMatch> Matches { get; set; } = new List<ReconciliationMatch>();
}
