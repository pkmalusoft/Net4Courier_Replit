using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public enum CreditNoteStatus
{
    Draft = 1,
    Approved = 2,
    Posted = 3,
    Cancelled = 4
}

public enum CreditNoteReason
{
    ServiceIssue = 1,
    Overcharge = 2,
    Damage = 3,
    LostShipment = 4,
    Delay = 5,
    Discount = 6,
    Other = 99
}

public class CreditNote : AuditableEntity
{
    public string CreditNoteNo { get; set; } = string.Empty;
    public DateTime CreditNoteDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public long? InvoiceId { get; set; }
    public string? InvoiceNo { get; set; }
    public CreditNoteReason Reason { get; set; } = CreditNoteReason.Other;
    public string? ReasonDetails { get; set; }
    public decimal Amount { get; set; }
    public decimal? TaxPercent { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public CreditNoteStatus Status { get; set; } = CreditNoteStatus.Draft;
    public string? Remarks { get; set; }
    public long? JournalId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
}
