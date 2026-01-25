using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class CustomerBranch : BaseEntity
{
    public long PartyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public long? CityId { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsHeadOffice { get; set; }
    
    public virtual Party Party { get; set; } = null!;
    public virtual City? CityNavigation { get; set; }
}
