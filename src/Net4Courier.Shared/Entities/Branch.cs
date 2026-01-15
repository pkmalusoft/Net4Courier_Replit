namespace Net4Courier.Shared.Entities;

public class Branch
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? Address3 { get; set; }
    public string? KeyPerson { get; set; }
    public string? Phone { get; set; }
    public string? MobileNo1 { get; set; }
    public string? MobileNo2 { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? BranchPrefix { get; set; }
    public string? AWBFormat { get; set; }
    public string? InvoicePrefix { get; set; }
    public string? InvoiceFormat { get; set; }
    public string? VATRegistrationNo { get; set; }
    public decimal? VATPercent { get; set; }
    public string? CODReceiptPrefix { get; set; }
    public string? CODReceiptFormat { get; set; }
    public bool TaxEnable { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CurrentFinancialYearId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Company Company { get; set; } = null!;
    public virtual FinancialYear? CurrentFinancialYear { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
