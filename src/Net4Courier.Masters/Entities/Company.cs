using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Logo { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<FinancialYear> FinancialYears { get; set; } = new List<FinancialYear>();
}
