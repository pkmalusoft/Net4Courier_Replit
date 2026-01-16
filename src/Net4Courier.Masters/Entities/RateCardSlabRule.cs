using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class RateCardSlabRule : AuditableEntity
{
    public long RateCardZoneId { get; set; }
    public decimal FromWeight { get; set; }
    public decimal ToWeight { get; set; }
    public decimal IncrementWeight { get; set; }
    public decimal IncrementRate { get; set; }
    public SlabCalculationMode CalculationMode { get; set; } = SlabCalculationMode.PerStep;
    public int SortOrder { get; set; }
    
    public virtual RateCardZone? RateCardZone { get; set; }
}

public enum SlabCalculationMode
{
    PerStep = 1,
    PerKg = 2,
    FlatAfter = 3
}
