using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class RateCardZone : AuditableEntity
{
    public long RateCardId { get; set; }
    public long ZoneMatrixId { get; set; }
    public decimal BaseWeight { get; set; }
    public decimal BaseRate { get; set; }
    public TaxMode TaxMode { get; set; } = TaxMode.Exclusive;
    public long? ForwardingAgentId { get; set; }
    public decimal? MarginPercentage { get; set; }
    public long? CurrencyId { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? MinCharge { get; set; }
    public decimal? MaxCharge { get; set; }
    
    public virtual RateCard? RateCard { get; set; }
    public virtual ZoneMatrix? ZoneMatrix { get; set; }
    public virtual ICollection<RateCardSlabRule> SlabRules { get; set; } = new List<RateCardSlabRule>();
}

public enum TaxMode
{
    Exclusive = 1,
    Inclusive = 2,
    WithTax = 3
}
