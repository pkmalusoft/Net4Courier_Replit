using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class QuickInscanMaster : BaseEntity
{
    public string InscanSheetNumber { get; set; } = string.Empty;
    public DateTime InscanDateTime { get; set; }
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public long? DepotId { get; set; }
    public long? FinancialYearId { get; set; }
    public int? ReceivedByEmployeeId { get; set; }
    public int? CollectedByEmployeeId { get; set; }
    public DateTime? CollectedDate { get; set; }
    public int? VehicleId { get; set; }
    public string? DriverName { get; set; }
    public long? AgentId { get; set; }
    public string? Source { get; set; }
    public bool IsOutscanReturned { get; set; }
    public int? UserId { get; set; }
    
    public virtual ICollection<InscanMaster> Inscans { get; set; } = new List<InscanMaster>();
}
