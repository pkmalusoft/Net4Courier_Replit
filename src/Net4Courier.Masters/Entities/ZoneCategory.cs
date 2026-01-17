using Net4Courier.Kernel.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Masters.Entities;

public class ZoneCategory : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public MovementType MovementType { get; set; } = MovementType.Domestic;
    public long? ForwardingAgentId { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    
    public virtual Party? ForwardingAgent { get; set; }
    public virtual ICollection<ZoneMatrix> ZoneMatrices { get; set; } = new List<ZoneMatrix>();
}
