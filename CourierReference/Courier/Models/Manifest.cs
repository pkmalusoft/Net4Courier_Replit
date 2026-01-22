using System.ComponentModel.DataAnnotations;
using Server.Core.Common;

namespace Server.Modules.Courier.Models;

public enum ManifestStatus
{
    Draft,
    Open,
    Dispatched,
    InTransit,
    Received,
    Closed,
    Cancelled
}

public enum ManifestType
{
    Outbound,
    Inbound,
    Transfer
}

public class Manifest : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string ManifestNumber { get; set; } = string.Empty;
    
    public ManifestType Type { get; set; } = ManifestType.Outbound;
    
    public DateTime ManifestDate { get; set; } = DateTime.UtcNow;
    
    public Guid OriginHubId { get; set; }
    
    [MaxLength(100)]
    public string OriginHubName { get; set; } = string.Empty;
    
    public Guid DestinationHubId { get; set; }
    
    [MaxLength(100)]
    public string DestinationHubName { get; set; } = string.Empty;
    
    public ManifestStatus Status { get; set; } = ManifestStatus.Draft;
    
    [MaxLength(50)]
    public string? SealNumber { get; set; }
    
    [MaxLength(50)]
    public string? VehicleNumber { get; set; }
    
    [MaxLength(100)]
    public string? DriverName { get; set; }
    
    [MaxLength(20)]
    public string? DriverPhone { get; set; }
    
    [MaxLength(100)]
    public string? CoLoaderName { get; set; }
    
    public Guid? CoLoaderVendorId { get; set; }
    
    public int TotalShipments { get; set; }
    
    public decimal TotalWeight { get; set; }
    
    public int TotalPieces { get; set; }
    
    public decimal TotalCODAmount { get; set; }
    
    public decimal TotalDeclaredValue { get; set; }
    
    public DateTime? DispatchedAt { get; set; }
    
    public Guid? DispatchedByUserId { get; set; }
    
    public DateTime? ExpectedArrival { get; set; }
    
    public DateTime? ReceivedAt { get; set; }
    
    public Guid? ReceivedByUserId { get; set; }
    
    public int? ShipmentsReceived { get; set; }
    
    public int? ShortShipments { get; set; }
    
    public int? DamagedShipments { get; set; }
    
    [MaxLength(1000)]
    public string? DispatchNotes { get; set; }
    
    [MaxLength(1000)]
    public string? ReceiptNotes { get; set; }
    
    [MaxLength(500)]
    public string? DiscrepancyNotes { get; set; }
    
    [MaxLength(1000)]
    public string? ReceiveRemarks { get; set; }
    
    public int ActualPiecesReceived { get; set; }
    
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    
    public ICollection<ManifestItem> Items { get; set; } = new List<ManifestItem>();
    
    public ICollection<Shipment> InboundShipments { get; set; } = new List<Shipment>();
    
    public ICollection<Shipment> OutboundShipments { get; set; } = new List<Shipment>();
}

public class ManifestItem : BaseEntity
{
    public Guid ManifestId { get; set; }
    public Manifest Manifest { get; set; } = null!;
    
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;
    
    public int Sequence { get; set; }
    
    [MaxLength(50)]
    public string AWBNumber { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? DestinationCity { get; set; }
    
    public int Pieces { get; set; } = 1;
    
    public decimal Weight { get; set; }
    
    public decimal CODAmount { get; set; }
    
    public bool IsCOD { get; set; } = false;
    
    public DateTime? ScanTime { get; set; }
    
    public Guid? ScannedByUserId { get; set; }
    
    public bool IsReceived { get; set; } = false;
    
    public DateTime? ReceivedAt { get; set; }
    
    public DateTime? ReceiveTime { get; set; }
    
    public Guid? ReceivedByUserId { get; set; }
    
    public bool HasDiscrepancy { get; set; } = false;
    
    [MaxLength(500)]
    public string? DiscrepancyNotes { get; set; }
}
