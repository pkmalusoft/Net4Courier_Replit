using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Masters.Entities;

public class RateCard : AuditableEntity
{
    public string RateCardName { get; set; } = string.Empty;
    public int? CourierServiceId { get; set; }
    public MovementType MovementTypeId { get; set; } = MovementType.Domestic;
    public long? ZoneCategoryId { get; set; }
    public PaymentMode PaymentModeId { get; set; } = PaymentMode.Prepaid;
    public bool IsDefault { get; set; }
    public RateBasedType RateBasedType { get; set; } = RateBasedType.Weight;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public RateCardStatus Status { get; set; } = RateCardStatus.Active;
    public long? CompanyId { get; set; }
    public long? ForwardingAgentId { get; set; }
    public string? Description { get; set; }
    
    public virtual ICollection<RateCardZone> RateCardZones { get; set; } = new List<RateCardZone>();
    public virtual ICollection<CustomerRateAssignment> CustomerAssignments { get; set; } = new List<CustomerRateAssignment>();
}

public enum RateBasedType
{
    Weight = 1,
    Box = 2,
    Flat = 3
}

public enum RateCardStatus
{
    Draft = 0,
    PendingApproval = 1,
    Active = 2,
    Expired = 3,
    Suspended = 4
}
