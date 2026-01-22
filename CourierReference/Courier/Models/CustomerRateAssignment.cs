using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public class CustomerRateAssignment : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string RateName { get; set; } = string.Empty;

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
