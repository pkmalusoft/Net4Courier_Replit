using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Operations.Entities;

public class CourierLedger : AuditableEntity
{
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public int CourierId { get; set; }
    public string? CourierName { get; set; }
    public DateTime TransactionDate { get; set; }
    public LedgerEntryType EntryType { get; set; }
    public long? DRSId { get; set; }
    public string? DRSNo { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RunningBalance { get; set; }
    public string? Narration { get; set; }
    public string? Reference { get; set; }
    public bool IsSettled { get; set; }
    public DateTime? SettledAt { get; set; }
    public string? SettlementRef { get; set; }
    
    public virtual DRS? DRS { get; set; }
}
