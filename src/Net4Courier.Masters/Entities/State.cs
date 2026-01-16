using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class State : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public long CountryId { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual Country? Country { get; set; }
    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
