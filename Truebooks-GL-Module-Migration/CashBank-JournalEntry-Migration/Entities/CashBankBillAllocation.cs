using Server.Modules.AP.Models;

namespace Server.Modules.CashBank.Models;

public class CashBankBillAllocation : BaseEntity
{
    public Guid CashBankTransactionId { get; set; }
    public CashBankTransaction CashBankTransaction { get; set; } = null!;

    public Guid PurchaseBillId { get; set; }
    public PurchaseBill PurchaseBill { get; set; } = null!;

    public decimal AllocationAmount { get; set; }

    public DateTime AllocationDate { get; set; } = DateTime.UtcNow;
}
