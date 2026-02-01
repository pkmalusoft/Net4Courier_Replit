using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class CustomerAwbBalance : BaseEntity
{
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    
    public int TotalAwbsPurchased { get; set; }
    public int AwbsUsed { get; set; }
    public int AwbsBalance { get; set; }
    
    public decimal TotalAdvanceAmount { get; set; }
    public decimal UsedAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    
    public DateTime? LastPurchaseDate { get; set; }
    public DateTime? LastUsedDate { get; set; }
}
