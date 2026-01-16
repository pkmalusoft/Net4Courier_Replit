using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Country : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? IATACode { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<State> States { get; set; } = new List<State>();
    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
