using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class CustomerAwbIssue : AuditableEntity
{
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    
    public string IssueNo { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    
    public AwbIssueType IssueType { get; set; }
    
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public bool DestinationLocked { get; set; }
    
    public int NoOfAWBs { get; set; }
    public decimal RatePerAWB { get; set; }
    public decimal TotalAmount { get; set; }
    
    public AwbIssuePaymentMode? PaymentMode { get; set; }
    public long? CashAccountId { get; set; }
    public string? CashAccountName { get; set; }
    public long? BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankReferenceNo { get; set; }
    public DateTime? PaymentDate { get; set; }
    
    public long? JournalId { get; set; }
    public long? PrepaidAdvanceAccountId { get; set; }
    
    public string? Remarks { get; set; }
    public AwbIssueStatus Status { get; set; } = AwbIssueStatus.Issued;
    
    public virtual ICollection<CustomerAwbIssueDetail> Details { get; set; } = new List<CustomerAwbIssueDetail>();
}

public class CustomerAwbIssueDetail : BaseEntity
{
    public long CustomerAwbIssueId { get; set; }
    
    public string AWBNo { get; set; } = string.Empty;
    public AwbDetailStatus Status { get; set; } = AwbDetailStatus.Issued;
    
    public DateTime? UsedDate { get; set; }
    public long? InscanMasterId { get; set; }
    
    public virtual CustomerAwbIssue CustomerAwbIssue { get; set; } = null!;
}

public enum AwbIssueType
{
    Prepaid = 1,
    NonPrepaid = 2
}

public enum AwbIssuePaymentMode
{
    Cash = 1,
    Bank = 2
}

public enum AwbIssueStatus
{
    Issued = 1,
    PartiallyUsed = 2,
    FullyUsed = 3,
    Cancelled = 4
}

public enum AwbDetailStatus
{
    Issued = 1,
    Used = 2,
    Cancelled = 3
}
