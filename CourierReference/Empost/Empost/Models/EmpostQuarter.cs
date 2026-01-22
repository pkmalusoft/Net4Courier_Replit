using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Empost.Models;

public enum QuarterNumber
{
    Q1 = 1,
    Q2 = 2,
    Q3 = 3,
    Q4 = 4
}

public enum QuarterStatus
{
    Open,
    PendingSubmission,
    Submitted,
    Locked
}

public class EmpostQuarter : BaseEntity
{
    public Guid EmpostLicenseId { get; set; }
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

    public Guid? LockedBy { get; set; }

    [MaxLength(200)]
    public string? LockedByName { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public Guid? SubmittedBy { get; set; }

    [MaxLength(200)]
    public string? SubmittedByName { get; set; }

    public decimal TotalGrossRevenue { get; set; }

    public decimal TotalTaxableRevenue { get; set; }

    public decimal TotalExemptRevenue { get; set; }

    public decimal TotalEmpostFee { get; set; }

    public decimal TotalReturnAdjustments { get; set; }

    public decimal NetEmpostFee { get; set; }

    public int TotalShipments { get; set; }

    public int TaxableShipments { get; set; }

    public int ExemptShipments { get; set; }

    public int ReturnedShipments { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public ICollection<EmpostShipmentFee> ShipmentFees { get; set; } = new List<EmpostShipmentFee>();
    public ICollection<EmpostReturnAdjustment> ReturnAdjustments { get; set; } = new List<EmpostReturnAdjustment>();
    public ICollection<EmpostQuarterlySettlement> Settlements { get; set; } = new List<EmpostQuarterlySettlement>();
}
