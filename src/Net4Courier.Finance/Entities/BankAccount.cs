using Net4Courier.Masters.Entities;

namespace Net4Courier.Finance.Entities;

public class BankAccount
{
    public long Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? SwiftCode { get; set; }
    public string? IbanNumber { get; set; }
    public decimal OpeningBalance { get; set; }
    public DateTime OpeningBalanceDate { get; set; } = DateTime.UtcNow.Date;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    
    public long AccountHeadId { get; set; }
    public AccountHead? AccountHead { get; set; }
    
    public long? CurrencyId { get; set; }
    public Currency? Currency { get; set; }
    
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}
