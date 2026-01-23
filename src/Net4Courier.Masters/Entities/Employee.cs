using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Employee : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? JoiningDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    
    public long? UserId { get; set; }
    public virtual User? User { get; set; }
    
    public long? BranchId { get; set; }
    public virtual Branch? Branch { get; set; }
}
