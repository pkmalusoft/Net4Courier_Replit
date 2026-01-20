using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Operations.Entities;

public class ImportMaster : AuditableEntity
{
    public string ImportRefNo { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public long? FinancialYearId { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    
    public ImportMode ImportMode { get; set; } = ImportMode.Air;
    public MasterReferenceType MasterReferenceType { get; set; } = MasterReferenceType.MAWB;
    public string MasterReferenceNumber { get; set; } = string.Empty;
    
    public long? OriginCountryId { get; set; }
    public string? OriginCountryName { get; set; }
    public long? OriginCityId { get; set; }
    public string? OriginCityName { get; set; }
    public string? OriginPortCode { get; set; }
    
    public long? DestinationCountryId { get; set; }
    public string? DestinationCountryName { get; set; }
    public long? DestinationCityId { get; set; }
    public string? DestinationCityName { get; set; }
    public string? DestinationPortCode { get; set; }
    
    public DateTime? ETD { get; set; }
    public DateTime? ETA { get; set; }
    public DateTime? ActualArrivalDate { get; set; }
    
    public string? CarrierName { get; set; }
    public string? CarrierCode { get; set; }
    
    public string? FlightNo { get; set; }
    public DateTime? FlightDate { get; set; }
    
    public string? VesselName { get; set; }
    public string? VoyageNumber { get; set; }
    
    public string? TruckNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    
    public string? ManifestNumber { get; set; }
    public CargoType CargoType { get; set; } = CargoType.Courier;
    
    public int TotalBags { get; set; }
    public int TotalShipments { get; set; }
    public decimal? TotalGrossWeight { get; set; }
    public decimal? TotalChargeableWeight { get; set; }
    public int? TotalPieces { get; set; }
    
    public long? ImportWarehouseId { get; set; }
    public string? ImportWarehouseName { get; set; }
    
    public ImportMasterStatus Status { get; set; } = ImportMasterStatus.Draft;
    public string? Remarks { get; set; }
    
    public string? CoLoaderName { get; set; }
    public long? CoLoaderId { get; set; }
    public string? CoLoaderRefNo { get; set; }
    
    public string? CustomsDeclarationNo { get; set; }
    public string? ExportPermitNo { get; set; }
    
    public DateTime? InscannedAt { get; set; }
    public long? InscannedByUserId { get; set; }
    public string? InscannedByUserName { get; set; }
    
    public DateTime? ClosedAt { get; set; }
    public long? ClosedByUserId { get; set; }
    public string? ClosedByUserName { get; set; }
    
    public virtual ICollection<ImportBag> Bags { get; set; } = new List<ImportBag>();
}
