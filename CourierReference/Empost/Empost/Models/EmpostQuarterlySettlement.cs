using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Empost.Models;

public enum EmpostSettlementStatus
{
    Pending,
    PartiallyPaid,
    Paid,
    Waived
}

public class EmpostQuarterlySettlement : BaseEntity
{
    public Guid EmpostQuarterId { get; set; }
    public EmpostQuarter EmpostQuarter { get; set; } = null!;

    public Guid EmpostLicenseId { get; set; }
    public EmpostLicense EmpostLicense { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string SettlementReference { get; set; } = string.Empty;

    public int Year { get; set; }

    public QuarterNumber Quarter { get; set; }

    public decimal CumulativeFeeToDate { get; set; }

    public decimal AdvancePaymentAmount { get; set; }

    public decimal PreviousSettlements { get; set; }

    public decimal QuarterFeeAmount { get; set; }

    public decimal ReturnAdjustments { get; set; }

    public decimal NetQuarterFee { get; set; }

    public decimal ExcessOverAdvance { get; set; }

    public decimal AmountPayable { get; set; }

    public decimal VATOnFee { get; set; }

    public decimal TotalPayable { get; set; }

    public decimal AmountPaid { get; set; }

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

    public Guid? RecordedBy { get; set; }

    public DateTime? RecordedDate { get; set; }
}
