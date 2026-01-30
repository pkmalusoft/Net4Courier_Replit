using System.ComponentModel.DataAnnotations;
using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public class CashBankTransactionLine : BaseEntity
{
    public long CashBankTransactionId { get; set; }
    public CashBankTransaction CashBankTransaction { get; set; } = null!;

    public long DestinationAccountId { get; set; }
    public AccountHead? DestinationAccount { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal TaxRate { get; set; }

    public bool IsTaxInclusive { get; set; }

    public decimal NetAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public long? CostCenterId { get; set; }

    public int LineNumber { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}
