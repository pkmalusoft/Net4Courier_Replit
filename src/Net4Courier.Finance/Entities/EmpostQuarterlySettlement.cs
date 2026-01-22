using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Net4Courier.Finance.Entities;

[Table("EmpostQuarterlySettlements")]
public class EmpostQuarterlySettlement
{
    [Key]
    public long Id { get; set; }

    public long EmpostQuarterId { get; set; }

    [ForeignKey(nameof(EmpostQuarterId))]
    public EmpostQuarter EmpostQuarter { get; set; } = null!;

    public long EmpostLicenseId { get; set; }

    [ForeignKey(nameof(EmpostLicenseId))]
    public EmpostLicense EmpostLicense { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string SettlementReference { get; set; } = string.Empty;

    public int Year { get; set; }

    public QuarterNumber Quarter { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CumulativeFeeToDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AdvancePaymentAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PreviousSettlements { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuarterFeeAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ReturnAdjustments { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetQuarterFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ExcessOverAdvance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPayable { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal VATOnFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPayable { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceDue { get; set; }

    public EmpostSettlementStatus Status { get; set; } = EmpostSettlementStatus.Pending;

    public DateTime? PaymentDate { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    [MaxLength(100)]
    public string? PaymentReference { get; set; }

    public DateTime SettlementDueDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public long? RecordedBy { get; set; }

    public DateTime? RecordedDate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }
}
