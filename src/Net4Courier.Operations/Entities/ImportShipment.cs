using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Operations.Entities;

public class ImportShipment : AuditableEntity
{
    public long ImportMasterId { get; set; }
    public long? ImportBagId { get; set; }
    
    public string AWBNo { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    
    public string? ShipperName { get; set; }
    public string? ShipperAddress { get; set; }
    public string? ShipperCity { get; set; }
    public string? ShipperCountry { get; set; }
    public string? ShipperPhone { get; set; }
    
    public string ConsigneeName { get; set; } = string.Empty;
    public string? ConsigneeAddress { get; set; }
    public string? ConsigneeCity { get; set; }
    public string? ConsigneeState { get; set; }
    public string ConsigneeCountry { get; set; } = string.Empty;
    public string? ConsigneePostalCode { get; set; }
    public string? ConsigneePhone { get; set; }
    public string? ConsigneeMobile { get; set; }
    public string? ConsigneeMobile2 { get; set; }
    
    public int Pieces { get; set; } = 1;
    public decimal Weight { get; set; }
    public decimal? VolumetricWeight { get; set; }
    public decimal? ChargeableWeight { get; set; }
    
    public ImportShipmentType ShipmentType { get; set; } = ImportShipmentType.NonDocument;
    public ImportType ImportType { get; set; } = ImportType.Courier;
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Prepaid;
    
    public string? ContentsDescription { get; set; }
    public string? SpecialInstructions { get; set; }
    
    public decimal? DeclaredValue { get; set; }
    public string? Currency { get; set; }
    public string? HSCode { get; set; }
    
    public string? IncoTerms { get; set; }
    public string? ServiceType { get; set; }
    public string? CustomerAccountNo { get; set; }
    
    public decimal? CustomsValue { get; set; }
    public decimal? DutyAmount { get; set; }
    public decimal? DutyVatAmount { get; set; }
    public decimal? VATAmount { get; set; }
    public decimal? OtherCharges { get; set; }
    public decimal? TotalCustomsCharges { get; set; }
    public decimal? CalculatedImportVAT { get; set; }
    public decimal? NetTotal { get; set; }
    
    public bool IsCOD { get; set; }
    public decimal? CODAmount { get; set; }
    
    public ImportShipmentStatus Status { get; set; } = ImportShipmentStatus.Expected;
    public CustomsStatus CustomsStatus { get; set; } = CustomsStatus.NotApplicable;
    public CustomsWorkStatus? CustomsWorkStatus { get; set; }
    public CustomsHoldReason HoldReason { get; set; } = CustomsHoldReason.None;
    public string? HoldReasonDetails { get; set; }
    
    public string? ImporterOfRecord { get; set; }
    public string? CustomsEntryNumber { get; set; }
    public string? ExaminationRemarks { get; set; }
    
    public DateTime? InscannedAt { get; set; }
    public long? InscannedByUserId { get; set; }
    public string? InscannedByUserName { get; set; }
    
    public DateTime? CustomsClearedAt { get; set; }
    public long? CustomsClearedByUserId { get; set; }
    public string? CustomsClearedByUserName { get; set; }
    
    public DateTime? ReleasedAt { get; set; }
    public long? ReleasedByUserId { get; set; }
    public string? ReleasedByUserName { get; set; }
    
    public DateTime? HandedOverAt { get; set; }
    public long? HandedOverToUserId { get; set; }
    public string? HandedOverToUserName { get; set; }
    
    public long? LinkedInscanMasterId { get; set; }
    
    public string? Remarks { get; set; }
    
    public virtual ImportMaster? ImportMaster { get; set; }
    public virtual ImportBag? ImportBag { get; set; }
    public virtual ICollection<ImportShipmentNote> Notes { get; set; } = new List<ImportShipmentNote>();
    public virtual ICollection<ImportShipmentOtherCharge> OtherChargesList { get; set; } = new List<ImportShipmentOtherCharge>();
}
