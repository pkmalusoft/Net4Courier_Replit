using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class PrepaidAWB : BaseEntity
{
    public long PrepaidDocumentId { get; set; }
    public long? AWBStockId { get; set; }
    
    public string AWBNo { get; set; } = string.Empty;
    public string? Consignor { get; set; }
    public string? Consignee { get; set; }
    
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    
    public bool IsUsed { get; set; }
    public DateTime? UsedDate { get; set; }
    public long? InscanMasterId { get; set; }
    public long? UsageJournalId { get; set; }
    
    public virtual PrepaidDocument PrepaidDocument { get; set; } = null!;
}
