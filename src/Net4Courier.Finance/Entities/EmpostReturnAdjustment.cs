using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Net4Courier.Finance.Entities;

[Table("EmpostReturnAdjustments")]
public class EmpostReturnAdjustment
{
    [Key]
    public long Id { get; set; }

    public long EmpostShipmentFeeId { get; set; }

    [ForeignKey(nameof(EmpostShipmentFeeId))]
    public EmpostShipmentFee EmpostShipmentFee { get; set; } = null!;

    public long EmpostQuarterId { get; set; }

    [ForeignKey(nameof(EmpostQuarterId))]
    public EmpostQuarter EmpostQuarter { get; set; } = null!;

    public long InscanMasterId { get; set; }

    [MaxLength(50)]
    public string AWBNumber { get; set; } = string.Empty;

    public DateTime OriginalShipmentDate { get; set; }

    public DateTime ReturnDate { get; set; }

    public EmpostAdjustmentType AdjustmentType { get; set; } = EmpostAdjustmentType.FullRefund;

    [Column(TypeName = "decimal(18,2)")]
    public decimal OriginalGrossAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OriginalFeeAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AdjustmentAmount { get; set; }

    public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Pending;

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public DateTime? AppliedDate { get; set; }

    public long? AppliedBy { get; set; }

    [MaxLength(200)]
    public string? AppliedByName { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }
}
