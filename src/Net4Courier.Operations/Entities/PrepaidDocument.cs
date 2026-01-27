using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class PrepaidDocument : AuditableEntity
{
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    
    public string DocumentNo { get; set; } = string.Empty;
    public DateTime DocumentDate { get; set; }
    
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    
    public int NoOfAWBs { get; set; }
    public decimal CourierCharge { get; set; }
    
    public string AWBNoFrom { get; set; } = string.Empty;
    public string AWBNoTo { get; set; } = string.Empty;
    
    public PrepaidPaymentMode PaymentMode { get; set; }
    public long? BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public long? CashAccountId { get; set; }
    
    public string? ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    
    public decimal TotalPrepaidAmount { get; set; }
    public decimal UsedAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    
    public long? PrepaidControlAccountId { get; set; }
    public long? JournalId { get; set; }
    
    public string? Remarks { get; set; }
    public PrepaidDocumentStatus Status { get; set; } = PrepaidDocumentStatus.Active;
    
    public virtual ICollection<PrepaidAWB> PrepaidAWBs { get; set; } = new List<PrepaidAWB>();
}

public enum PrepaidPaymentMode
{
    Cash = 0,
    Bank = 1,
    Cheque = 2
}

public enum PrepaidDocumentStatus
{
    Active = 0,
    PartiallyUsed = 1,
    FullyUsed = 2,
    Cancelled = 3
}
