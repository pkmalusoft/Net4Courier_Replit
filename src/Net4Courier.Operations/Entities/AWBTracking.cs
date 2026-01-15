using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Operations.Entities;

public class AWBTracking : BaseEntity
{
    public long InscanId { get; set; }
    public DateTime EventDateTime { get; set; }
    public CourierStatus StatusId { get; set; }
    public int? SubStatusId { get; set; }
    public string? Location { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Remarks { get; set; }
    public string? UpdatedByName { get; set; }
    public int? UpdatedByEmployeeId { get; set; }
    public long? BranchId { get; set; }
    public string? DeviceId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? PODImage { get; set; }
    public string? SignatureImage { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Relation { get; set; }
    public bool IsPublic { get; set; } = true;
    
    public virtual InscanMaster Inscan { get; set; } = null!;
}
