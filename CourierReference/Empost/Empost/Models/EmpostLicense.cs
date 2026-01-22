using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Empost.Models;

public enum EmpostLicenseStatus
{
    Active,
    PendingRenewal,
    Expired,
    Suspended
}

public class EmpostLicense : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LicenseeName { get; set; }

    public DateTime LicenseDate { get; set; }

    public DateTime LicensePeriodStart { get; set; }

    public DateTime LicensePeriodEnd { get; set; }

    public DateTime AdvancePaymentDueDate { get; set; }

    public decimal MinimumAdvanceAmount { get; set; } = 100000.00m;

    public decimal RoyaltyPercentage { get; set; } = 10.00m;

    public decimal WeightThresholdKg { get; set; } = 30.00m;

    public EmpostLicenseStatus Status { get; set; } = EmpostLicenseStatus.Active;

    public DateTime? RenewalDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<EmpostAdvancePayment> AdvancePayments { get; set; } = new List<EmpostAdvancePayment>();
    public ICollection<EmpostQuarter> Quarters { get; set; } = new List<EmpostQuarter>();
}
