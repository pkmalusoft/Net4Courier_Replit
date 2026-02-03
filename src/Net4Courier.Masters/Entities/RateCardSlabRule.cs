using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class RateCardSlabRule : AuditableEntity
{
    public long RateCardZoneId { get; set; }
    public decimal FromWeight { get; set; }
    public decimal ToWeight { get; set; }
    public decimal IncrementWeight { get; set; }
    public decimal IncrementRate { get; set; }
    public SlabCalculationMode CalculationMode { get; set; } = SlabCalculationMode.PerKg;
    public decimal? FlatRate { get; set; }
    public decimal? CostFlatRate { get; set; }
    public decimal? CostPerKgRate { get; set; }
    public decimal? Additional1KgRate { get; set; }
    public int SortOrder { get; set; }
    
    public virtual RateCardZone? RateCardZone { get; set; }
}

public enum SlabCalculationMode
{
    PerStep = 1,
    PerKg = 2,
    FlatAfter = 3,
    FlatForSlab = 4,
    FlatPlusAdditional = 5
}
