using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class Vehicle : BaseEntity
{
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public string VehicleNo { get; set; } = string.Empty;
    public string? VehicleType { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public decimal? Capacity { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
}
