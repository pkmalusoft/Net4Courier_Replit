using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class AWBStock : AuditableEntity
{
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public DateTime StockDate { get; set; }
    public string? ReferenceNo { get; set; }
    
    public string ItemName { get; set; } = string.Empty;
    public string? ItemType { get; set; }
    
    public int Qty { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    
    public int AWBCount { get; set; }
    public string AWBNoFrom { get; set; } = string.Empty;
    public string AWBNoTo { get; set; } = string.Empty;
    
    public int AllocatedCount { get; set; }
    public int AvailableCount { get; set; }
    
    public StockStatus Status { get; set; } = StockStatus.Available;
    public string? Remarks { get; set; }
}

public enum StockStatus
{
    Available = 0,
    PartiallyAllocated = 1,
    FullyAllocated = 2,
    Cancelled = 3
}
