using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class UserBranch : BaseEntity
{
    public long UserId { get; set; }
    public long BranchId { get; set; }
    public bool IsDefault { get; set; } = false;
    
    public virtual User User { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
}
