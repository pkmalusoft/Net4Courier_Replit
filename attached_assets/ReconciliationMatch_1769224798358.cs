using System.ComponentModel.DataAnnotations;
using Server.Modules.CashBank.Models;

namespace Server.Modules.BankReconciliation.Models;

public enum MatchType
{
    Automatic,
    Manual,
    System
}

public class ReconciliationMatch : BaseEntity
{
    public Guid BankReconciliationId { get; set; }
    public BankReconciliation? BankReconciliation { get; set; }

    public Guid BankStatementLineId { get; set; }
    public BankStatementLine? BankStatementLine { get; set; }

    public Guid? CashBankTransactionId { get; set; }
    public CashBankTransaction? CashBankTransaction { get; set; }

    public decimal MatchedAmount { get; set; }

    public MatchType MatchType { get; set; } = MatchType.Manual;

    [MaxLength(50)]
    public string? MatchGroup { get; set; } // For grouping multiple transactions to one statement line

    public DateTime MatchedAt { get; set; } = DateTime.UtcNow;
    public Guid MatchedByUserId { get; set; }

    [MaxLength(500)]
    public string? MatchNotes { get; set; }

    public bool IsReversed { get; set; } = false;
    public DateTime? ReversedAt { get; set; }
    public Guid? ReversedByUserId { get; set; }

    [MaxLength(500)]
    public string? ReversalReason { get; set; }
}
