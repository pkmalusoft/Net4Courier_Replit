using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class BankAccount : AuditableEntity
{
    public long CompanyId { get; set; }
    public long BranchId { get; set; }
    
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? SwiftCode { get; set; }
    public string? IbanNumber { get; set; }
    
    public long AccountHeadId { get; set; }
    public long? CurrencyId { get; set; }
    
    public decimal OpeningBalance { get; set; }
    public DateTime OpeningBalanceDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    
    public virtual AccountHead AccountHead { get; set; } = null!;
    public virtual ICollection<BankReconciliation> Reconciliations { get; set; } = new List<BankReconciliation>();
}
