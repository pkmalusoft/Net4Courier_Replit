using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Server.Core.Common;

namespace Server.Modules.Courier.Models;

public enum DRSStatus
{
    Draft,
    Open,
    Dispatched,
    InProgress,
    Completed,
    Reconciled,
    Closed,
    Cancelled
}

public class DeliveryRunSheet : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string DRSNumber { get; set; } = string.Empty;

    [JsonIgnore] // Duplicate of DRSDate - ignored to prevent JSON property collision
    public DateTime DrsDate { get; set; } = DateTime.UtcNow;
    
    public DateTime DRSDate { get; set; } = DateTime.UtcNow;
    
    public Guid? HubId { get; set; }
    
    [MaxLength(200)]
    public string? HubName { get; set; }

    public Guid? AgentId { get; set; }
    public CourierAgent? Agent { get; set; }
    
    public Guid? DriverId { get; set; }
    
    [MaxLength(200)]
    public string? DriverName { get; set; }

    public DRSStatus Status { get; set; } = DRSStatus.Draft;

    public int TotalShipments { get; set; }

    public int DeliveredCount { get; set; }

    public int FailedCount { get; set; }

    public int PendingCount { get; set; }

    public decimal TotalCODToCollect { get; set; }
    
    public decimal TotalCODExpected { get; set; }
    
    public decimal TotalCODCollected { get; set; }

    public decimal CODCollected { get; set; }
    
    public decimal TotalCashDeposited { get; set; }
    
    public decimal DriverExpenses { get; set; }
    
    public decimal ShortageAmount { get; set; }
    
    [MaxLength(50)]
    public string? VehicleNumber { get; set; }
    
    public Guid? RouteZoneId { get; set; }
    public CourierZone? RouteZone { get; set; }
    
    public decimal TotalFreightToCollect { get; set; }
    
    public decimal FreightCollected { get; set; }

    public DateTime? StartTime { get; set; }
    
    public DateTime? DispatchTime { get; set; }
    
    public DateTime? ReturnTime { get; set; }

    public DateTime? EndTime { get; set; }
    
    public bool IsReconciled { get; set; } = false;
    
    [MaxLength(1000)]
    public string? ReconciliationNotes { get; set; }
    
    public DateTime? ReconciledAt { get; set; }
    
    public Guid? ReconciledByUserId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    [MaxLength(500)]
    public string? ExpenseNotes { get; set; }

    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<DRSItem> Items { get; set; } = new List<DRSItem>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}

public enum DRSItemStatus
{
    Pending,
    OutForDelivery,
    Delivered,
    PartialDelivery,
    Failed,
    Undelivered,
    Refused,
    Rescheduled,
    ReturnedToOrigin
}

public class DRSItem : BaseEntity
{
    public Guid DeliveryRunSheetId { get; set; }
    public DeliveryRunSheet DeliveryRunSheet { get; set; } = null!;

    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;
    
    public int SequenceNumber { get; set; }

    public int Sequence { get; set; }

    public DRSItemStatus Status { get; set; } = DRSItemStatus.Pending;
    
    public DateTime? StatusUpdateTime { get; set; }
    
    [MaxLength(200)]
    public string ReceiverName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string ReceiverAddress { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string ReceiverPhone { get; set; } = string.Empty;
    
    public decimal CODAmount { get; set; }
    
    public decimal FreightAmount { get; set; }

    public DateTime? AttemptedAt { get; set; }
    
    public DateTime? DeliveryTime { get; set; }

    public DateTime? DeliveredAt { get; set; }

    [MaxLength(200)]
    public string? ReceivedBy { get; set; }

    [MaxLength(100)]
    public string? Relationship { get; set; }

    public byte[]? SignatureImage { get; set; }

    public byte[]? PODImage { get; set; }
    
    [MaxLength(500)]
    public string? PODImageUrl { get; set; }
    
    [MaxLength(500)]
    public string? SignatureImageUrl { get; set; }

    public decimal? CODCollected { get; set; }
    
    public decimal FreightCollected { get; set; }

    [MaxLength(500)]
    public string? FailureReason { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
    
    public int AttemptNumber { get; set; } = 1;
    
    public DateTime? RescheduledTo { get; set; }
}
