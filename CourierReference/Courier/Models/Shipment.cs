using System.ComponentModel.DataAnnotations;
using Server.Core.Common;
using Server.Modules.Empost.Models;

namespace Server.Modules.Courier.Models;

public enum ShipmentStatus
{
    Draft,
    Booked,
    PickedUp,
    InScan,
    InTransit,
    OutForDelivery,
    Delivered,
    PartialDelivery,
    ReturnedToOrigin,
    Cancelled,
    OnHold
}

public enum PaymentMode
{
    Prepaid,
    COD,
    Credit,
    ToPayDestination
}

public enum ContentType
{
    Documents,
    Parcel,
    Fragile,
    Perishable,
    Electronics,
    Clothing,
    Other
}

public enum ShipmentClassificationType
{
    Letter,
    Document,
    ParcelUpto30kg,
    ParcelAbove30kg
}

public enum ShipmentMode
{
    Domestic,
    Export,
    Import,
    Transhipment
}

public class Shipment : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string AWBNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ReferenceNumber { get; set; }

    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    public Guid CourierServiceTypeId { get; set; }
    public CourierServiceType CourierServiceType { get; set; } = null!;

    public Guid? OriginZoneId { get; set; }
    public CourierZone? OriginZone { get; set; }

    public Guid? DestinationZoneId { get; set; }
    public CourierZone? DestinationZone { get; set; }

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [Required]
    [MaxLength(200)]
    public string SenderName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? SenderAddress { get; set; }

    [MaxLength(100)]
    public string? SenderCity { get; set; }

    [MaxLength(100)]
    public string? SenderState { get; set; }

    [MaxLength(20)]
    public string? SenderPostalCode { get; set; }

    [MaxLength(100)]
    public string? SenderCountry { get; set; }

    [MaxLength(20)]
    public string? SenderPhone { get; set; }

    [MaxLength(100)]
    public string? SenderEmail { get; set; }

    [MaxLength(200)]
    public string? SenderCompany { get; set; }

    [Required]
    [MaxLength(200)]
    public string ReceiverName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ReceiverAddress { get; set; }

    [MaxLength(100)]
    public string? ReceiverCity { get; set; }

    [MaxLength(100)]
    public string? ReceiverState { get; set; }

    [MaxLength(20)]
    public string? ReceiverPostalCode { get; set; }

    [MaxLength(100)]
    public string? ReceiverCountry { get; set; }

    [MaxLength(20)]
    public string? ReceiverPhone { get; set; }

    [MaxLength(100)]
    public string? ReceiverEmail { get; set; }

    [MaxLength(200)]
    public string? ReceiverCompany { get; set; }

    public ContentType ContentType { get; set; } = ContentType.Parcel;
    
    public ShipmentClassificationType ShipmentClassification { get; set; } = ShipmentClassificationType.ParcelUpto30kg;
    
    public ShipmentMode ShipmentMode { get; set; } = ShipmentMode.Domestic;

    [MaxLength(500)]
    public string? ContentDescription { get; set; }

    public int NumberOfPieces { get; set; } = 1;

    public decimal ActualWeight { get; set; }

    public decimal VolumetricWeight { get; set; }

    public decimal ChargeableWeight { get; set; }

    public decimal Length { get; set; }

    public decimal Width { get; set; }

    public decimal Height { get; set; }

    public decimal DeclaredValue { get; set; }

    public PaymentMode PaymentMode { get; set; } = PaymentMode.Prepaid;

    public decimal CODAmount { get; set; }

    public bool CODCollected { get; set; } = false;

    public DateTime? CODCollectedDate { get; set; }
    
    public decimal CODCollectedAmount { get; set; }

    public decimal FreightCharge { get; set; }

    public decimal FuelSurcharge { get; set; }

    public decimal CODCharge { get; set; }

    public decimal InsuranceCharge { get; set; }

    public decimal OtherCharges { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalCharge { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal NetAmount { get; set; }

    public Guid? CurrencyId { get; set; }
    public Currency? Currency { get; set; }

    public ShipmentStatus Status { get; set; } = ShipmentStatus.Draft;

    public Guid? AssignedAgentId { get; set; }
    public CourierAgent? AssignedAgent { get; set; }

    public Guid? DeliveryRunSheetId { get; set; }
    public DeliveryRunSheet? DeliveryRunSheet { get; set; }

    public Guid? PickupRequestId { get; set; }
    public PickupRequest? PickupRequest { get; set; }

    public bool RequiresSignature { get; set; } = true;

    [MaxLength(200)]
    public string? ReceivedBy { get; set; }

    [MaxLength(100)]
    public string? Relationship { get; set; }

    public byte[]? SignatureImage { get; set; }

    public byte[]? PODImage { get; set; }

    [MaxLength(1000)]
    public string? DeliveryNotes { get; set; }
    
    public decimal? DeliveryLatitude { get; set; }
    
    public decimal? DeliveryLongitude { get; set; }

    [MaxLength(1000)]
    public string? SpecialInstructions { get; set; }

    [MaxLength(1000)]
    public string? InternalNotes { get; set; }

    public bool IsVoided { get; set; } = false;

    public Guid? VoidedBy { get; set; }

    public DateTime? VoidedDate { get; set; }

    [MaxLength(500)]
    public string? VoidReason { get; set; }
    
    public bool IsBilled { get; set; } = false;
    
    public DateTime? BilledDate { get; set; }

    public Guid? SalesInvoiceId { get; set; }
    
    public decimal MaterialCostAmount { get; set; }
    
    public bool MaterialCostRemitted { get; set; } = false;
    
    public Guid? RemittanceId { get; set; }
    
    public Guid? InboundManifestId { get; set; }
    public Manifest? InboundManifest { get; set; }
    
    public Guid? OutboundManifestId { get; set; }
    public Manifest? OutboundManifest { get; set; }
    
    public Guid? CurrentHubId { get; set; }
    
    [MaxLength(100)]
    public string? CurrentHubName { get; set; }
    
    public DateTime? LastScanTime { get; set; }
    
    [MaxLength(200)]
    public string? LastScanLocation { get; set; }

    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    
    public EmpostClassification EmpostClassification { get; set; } = EmpostClassification.Taxable;
    
    public decimal EmpostGrossAmount { get; set; }
    
    public decimal EmpostFeeAmount { get; set; }
    
    public EmpostFeeStatus EmpostFeeStatus { get; set; } = EmpostFeeStatus.Pending;
    
    public Guid? EmpostQuarterId { get; set; }
    public EmpostQuarter? EmpostQuarter { get; set; }
    
    public bool EmpostFeeCalculated { get; set; } = false;
    
    public DateTime? EmpostFeeCalculatedDate { get; set; }
    
    [MaxLength(500)]
    public string? EmpostClassificationReason { get; set; }

    public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
    public ICollection<ShipmentTracking> TrackingHistory { get; set; } = new List<ShipmentTracking>();
    public ICollection<ShipmentCharge> Charges { get; set; } = new List<ShipmentCharge>();
}

public class ShipmentItem : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public decimal Weight { get; set; }

    public decimal Length { get; set; }

    public decimal Width { get; set; }

    public decimal Height { get; set; }

    public decimal DeclaredValue { get; set; }

    [MaxLength(100)]
    public string? SKU { get; set; }

    [MaxLength(100)]
    public string? HSCode { get; set; }
}
