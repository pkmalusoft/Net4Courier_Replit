using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class CustomerZoneCourier : BaseEntity
{
    public long CustomerZoneId { get; set; }
    public long UserId { get; set; }
    
    public virtual CustomerZone CustomerZone { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
