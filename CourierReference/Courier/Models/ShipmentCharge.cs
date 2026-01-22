using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public class ShipmentCharge : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public Guid? ChargeTypeId { get; set; }
    public CourierChargeType? ChargeType { get; set; }

    [Required]
    [MaxLength(100)]
    public string ChargeName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
