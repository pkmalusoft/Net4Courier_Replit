using System.ComponentModel.DataAnnotations;
using Server.Modules.CashBank.Models;

namespace Server.Modules.BankReconciliation.Models;

public enum ReconciliationAdjustmentType
{
    BankFee,
    BankInterest,
    BankCharge,
    UnrecordedDeposit,
    UnrecordedWithdrawal,
    Error,
    Other
}

public class ReconciliationAdjustment : BaseEntity
{
    public Guid BankReconciliationId { get; set; }
    public BankReconciliation? BankReconciliation { get; set; }

    public Guid? BankStatementLineId { get; set; }
    public BankStatementLine? BankStatementLine { get; set; }

    public ReconciliationAdjustmentType AdjustmentType { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow.Date;

    // Auto-created CashBankTransaction when adjustment is posted
    public Guid? CashBankTransactionId { get; set; }
    public CashBankTransaction? CashBankTransaction { get; set; }

    public bool IsPosted { get; set; } = false;
    public DateTime? PostedAt { get; set; }
    public Guid? PostedByUserId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
