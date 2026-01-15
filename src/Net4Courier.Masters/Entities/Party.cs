using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Party : BaseEntity
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public PartyType PartyType { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public decimal CreditLimit { get; set; }
    public int CreditDays { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<PartyAddress> Addresses { get; set; } = new List<PartyAddress>();
}

public enum PartyType
{
    Customer = 1,
    Agent = 2,
    Supplier = 3,
    ForwardingAgent = 4
}

public class PartyAddress : BaseEntity
{
    public long PartyId { get; set; }
    public string AddressType { get; set; } = "Primary";
    public string? BuildingName { get; set; }
    public string? Street { get; set; }
    public string? Area { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Landmark { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsDefault { get; set; }
    
    public virtual Party Party { get; set; } = null!;
}
