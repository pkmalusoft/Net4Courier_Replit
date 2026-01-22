using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Net4Courier.Finance.Entities;

[Table("EmpostLicenses")]
public class EmpostLicense
{
    [Key]
    public long Id { get; set; }

    public long? CompanyId { get; set; }

    public long? BranchId { get; set; }

    [Required]
    [MaxLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LicenseeName { get; set; }

    public DateTime LicenseDate { get; set; }

    public DateTime LicensePeriodStart { get; set; }

    public DateTime LicensePeriodEnd { get; set; }

    public DateTime AdvancePaymentDueDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MinimumAdvanceAmount { get; set; } = 100000.00m;

    [Column(TypeName = "decimal(5,2)")]
    public decimal RoyaltyPercentage { get; set; } = 10.00m;

    [Column(TypeName = "decimal(10,2)")]
    public decimal WeightThresholdKg { get; set; } = 30.00m;

    public EmpostLicenseStatus Status { get; set; } = EmpostLicenseStatus.Active;

    public DateTime? RenewalDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }

    public ICollection<EmpostAdvancePayment> AdvancePayments { get; set; } = new List<EmpostAdvancePayment>();
    public ICollection<EmpostQuarter> Quarters { get; set; } = new List<EmpostQuarter>();
}
