using System.ComponentModel.DataAnnotations;

namespace Server.Modules.BankReconciliation.Models;

public class BankStatementLine : BaseEntity
{
    public Guid BankStatementImportId { get; set; }
    public BankStatementImport? BankStatementImport { get; set; }

    public DateTime TransactionDate { get; set; }

    public DateTime? ValueDate { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? ChequeNumber { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal Balance { get; set; }

    // Computed field
    public decimal NetAmount => CreditAmount - DebitAmount;

    public bool IsMatched { get; set; } = false;
    public bool IsAdjustment { get; set; } = false;

    [MaxLength(1000)]
    public string? MatchNotes { get; set; }

    // For deduplication within statement
    [MaxLength(100)]
    public string? LineHash { get; set; }

    // Navigation
    public ICollection<ReconciliationMatch> Matches { get; set; } = new List<ReconciliationMatch>();
}
