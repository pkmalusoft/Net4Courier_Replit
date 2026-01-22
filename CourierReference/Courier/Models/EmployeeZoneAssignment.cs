using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public class EmployeeZoneAssignment : BaseEntity
{
    public Guid CourierAgentId { get; set; }
    public CourierAgent? CourierAgent { get; set; }

    public Guid CourierZoneId { get; set; }
    public CourierZone? CourierZone { get; set; }

    public int PriorityRank { get; set; } = 1;

    public bool IsPrimary { get; set; } = false;

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
