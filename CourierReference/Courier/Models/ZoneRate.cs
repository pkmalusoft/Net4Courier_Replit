using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public enum RateType
{
    PerKg,
    FlatRate,
    PerPiece,
    Slab
}

public class ZoneRate : BaseEntity
{
    [MaxLength(100)]
    public string? RateName { get; set; }

    public Guid CourierZoneId { get; set; }
    public CourierZone CourierZone { get; set; } = null!;

    public Guid CourierServiceTypeId { get; set; }
    public CourierServiceType CourierServiceType { get; set; } = null!;

    public RateType RateType { get; set; } = RateType.PerKg;

    public decimal MinWeight { get; set; } = 0;

    public decimal MaxWeight { get; set; } = 999999;

    public decimal BaseRate { get; set; }

    public decimal AdditionalRatePerKg { get; set; }

    public decimal MinCharge { get; set; }

    public decimal FuelSurchargePercent { get; set; } = 0;
    
    public decimal InsurancePercent { get; set; } = 0;

    public decimal CODChargePercent { get; set; } = 0;

    public decimal CODMinCharge { get; set; } = 0;

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;
}
