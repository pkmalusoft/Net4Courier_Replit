using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class CODRemittance : AuditableEntity
{
    public string RemittanceNo { get; set; } = string.Empty;
    public DateTime RemittanceDate { get; set; } = DateTime.UtcNow;
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerCode { get; set; }
    public decimal TotalCODAmount { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal ServiceChargePercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetPayable { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string? PaymentMode { get; set; }
    public string? PaymentReference { get; set; }
    public string? BankName { get; set; }
    public string? ChequeNo { get; set; }
    public DateTime? ChequeDate { get; set; }
    public string? TransactionId { get; set; }
    public CODRemittanceStatus Status { get; set; } = CODRemittanceStatus.Draft;
    public DateTime? ApprovedAt { get; set; }
    public long? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? PaidAt { get; set; }
    public long? PaidByUserId { get; set; }
    public string? PaidByUserName { get; set; }
    public string? Remarks { get; set; }
    public long? JournalId { get; set; }
    public virtual ICollection<CODRemittanceDetail> Details { get; set; } = new List<CODRemittanceDetail>();
}

public class CODRemittanceDetail : BaseEntity
{
    public long CODRemittanceId { get; set; }
    public long InscanMasterId { get; set; }
    public string AWBNo { get; set; } = string.Empty;
    public DateTime? DeliveredDate { get; set; }
    public string? ConsigneeName { get; set; }
    public decimal CODAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal NetPayable { get; set; }
    public string? Remarks { get; set; }
    public virtual CODRemittance CODRemittance { get; set; } = null!;
    public virtual InscanMaster InscanMaster { get; set; } = null!;
}

public enum CODRemittanceStatus
{
    Draft = 1,
    Pending = 2,
    Approved = 3,
    Paid = 4,
    PartiallyPaid = 5,
    Cancelled = 6
}
