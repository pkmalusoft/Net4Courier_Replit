namespace Server.Modules.CashBank.Models;

public class CashBankAllocation : BaseEntity
{
    public Guid CashBankTransactionLineId { get; set; }
    public CashBankTransactionLine CashBankTransactionLine { get; set; } = null!;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal Percentage { get; set; }
}
