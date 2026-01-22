using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public class CourierZoneCountry : BaseEntity
{
    public Guid CourierZoneId { get; set; }
    public CourierZone CourierZone { get; set; } = null!;

    public Guid CountryId { get; set; }

    [MaxLength(100)]
    public string CountryName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string CountryCode { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;
}
