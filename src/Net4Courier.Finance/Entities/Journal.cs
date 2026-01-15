using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class Journal : AuditableEntity
{
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    public string? VoucherType { get; set; }
    public string? Narration { get; set; }
    public string? Reference { get; set; }
    public decimal? TotalDebit { get; set; }
    public decimal? TotalCredit { get; set; }
    public bool IsPosted { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedBy { get; set; }
    
    public virtual ICollection<JournalEntry> Entries { get; set; } = new List<JournalEntry>();
}

public class JournalEntry : BaseEntity
{
    public long JournalId { get; set; }
    public long AccountHeadId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public decimal? Debit { get; set; }
    public decimal? Credit { get; set; }
    public string? Narration { get; set; }
    public long? PartyId { get; set; }
    public string? CostCentre { get; set; }
    
    public virtual Journal Journal { get; set; } = null!;
}
