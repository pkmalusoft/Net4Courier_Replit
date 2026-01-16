using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class City : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public long? CountryId { get; set; }
    public long? StateId { get; set; }
    public bool IsHub { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    public virtual Country? Country { get; set; }
    public virtual State? State { get; set; }
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
