using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class Receipt : AuditableEntity
{
    public string ReceiptNo { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentMode { get; set; }
    public string? BankName { get; set; }
    public string? ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string? TransactionRef { get; set; }
    public string? Remarks { get; set; }
    public long? JournalId { get; set; }
    public bool IsAllocated { get; set; }
    
    public virtual ICollection<ReceiptAllocation> Allocations { get; set; } = new List<ReceiptAllocation>();
}

public class ReceiptAllocation : BaseEntity
{
    public long ReceiptId { get; set; }
    public long InvoiceId { get; set; }
    public decimal? AllocatedAmount { get; set; }
    public string? Remarks { get; set; }
    
    public virtual Receipt Receipt { get; set; } = null!;
}
