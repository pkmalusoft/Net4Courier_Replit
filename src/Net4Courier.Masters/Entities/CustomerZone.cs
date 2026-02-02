using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class CustomerZone : BaseEntity
{
    public long BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }
    
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<CustomerZoneCity> Cities { get; set; } = new List<CustomerZoneCity>();
    public virtual ICollection<CustomerZoneCourier> Couriers { get; set; } = new List<CustomerZoneCourier>();
    public virtual ICollection<Party> Customers { get; set; } = new List<Party>();
}
