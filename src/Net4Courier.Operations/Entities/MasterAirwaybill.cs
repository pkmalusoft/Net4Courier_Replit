using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public enum MAWBStatus
{
    Draft = 0,
    Finalized = 1,
    Dispatched = 2,
    InTransit = 3,
    Received = 4,
    Cancelled = 5
}

public class MasterAirwaybill : AuditableEntity
{
    public string MAWBNo { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    
    public long? OriginCityId { get; set; }
    public string? OriginCityName { get; set; }
    public long? OriginCountryId { get; set; }
    public string? OriginCountryName { get; set; }
    public string? OriginAirportCode { get; set; }
    
    public long? DestinationCityId { get; set; }
    public string? DestinationCityName { get; set; }
    public long? DestinationCountryId { get; set; }
    public string? DestinationCountryName { get; set; }
    public string? DestinationAirportCode { get; set; }
    
    public string? CarrierCode { get; set; }
    public string? CarrierName { get; set; }
    public string? FlightNo { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public TimeSpan? DepartureTime { get; set; }
    public TimeSpan? ArrivalTime { get; set; }
    
    public long? CoLoaderId { get; set; }
    public string? CoLoaderName { get; set; }
    public string? CoLoaderMAWBNo { get; set; }
    
    public int TotalBags { get; set; }
    public int TotalPieces { get; set; }
    public decimal TotalGrossWeight { get; set; }
    public decimal TotalChargeableWeight { get; set; }
    
    public MAWBStatus Status { get; set; } = MAWBStatus.Draft;
    public DateTime? FinalizedAt { get; set; }
    public long? FinalizedByUserId { get; set; }
    public string? FinalizedByUserName { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public long? DispatchedByUserId { get; set; }
    public string? DispatchedByUserName { get; set; }
    
    public string? Remarks { get; set; }
    public string? CustomsDeclarationNo { get; set; }
    public string? ExportPermitNo { get; set; }
    
    public virtual ICollection<MAWBBag> Bags { get; set; } = new List<MAWBBag>();
}
