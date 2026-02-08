namespace Net4Courier.Shared.Entities;

public class UserBranch
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
}
