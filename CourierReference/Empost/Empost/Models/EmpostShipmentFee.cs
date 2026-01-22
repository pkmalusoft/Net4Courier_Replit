using System.ComponentModel.DataAnnotations;
using Server.Modules.Courier.Models;

namespace Server.Modules.Empost.Models;

public enum EmpostClassification
{
    Taxable,
    Exempt,
    FreightOver30Kg,
    LumpSumContract,
    Warehousing,
    PassThrough
}

public enum EmpostTaxabilityStatus
{
    Taxable,
    NonTaxable
}

public enum EmpostFeeStatus
{
    Pending,
    Settled,
    Credited,
    Adjusted
}

public class EmpostShipmentFee : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public Guid EmpostQuarterId { get; set; }
    public EmpostQuarter EmpostQuarter { get; set; } = null!;

    [MaxLength(50)]
    public string AWBNumber { get; set; } = string.Empty;

    public DateTime ShipmentDate { get; set; }

    public decimal ActualWeight { get; set; }

    public decimal ChargeableWeight { get; set; }

    public EmpostClassification Classification { get; set; } = EmpostClassification.Taxable;
    
    public ShipmentClassificationType ShipmentClassification { get; set; } = ShipmentClassificationType.ParcelUpto30kg;
    
    public ShipmentMode ShipmentMode { get; set; } = ShipmentMode.Domestic;
    
    public EmpostTaxabilityStatus TaxabilityStatus { get; set; } = EmpostTaxabilityStatus.Taxable;

    public decimal FreightCharge { get; set; }

    public decimal FuelSurcharge { get; set; }

    public decimal InsuranceCharge { get; set; }

    public decimal CODCharge { get; set; }

    public decimal OtherCharges { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal RoyaltyPercentage { get; set; } = 10.00m;

    public decimal EmpostFeeAmount { get; set; }

    public EmpostFeeStatus FeeStatus { get; set; } = EmpostFeeStatus.Pending;

    public bool IsReturnAdjusted { get; set; } = false;

    public DateTime? AdjustedDate { get; set; }

    [MaxLength(500)]
    public string? AdjustmentReason { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
