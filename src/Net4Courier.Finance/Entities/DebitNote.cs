using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public enum DebitNoteStatus
{
    Draft = 1,
    Approved = 2,
    Posted = 3,
    Cancelled = 4
}

public enum DebitNoteReason
{
    AdditionalCharges = 1,
    RateRevision = 2,
    Detention = 3,
    Storage = 4,
    Customs = 5,
    Insurance = 6,
    Other = 99
}

public class DebitNote : AuditableEntity
{
    public string DebitNoteNo { get; set; } = string.Empty;
    public DateTime DebitNoteDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    public long? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public long? BillId { get; set; }
    public string? BillNo { get; set; }
    public DebitNoteReason Reason { get; set; } = DebitNoteReason.Other;
    public string? ReasonDetails { get; set; }
    public decimal Amount { get; set; }
    public decimal? TaxPercent { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public DebitNoteStatus Status { get; set; } = DebitNoteStatus.Draft;
    public string? Remarks { get; set; }
    public long? JournalId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
}
