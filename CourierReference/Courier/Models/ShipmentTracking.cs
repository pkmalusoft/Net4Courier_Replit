using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public class ShipmentTracking : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public DateTime EventDateTime { get; set; } = DateTime.UtcNow;

    public ShipmentStatus Status { get; set; }

    [Required]
    [MaxLength(500)]
    public string StatusDescription { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(200)]
    public new string? UpdatedBy { get; set; }

    public Guid? AgentId { get; set; }
    public CourierAgent? Agent { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsPublic { get; set; } = true;
}
