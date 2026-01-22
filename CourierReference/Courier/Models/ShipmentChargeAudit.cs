using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public enum ChargeAuditAction
{
    FreightChargeModified,
    SurchargeModified,
    DiscountModified,
    TotalChargesRecalculated,
    ManualAdjustment
}

public class ShipmentChargeAudit : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string AWBNumber { get; set; } = string.Empty;

    [Required]
    public ChargeAuditAction Action { get; set; }

    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    public decimal OldValue { get; set; }

    public decimal NewValue { get; set; }

    [MaxLength(500)]
    public string? ChangeReason { get; set; }

    public Guid ModifiedByUserId { get; set; }

    [MaxLength(200)]
    public string? ModifiedByUserName { get; set; }

    [MaxLength(200)]
    public string? ModifiedByUserEmail { get; set; }

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? IpAddress { get; set; }
}
