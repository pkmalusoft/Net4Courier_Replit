using Net4Courier.Kernel.Entities;

namespace Net4Courier.Operations.Entities;

public class PickupSchedule : BaseEntity
{
    public long? CompanyId { get; set; }
    public long? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan FromTime { get; set; }
    public TimeSpan ToTime { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    public string DisplayText => $"{Name} ({FromTime:hh\\:mm} - {ToTime:hh\\:mm})";
}
