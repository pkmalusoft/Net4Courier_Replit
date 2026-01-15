using Net4Courier.Kernel.Entities;

namespace Net4Courier.Masters.Entities;

public class Branch : BaseEntity
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ManagerName { get; set; }
    public decimal? VatPercentage { get; set; }
    public bool IsHeadOffice { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    public virtual Company Company { get; set; } = null!;
}
