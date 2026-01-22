using System.ComponentModel.DataAnnotations;

namespace Server.Modules.Courier.Models;

public enum AgentType
{
    DeliveryAgent,
    PickupAgent,
    FranchisePartner,
    SubContractor
}

public enum CommissionType
{
    Percentage,
    FixedPerShipment,
    FixedPerKg
}

public class CourierAgent : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string AgentCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public AgentType AgentType { get; set; } = AgentType.DeliveryAgent;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? Mobile { get; set; }

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    public Guid? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public CommissionType CommissionType { get; set; } = CommissionType.Percentage;
    
    public decimal CommissionRate { get; set; } = 0;
    
    public decimal CommissionPercent { get; set; } = 0;
    
    public decimal FixedCommission { get; set; } = 0;

    public decimal CODCommissionPercent { get; set; } = 0;

    public bool CanCollectCOD { get; set; } = true;
    
    [MaxLength(50)]
    public string? BankAccountNo { get; set; }
    
    [MaxLength(100)]
    public string? BankName { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Shipment> AssignedShipments { get; set; } = new List<Shipment>();
    public ICollection<DeliveryRunSheet> DeliveryRunSheets { get; set; } = new List<DeliveryRunSheet>();
    public ICollection<EmployeeZoneAssignment> ZoneAssignments { get; set; } = new List<EmployeeZoneAssignment>();
    public ICollection<PickupCommitment> PickupCommitments { get; set; } = new List<PickupCommitment>();
}
