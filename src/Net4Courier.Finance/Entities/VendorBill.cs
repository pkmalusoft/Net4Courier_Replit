using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public enum VendorBillStatus
{
    Draft = 1,
    Approved = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Cancelled = 5
}

public class VendorBill : AuditableEntity
{
    public string BillNo { get; set; } = string.Empty;
    public string? VendorBillNo { get; set; }
    public DateTime BillDate { get; set; }
    public DateTime? DueDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public string? BranchName { get; set; }
    public long? FinancialYearId { get; set; }
    public long? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? Description { get; set; }
    public decimal SubTotal { get; set; }
    public decimal? TaxPercent { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public VendorBillStatus Status { get; set; } = VendorBillStatus.Draft;
    public string? Remarks { get; set; }
    public long? JournalId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
}
