using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class CustomerZoneCity : BaseEntity
{
    public long CustomerZoneId { get; set; }
    public long CityId { get; set; }
    
    public virtual CustomerZone CustomerZone { get; set; } = null!;
    public virtual City City { get; set; } = null!;
}
