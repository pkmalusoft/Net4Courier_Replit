using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Location : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Pincode { get; set; }
    public long? CityId { get; set; }
    public long? BranchId { get; set; }
    public bool IsServiceable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    
    public virtual City? City { get; set; }
    public virtual Branch? Branch { get; set; }
}
