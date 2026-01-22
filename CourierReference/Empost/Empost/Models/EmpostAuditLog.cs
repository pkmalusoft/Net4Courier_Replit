using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Empost.Models;

public enum EmpostAuditAction
{
    LicenseCreated,
    LicenseUpdated,
    LicenseRenewed,
    AdvancePaymentRecorded,
    QuarterLocked,
    QuarterUnlocked,
    QuarterSubmitted,
    SettlementCreated,
    SettlementPaid,
    ShipmentFeeCalculated,
    ReturnAdjustmentCreated,
    ReturnAdjustmentApplied,
    ReportGenerated,
    ReconciliationPerformed,
    ClassificationOverride
}

public class EmpostAuditLog : BaseEntity
{
    public EmpostAuditAction Action { get; set; }

    [Required]
    [MaxLength(200)]
    public string ActionDescription { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? EntityType { get; set; }

    public Guid? EntityId { get; set; }

    public Guid? EmpostLicenseId { get; set; }

    public Guid? EmpostQuarterId { get; set; }

    public int? Year { get; set; }

    public QuarterNumber? Quarter { get; set; }

    [MaxLength(50)]
    public string? AWBNumber { get; set; }

    public decimal? OldValue { get; set; }

    public decimal? NewValue { get; set; }

    [MaxLength(500)]
    public string? OldData { get; set; }

    [MaxLength(500)]
    public string? NewData { get; set; }

    public Guid? PerformedBy { get; set; }

    [MaxLength(200)]
    public string? PerformedByName { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? IpAddress { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
