namespace Net4Courier.Shared.Entities;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public int RoleId { get; set; }
    public int? DefaultBranchId { get; set; }
    public bool IsLoggedIn { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LoginExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Role Role { get; set; } = null!;
    public virtual Branch? DefaultBranch { get; set; }
    public virtual ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}
