using System.ComponentModel.DataAnnotations;
using Server.Data;

namespace Server.Modules.Courier.Models;

public enum TransferType
{
    DeliveryOutscan,
    WarehouseTransfer,
    CourierReassignment
}

public enum TransferStatus
{
    Draft,
    InProgress,
    HandoverPending,
    Completed,
    Rejected,
    Cancelled
}

public class TransferOrder : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string TransferNumber { get; set; } = string.Empty;

    public TransferType TransferType { get; set; } = TransferType.DeliveryOutscan;

    public TransferStatus Status { get; set; } = TransferStatus.Draft;

    public Guid? SourceWarehouseId { get; set; }
    
    public Guid? DestinationWarehouseId { get; set; }

    public Guid? SourceCourierId { get; set; }

    public Guid? DestinationCourierId { get; set; }

    public Guid? VehicleId { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? ExecutedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public int TotalItems { get; set; } = 0;

    public int ScannedItems { get; set; } = 0;

    public int ExceptionItems { get; set; } = 0;

    public ICollection<TransferOrderItem> Items { get; set; } = new List<TransferOrderItem>();

    public ICollection<TransferScanEvent> ScanEvents { get; set; } = new List<TransferScanEvent>();
}

public enum TransferItemStatus
{
    Pending,
    Scanned,
    Loaded,
    InTransit,
    Received,
    Exception,
    Returned
}

public class TransferOrderItem : BaseEntity
{
    public Guid TransferOrderId { get; set; }
    public TransferOrder TransferOrder { get; set; } = null!;

    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    [MaxLength(50)]
    public string AWBNumber { get; set; } = string.Empty;

    public TransferItemStatus Status { get; set; } = TransferItemStatus.Pending;

    public DateTime? ScannedAt { get; set; }

    public DateTime? LoadedAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    [MaxLength(500)]
    public string? ExceptionReason { get; set; }

    [MaxLength(200)]
    public string? ScannedBy { get; set; }

    [MaxLength(200)]
    public string? ReceivedBy { get; set; }
}

public enum ScanEventType
{
    OutscanStart,
    ItemScanned,
    LoadVehicle,
    DepartWarehouse,
    ArriveWarehouse,
    CourierHandover,
    CourierReceive,
    ItemReceived,
    Exception,
    TransferComplete
}

public class TransferScanEvent : BaseEntity
{
    public Guid TransferOrderId { get; set; }
    public TransferOrder TransferOrder { get; set; } = null!;

    public Guid? TransferOrderItemId { get; set; }
    public TransferOrderItem? TransferOrderItem { get; set; }

    public Guid? ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    [MaxLength(50)]
    public string? AWBNumber { get; set; }

    public ScanEventType ScanType { get; set; }

    public DateTime ScanTimestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? ScannedByEmployeeId { get; set; }

    [MaxLength(200)]
    public string? ScannedByName { get; set; }

    public Guid? ScanLocationId { get; set; }

    [MaxLength(200)]
    public string? ScanLocationName { get; set; }

    [MaxLength(100)]
    public string? DeviceId { get; set; }

    [MaxLength(100)]
    public string? DeviceName { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsException { get; set; } = false;

    [MaxLength(500)]
    public string? ExceptionDetails { get; set; }
}
