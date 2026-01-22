using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Empost.Models;

public enum AdvancePaymentStatus
{
    Pending,
    Paid,
    PartiallyPaid,
    Overdue
}

public class EmpostAdvancePayment : BaseEntity
{
    public Guid EmpostLicenseId { get; set; }
    public EmpostLicense EmpostLicense { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string PaymentReference { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public DateTime? PaymentDate { get; set; }

    public decimal AmountDue { get; set; } = 100000.00m;

    public decimal AmountPaid { get; set; }

    public AdvancePaymentStatus Status { get; set; } = AdvancePaymentStatus.Pending;

    public int ForLicenseYear { get; set; }

    public DateTime LicensePeriodStart { get; set; }

    public DateTime LicensePeriodEnd { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    [MaxLength(100)]
    public string? BankReference { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public Guid? RecordedBy { get; set; }
}
