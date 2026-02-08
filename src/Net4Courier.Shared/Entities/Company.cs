namespace Net4Courier.Shared.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? Address3 { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? KeyPerson { get; set; }
    public string? MobileNo1 { get; set; }
    public string? MobileNo2 { get; set; }
    public string? Website { get; set; }
    public string? CompanyPrefix { get; set; }
    public string? AWBFormat { get; set; }
    public string? InvoicePrefix { get; set; }
    public string? InvoiceFormat { get; set; }
    public byte[]? CompanyLogo { get; set; }
    public string? LogoFileName { get; set; }
    public bool EnableAPI { get; set; }
    public bool EnableCashCustomerInvoice { get; set; }
    public bool AcceptSystem { get; set; }
    public bool AWBAlphaNumeric { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<FinancialYear> FinancialYears { get; set; } = new List<FinancialYear>();
}
