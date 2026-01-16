using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public long? BranchId { get; set; }
    public long? RoleId { get; set; }
    public long? UserTypeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    
    public virtual Branch? Branch { get; set; }
    public virtual Role? Role { get; set; }
    public virtual UserType? UserType { get; set; }
}
