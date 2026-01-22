using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public enum ZoneType
{
    Local,
    Metro,
    State,
    Regional,
    National,
    International
}

public class CourierZone : BaseEntity
{
    public Guid? ZoneCategoryId { get; set; }
    public ZoneCategory? ZoneCategory { get; set; }

    [Required]
    [MaxLength(50)]
    public string ZoneCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ZoneName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(20)]
    public string? PostalCodeFrom { get; set; }

    [MaxLength(20)]
    public string? PostalCodeTo { get; set; }

    public bool IsRemote { get; set; } = false;

    public int TransitDays { get; set; } = 1;
    
    public ZoneType ZoneType { get; set; } = ZoneType.Local;

    public bool IsActive { get; set; } = true;

    public ICollection<ZoneRate> ZoneRates { get; set; } = new List<ZoneRate>();
    
    public ICollection<CourierZoneCountry> ZoneCountries { get; set; } = new List<CourierZoneCountry>();
    
    public ICollection<CourierZoneState> ZoneStates { get; set; } = new List<CourierZoneState>();
    
    public ICollection<EmployeeZoneAssignment> EmployeeAssignments { get; set; } = new List<EmployeeZoneAssignment>();
    
    public ICollection<PickupRequest> PickupRequests { get; set; } = new List<PickupRequest>();
    
    public ICollection<PickupIncentiveSchedule> IncentiveSchedules { get; set; } = new List<PickupIncentiveSchedule>();
}
