using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

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
    
    /// <summary>
    /// Optional: Filter rate by service type (e.g., Standard, Express)
    /// </summary>
    public long? ServiceTypeId { get; set; }
    
    /// <summary>
    /// Optional: Filter rate by shipment mode (e.g., Air, Sea, Land)
    /// </summary>
    public long? ShipmentModeId { get; set; }
    
    /// <summary>
    /// Optional: Filter rate by shipment type (Letter, Document, Parcel Upto 30kg, Parcel Above 30kg)
    /// </summary>
    public DocumentType? DocumentType { get; set; }
    
    [System.Obsolete("Use TaxPercent instead")]
    public decimal? MarginPercentage { get; set; }
    [System.Obsolete("Currency is now inherited from Branch")]
    public long? CurrencyId { get; set; }
    [System.Obsolete("Currency is now inherited from Branch")]
    public string? CurrencyCode { get; set; }
    [System.Obsolete("No longer used - use zone-level AdditionalWeight/AdditionalRate instead")]
    public decimal? MinCharge { get; set; }
    [System.Obsolete("No longer used - use zone-level AdditionalWeight/AdditionalRate instead")]
    public decimal? MaxCharge { get; set; }
    
    public decimal MinWeight { get; set; } = 1m;
    public decimal MaxWeight { get; set; } = 5m;
    public decimal TaxPercent { get; set; } = 0m;
    
    /// <summary>
    /// Weight increment for additional rate calculation (default 1kg)
    /// </summary>
    public decimal AdditionalWeight { get; set; } = 1m;
    
    /// <summary>
    /// Rate per AdditionalWeight above BaseWeight
    /// </summary>
    public decimal AdditionalRate { get; set; } = 0m;
    
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
