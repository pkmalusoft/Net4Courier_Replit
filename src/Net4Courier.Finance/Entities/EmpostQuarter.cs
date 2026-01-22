using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Net4Courier.Finance.Entities;

[Table("EmpostQuarters")]
public class EmpostQuarter
{
    [Key]
    public long Id { get; set; }

    public long EmpostLicenseId { get; set; }

    [ForeignKey(nameof(EmpostLicenseId))]
    public EmpostLicense EmpostLicense { get; set; } = null!;

    public int Year { get; set; }

    public QuarterNumber Quarter { get; set; }

    [MaxLength(10)]
    public string QuarterName { get; set; } = string.Empty;

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public DateTime SubmissionDeadline { get; set; }

    public QuarterStatus Status { get; set; } = QuarterStatus.Open;

    public bool IsLocked { get; set; } = false;

    public DateTime? LockedDate { get; set; }

    public long? LockedBy { get; set; }

    [MaxLength(200)]
    public string? LockedByName { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public long? SubmittedBy { get; set; }

    [MaxLength(200)]
    public string? SubmittedByName { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalGrossRevenue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalTaxableRevenue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalExemptRevenue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalEmpostFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalReturnAdjustments { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetEmpostFee { get; set; }

    public int TotalShipments { get; set; }

    public int TaxableShipments { get; set; }

    public int ExemptShipments { get; set; }

    public int ReturnedShipments { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }

    public ICollection<EmpostShipmentFee> ShipmentFees { get; set; } = new List<EmpostShipmentFee>();
    public ICollection<EmpostReturnAdjustment> ReturnAdjustments { get; set; } = new List<EmpostReturnAdjustment>();
    public ICollection<EmpostQuarterlySettlement> Settlements { get; set; } = new List<EmpostQuarterlySettlement>();
}
