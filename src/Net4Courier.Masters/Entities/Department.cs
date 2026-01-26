using System.ComponentModel.DataAnnotations;
using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Department : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public long? BranchId { get; set; }

    public long? ParentDepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;

    public DateTime? DeactivatedDate { get; set; }

    public long? DeactivatedByUserId { get; set; }

    [MaxLength(500)]
    public string? DeactivationReason { get; set; }

    public virtual Branch? Branch { get; set; }
    
    public virtual Department? ParentDepartment { get; set; }
    
    public virtual ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
    
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
