using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Net4Courier.Finance.Entities;

[Table("EmpostAdvancePayments")]
public class EmpostAdvancePayment
{
    [Key]
    public long Id { get; set; }

    public long EmpostLicenseId { get; set; }

    [ForeignKey(nameof(EmpostLicenseId))]
    public EmpostLicense EmpostLicense { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string PaymentReference { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public DateTime? PaymentDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountDue { get; set; } = 100000.00m;

    [Column(TypeName = "decimal(18,2)")]
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

    public long? RecordedBy { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }
}
