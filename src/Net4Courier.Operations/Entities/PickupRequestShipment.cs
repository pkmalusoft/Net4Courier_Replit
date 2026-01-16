using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Operations.Entities;

public class PickupRequestShipment : AuditableEntity
{
    public long PickupRequestId { get; set; }
    public int LineNo { get; set; }
    
    public string? Consignee { get; set; }
    public string? ConsigneeContact { get; set; }
    public string? ConsigneePhone { get; set; }
    public string? ConsigneeMobile { get; set; }
    public string? ConsigneeAddress1 { get; set; }
    public string? ConsigneeAddress2 { get; set; }
    public string? ConsigneeCity { get; set; }
    public string? ConsigneeState { get; set; }
    public string? ConsigneeCountry { get; set; }
    public string? ConsigneePostalCode { get; set; }
    public long? DestinationLocationId { get; set; }
    
    public int? Pieces { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public decimal? VolumetricWeight { get; set; }
    
    public string? CargoDescription { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? ReferenceNo { get; set; }
    public string? PONumber { get; set; }
    public decimal? DeclaredValue { get; set; }
    public string? Currency { get; set; }
    
    public PaymentMode PaymentModeId { get; set; } = PaymentMode.Prepaid;
    public DocumentType DocumentTypeId { get; set; } = DocumentType.Document;
    public int? ProductTypeId { get; set; }
    
    public ShipmentLineStatus Status { get; set; } = ShipmentLineStatus.Pending;
    public long? AWBId { get; set; }
    public string? AWBNo { get; set; }
    public DateTime? BookedAt { get; set; }
    
    public PickupRequest? PickupRequest { get; set; }
}

public enum ShipmentLineStatus
{
    Pending = 1,
    Booked = 2,
    Cancelled = 3
}
