using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Branch : BaseEntity
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public long? CountryId { get; set; }
    public long? StateId { get; set; }
    public long? CityId { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ManagerName { get; set; }
    public decimal? VatPercentage { get; set; }
    public bool IsHeadOffice { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    public virtual Company Company { get; set; } = null!;
    public virtual Country? Country { get; set; }
    public virtual State? State { get; set; }
    public virtual City? City { get; set; }
}
