using System.ComponentModel.DataAnnotations;

namespace Server.Modules.CashBank.Models;

public class CashBankTransactionLine : BaseEntity
{
    public Guid CashBankTransactionId { get; set; }
    public CashBankTransaction CashBankTransaction { get; set; } = null!;

    public Guid DestinationAccountId { get; set; }
    public ChartOfAccount DestinationAccount { get; set; } = null!;

    public decimal GrossAmount { get; set; }

    public decimal TaxRate { get; set; }

    public bool IsTaxInclusive { get; set; }

    public decimal NetAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }

    public int LineNumber { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public ICollection<CashBankAllocation> Allocations { get; set; } = new List<CashBankAllocation>();
}
