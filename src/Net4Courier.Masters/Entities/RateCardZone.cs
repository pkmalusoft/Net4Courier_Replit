using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class RateCardZone : AuditableEntity
{
    public long RateCardId { get; set; }
    public long ZoneMatrixId { get; set; }
    public decimal BaseWeight { get; set; }
    
    public decimal CostBaseRate { get; set; }
    public decimal CostPerKg { get; set; }
    public decimal SalesBaseRate { get; set; }
    public decimal SalesPerKg { get; set; }
    
    [System.Obsolete("Use SalesBaseRate instead")]
    public decimal BaseRate { get; set; }
    
    public TaxMode TaxMode { get; set; } = TaxMode.Exclusive;
    public long? ForwardingAgentId { get; set; }
    
    [System.Obsolete("Use TaxPercent instead")]
    public decimal? MarginPercentage { get; set; }
    [System.Obsolete("Currency is now inherited from Branch")]
    public long? CurrencyId { get; set; }
    [System.Obsolete("Currency is now inherited from Branch")]
    public string? CurrencyCode { get; set; }
    public decimal? MinCharge { get; set; }
    public decimal? MaxCharge { get; set; }
    
    public decimal MinWeight { get; set; } = 1m;
    public decimal MaxWeight { get; set; } = 5m;
    public decimal TaxPercent { get; set; } = 0m;
    
    public decimal MarginBaseRate => SalesBaseRate - CostBaseRate;
    public decimal MarginPerKg => SalesPerKg - CostPerKg;
    
    public virtual RateCard? RateCard { get; set; }
    public virtual ZoneMatrix? ZoneMatrix { get; set; }
    public virtual ICollection<RateCardSlabRule> SlabRules { get; set; } = new List<RateCardSlabRule>();
}

public enum TaxMode
{
    Exclusive = 1,
    Inclusive = 2
}
