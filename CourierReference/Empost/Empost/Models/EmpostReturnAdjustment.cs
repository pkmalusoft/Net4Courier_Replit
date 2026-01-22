using System.ComponentModel.DataAnnotations;
using Server.Modules.Courier.Models;

namespace Server.Modules.Empost.Models;

public enum EmpostAdjustmentType
{
    FullRefund,
    PartialRefund,
    Reversal
}

public enum AdjustmentStatus
{
    Pending,
    Applied,
    Rejected
}

public class EmpostReturnAdjustment : BaseEntity
{
    public Guid EmpostShipmentFeeId { get; set; }
    public EmpostShipmentFee EmpostShipmentFee { get; set; } = null!;

    public Guid EmpostQuarterId { get; set; }
    public EmpostQuarter EmpostQuarter { get; set; } = null!;

    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    [MaxLength(50)]
    public string AWBNumber { get; set; } = string.Empty;

    public DateTime OriginalShipmentDate { get; set; }

    public DateTime ReturnDate { get; set; }

    public EmpostAdjustmentType AdjustmentType { get; set; } = EmpostAdjustmentType.FullRefund;

    public decimal OriginalGrossAmount { get; set; }

    public decimal OriginalFeeAmount { get; set; }

    public decimal AdjustmentAmount { get; set; }

    public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Pending;

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public DateTime? AppliedDate { get; set; }

    public Guid? AppliedBy { get; set; }

    [MaxLength(200)]
    public string? AppliedByName { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
