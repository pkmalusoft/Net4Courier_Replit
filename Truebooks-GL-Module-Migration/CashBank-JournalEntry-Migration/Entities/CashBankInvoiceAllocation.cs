using Server.Modules.AR.Models;

namespace Server.Modules.CashBank.Models;

public class CashBankInvoiceAllocation : BaseEntity
{
    public Guid CashBankTransactionId { get; set; }
    public CashBankTransaction CashBankTransaction { get; set; } = null!;

    public Guid SalesInvoiceId { get; set; }
    public SalesInvoice SalesInvoice { get; set; } = null!;

    public decimal AllocationAmount { get; set; }

    public DateTime AllocationDate { get; set; } = DateTime.UtcNow;
}
