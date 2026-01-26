using System.ComponentModel.DataAnnotations;
using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public enum EmployeeStatus
{
    Active,
    OnLeave,
    Suspended,
    Resigned
}

public class Employee : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? FirstName { get; set; }

    [MaxLength(200)]
    public string? LastName { get; set; }

    public long? DesignationId { get; set; }

    public long? DepartmentId { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? Mobile { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(50)]
    public string? TaxIdNumber { get; set; }

    public DateTime? JoiningDate { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(200)]
    public string? EmergencyContact { get; set; }

    [MaxLength(20)]
    public string? EmergencyPhone { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    public bool IsActive { get; set; } = true;

    [MaxLength(50)]
    public string? BankAccountNumber { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(20)]
    public string? BankIFSC { get; set; }

    public decimal? BaseSalary { get; set; }

    public DateTime? DeactivatedDate { get; set; }

    public long? DeactivatedByUserId { get; set; }

    [MaxLength(500)]
    public string? DeactivationReason { get; set; }

    public long? UserId { get; set; }
    public virtual User? User { get; set; }

    public long? BranchId { get; set; }
    public virtual Branch? Branch { get; set; }

    public virtual Designation? Designation { get; set; }

    public virtual Department? Department { get; set; }
}
