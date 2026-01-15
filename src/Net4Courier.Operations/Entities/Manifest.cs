using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class Manifest : BaseEntity
{
    public string ManifestNo { get; set; } = string.Empty;
    public DateTime ManifestDate { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? FinancialYearId { get; set; }
    public long? OriginBranchId { get; set; }
    public long? DestinationBranchId { get; set; }
    public string? OriginCity { get; set; }
    public string? DestinationCity { get; set; }
    public string? FlightNo { get; set; }
    public string? VehicleNo { get; set; }
    public string? DriverName { get; set; }
    public string? MAWB { get; set; }
    public int? TotalAWBs { get; set; }
    public decimal? TotalWeight { get; set; }
    public decimal? TotalValue { get; set; }
    public string? Remarks { get; set; }
    public int? ManifestStatus { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public int? DispatchedBy { get; set; }
    public int? ReceivedBy { get; set; }
}
