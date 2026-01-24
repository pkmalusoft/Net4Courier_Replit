using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class TransferOrder : AuditableEntity
{
    public string TransferNo { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public long? CompanyId { get; set; }
    public long? FinancialYearId { get; set; }
    public long SourceBranchId { get; set; }
    public string? SourceBranchName { get; set; }
    public long DestinationBranchId { get; set; }
    public string? DestinationBranchName { get; set; }
    public long? SourceWarehouseId { get; set; }
    public string? SourceWarehouseName { get; set; }
    public long? DestinationWarehouseId { get; set; }
    public string? DestinationWarehouseName { get; set; }
    public TransferOrderType TransferType { get; set; } = TransferOrderType.Standard;
    public TransferOrderStatus Status { get; set; } = TransferOrderStatus.Draft;
    public int TotalItems { get; set; }
    public int TotalPieces { get; set; }
    public decimal TotalWeight { get; set; }
    public long? VehicleId { get; set; }
    public string? VehicleNo { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? SealNo { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public long? DispatchedByUserId { get; set; }
    public string? DispatchedByUserName { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public long? ReceivedByUserId { get; set; }
    public string? ReceivedByUserName { get; set; }
    public DateTime? ExpectedArrival { get; set; }
    public string? Remarks { get; set; }
    public string? DispatchRemarks { get; set; }
    public string? ReceiptRemarks { get; set; }
    public int? ReceivedCount { get; set; }
    public int? ShortCount { get; set; }
    public int? DamagedCount { get; set; }
    public int? ExcessCount { get; set; }
    public virtual ICollection<TransferOrderItem> Items { get; set; } = new List<TransferOrderItem>();
    public virtual ICollection<TransferOrderEvent> Events { get; set; } = new List<TransferOrderEvent>();
}

public class TransferOrderItem : BaseEntity
{
    public long TransferOrderId { get; set; }
    public long? InscanMasterId { get; set; }
    public string? AWBNo { get; set; }
    public string? Description { get; set; }
    public int Pieces { get; set; } = 1;
    public decimal Weight { get; set; }
    public string? Dimensions { get; set; }
    public TransferItemStatus Status { get; set; } = TransferItemStatus.Pending;
    public DateTime? ScannedAt { get; set; }
    public long? ScannedByUserId { get; set; }
    public string? ScannedByUserName { get; set; }
    public DateTime? LoadedAt { get; set; }
    public DateTime? UnloadedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? ReceivedRemarks { get; set; }
    public bool IsShort { get; set; }
    public bool IsDamaged { get; set; }
    public bool IsExcess { get; set; }
    public string? DamageDescription { get; set; }
    public string? Remarks { get; set; }
    public virtual TransferOrder TransferOrder { get; set; } = null!;
    public virtual InscanMaster? InscanMaster { get; set; }
}

public class TransferOrderEvent : BaseEntity
{
    public long TransferOrderId { get; set; }
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public long? BranchId { get; set; }
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public virtual TransferOrder TransferOrder { get; set; } = null!;
}

public enum TransferOrderType
{
    Standard = 1,
    Emergency = 2,
    Return = 3,
    Consolidation = 4
}

public enum TransferOrderStatus
{
    Draft = 1,
    Confirmed = 2,
    Loading = 3,
    InTransit = 4,
    Arrived = 5,
    Unloading = 6,
    Received = 7,
    Completed = 8,
    Cancelled = 9
}

public enum TransferItemStatus
{
    Pending = 1,
    Scanned = 2,
    Loaded = 3,
    InTransit = 4,
    Unloaded = 5,
    Received = 6,
    Short = 7,
    Damaged = 8,
    Excess = 9
}
