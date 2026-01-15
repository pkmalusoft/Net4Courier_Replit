using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Finance.Entities;

public class Invoice : AuditableEntity
{
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerTaxNo { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
    public int? TotalAWBs { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? TaxPercent { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? NetTotal { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? BalanceAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime? DueDate { get; set; }
    public string? Remarks { get; set; }
    public string? Terms { get; set; }
    public long? JournalId { get; set; }
    
    public virtual ICollection<InvoiceDetail> Details { get; set; } = new List<InvoiceDetail>();
}

public class InvoiceDetail : BaseEntity
{
    public long InvoiceId { get; set; }
    public long InscanId { get; set; }
    public string? AWBNo { get; set; }
    public DateTime? AWBDate { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public int? Pieces { get; set; }
    public decimal? Weight { get; set; }
    public decimal? CourierCharge { get; set; }
    public decimal? OtherCharge { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? Total { get; set; }
    
    public virtual Invoice Invoice { get; set; } = null!;
}
