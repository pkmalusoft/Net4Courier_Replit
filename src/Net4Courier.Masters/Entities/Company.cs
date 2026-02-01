using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Logo { get; set; }
    
    public string? ContactPerson { get; set; }
    public string? ContactPersonPhone { get; set; }
    public string? ContactPersonEmail { get; set; }
    
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? SubscriptionPlan { get; set; }
    
    public long? CountryId { get; set; }
    public virtual Country? Country { get; set; }
    
    public long? StateId { get; set; }
    public virtual State? State { get; set; }
    
    public long? CityId { get; set; }
    public virtual City? City { get; set; }
    
    public long? CurrencyId { get; set; }
    public virtual Currency? Currency { get; set; }
    
    public bool UseAwbStockManagement { get; set; } = true;
    
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<FinancialYear> FinancialYears { get; set; } = new List<FinancialYear>();
}
