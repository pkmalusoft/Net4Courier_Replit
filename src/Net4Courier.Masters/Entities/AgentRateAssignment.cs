using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class AgentRateAssignment : AuditableEntity
{
    public long AgentId { get; set; }
    public long RateCardId { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int Priority { get; set; } = 1;
    public string? Remarks { get; set; }
    public long? CompanyId { get; set; }

    public virtual Party? Agent { get; set; }
    public virtual RateCard? RateCard { get; set; }
}
