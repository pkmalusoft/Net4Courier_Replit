using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class VendorPayment : AuditableEntity
{
    public string PaymentNo { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    public int? PartyType { get; set; }
    public long? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentMode { get; set; }
    public string? BankName { get; set; }
    public string? ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string? TransactionRef { get; set; }
    public string? Remarks { get; set; }
    public long? JournalId { get; set; }
    public bool IsAllocated { get; set; }

    public virtual ICollection<VendorPaymentAllocation> Allocations { get; set; } = new List<VendorPaymentAllocation>();
}

public class VendorPaymentAllocation : BaseEntity
{
    public long VendorPaymentId { get; set; }
    public long VendorBillId { get; set; }
    public decimal? AllocatedAmount { get; set; }
    public string? Remarks { get; set; }

    public virtual VendorPayment VendorPayment { get; set; } = null!;
}
