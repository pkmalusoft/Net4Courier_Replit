using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Net4Courier.Finance.Entities;

[Table("EmpostAuditLogs")]
public class EmpostAuditLog
{
    [Key]
    public long Id { get; set; }

    public EmpostAuditAction Action { get; set; }

    [Required]
    [MaxLength(200)]
    public string ActionDescription { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? EntityType { get; set; }

    public long? EntityId { get; set; }

    public long? EmpostLicenseId { get; set; }

    public long? EmpostQuarterId { get; set; }

    public int? Year { get; set; }

    public QuarterNumber? Quarter { get; set; }

    [MaxLength(50)]
    public string? AWBNumber { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OldValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? NewValue { get; set; }

    [MaxLength(500)]
    public string? OldData { get; set; }

    [MaxLength(500)]
    public string? NewData { get; set; }

    public long? PerformedBy { get; set; }

    [MaxLength(200)]
    public string? PerformedByName { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? IpAddress { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }
}
