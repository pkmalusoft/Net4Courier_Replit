using System.ComponentModel.DataAnnotations;
using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Designation : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime? DeactivatedDate { get; set; }

    public long? DeactivatedByUserId { get; set; }

    [MaxLength(500)]
    public string? DeactivationReason { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
