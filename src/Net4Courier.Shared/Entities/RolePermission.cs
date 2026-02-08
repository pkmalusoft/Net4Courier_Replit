namespace Net4Courier.Shared.Entities;

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Role Role { get; set; } = null!;
}
