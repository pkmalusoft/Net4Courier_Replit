using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<FeaturePermission> FeaturePermissions { get; set; } = new List<FeaturePermission>();
}

public class RolePermission : BaseEntity
{
    public long RoleId { get; set; }
    public string MenuCode { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanPrint { get; set; }
    
    public virtual Role Role { get; set; } = null!;
}
