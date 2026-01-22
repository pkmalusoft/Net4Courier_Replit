using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Net4Courier.Finance.Entities;

[Table("EmpostShipmentFees")]
public class EmpostShipmentFee
{
    [Key]
    public long Id { get; set; }

    public long InscanMasterId { get; set; }

    public long EmpostQuarterId { get; set; }

    [ForeignKey(nameof(EmpostQuarterId))]
    public EmpostQuarter EmpostQuarter { get; set; } = null!;

    [MaxLength(50)]
    public string AWBNumber { get; set; } = string.Empty;

    public DateTime ShipmentDate { get; set; }

    [Column(TypeName = "decimal(10,3)")]
    public decimal ActualWeight { get; set; }

    [Column(TypeName = "decimal(10,3)")]
    public decimal ChargeableWeight { get; set; }

    public EmpostClassification Classification { get; set; } = EmpostClassification.Taxable;

    public EmpostTaxabilityStatus TaxabilityStatus { get; set; } = EmpostTaxabilityStatus.Taxable;

    [Column(TypeName = "decimal(18,2)")]
    public decimal FreightCharge { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal FuelSurcharge { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InsuranceCharge { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CODCharge { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherCharges { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossAmount { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal RoyaltyPercentage { get; set; } = 10.00m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal EmpostFeeAmount { get; set; }

    public EmpostFeeStatus FeeStatus { get; set; } = EmpostFeeStatus.Pending;

    public bool IsReturnAdjusted { get; set; } = false;

    public DateTime? AdjustedDate { get; set; }

    [MaxLength(500)]
    public string? AdjustmentReason { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }
}
