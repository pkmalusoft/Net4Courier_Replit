using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public class ZoneCategory : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? CarrierName { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public ICollection<CourierZone> CourierZones { get; set; } = new List<CourierZone>();
}
