using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class PostingSetup : AuditableEntity
{
    public long TransactionPageTypeId { get; set; }
    public TransactionPageType? TransactionPageType { get; set; }
    
    public long DebitAccountId { get; set; }
    public string? DebitAccountCode { get; set; }
    public string? DebitAccountName { get; set; }
    
    public long CreditAccountId { get; set; }
    public string? CreditAccountCode { get; set; }
    public string? CreditAccountName { get; set; }
    
    public string? Description { get; set; }
    public long? CompanyId { get; set; }
}
