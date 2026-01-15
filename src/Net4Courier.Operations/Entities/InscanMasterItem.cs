using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class InscanMasterItem : BaseEntity
{
    public long InscanId { get; set; }
    public string? BoxName { get; set; }
    public string? Contents { get; set; }
    public int? Quantity { get; set; }
    public decimal? Value { get; set; }
    public decimal? WeightPerItem { get; set; }
    public decimal? TotalWeight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    
    public virtual InscanMaster Inscan { get; set; } = null!;
}
