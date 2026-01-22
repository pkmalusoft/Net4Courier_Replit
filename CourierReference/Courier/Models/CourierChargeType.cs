using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public class CourierChargeType : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal DefaultAmount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;

    public bool IsSystemDefault { get; set; } = false;
}
