using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Masters.Entities;

public class BranchAWBConfig : BaseEntity
{
    public long BranchId { get; set; }
    public MovementType MovementType { get; set; }
    public string AWBPrefix { get; set; } = string.Empty;
    public long StartingNumber { get; set; } = 1;
    public int IncrementBy { get; set; } = 1;
    public long LastUsedNumber { get; set; } = 0;
    
    public virtual Branch Branch { get; set; } = null!;
}
