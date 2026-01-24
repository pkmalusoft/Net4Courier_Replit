using System.ComponentModel.DataAnnotations;

namespace Server.Modules.BankReconciliation.Models;

public enum ReconciliationStatus
{
    Draft,
    InProgress,
    Completed,
    Locked
}

public class BankReconciliation : BaseEntity
{
    public Guid BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }

    [Required]
    [MaxLength(50)]
    public string ReconciliationNumber { get; set; } = string.Empty;

    public DateTime StatementDate { get; set; }
    
    public decimal StatementOpeningBalance { get; set; }
    public decimal StatementClosingBalance { get; set; }

    public decimal BookOpeningBalance { get; set; }
    public decimal BookClosingBalance { get; set; }

    public decimal DifferenceAmount { get; set; }

    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.Draft;

    public DateTime? CompletedDate { get; set; }
    public Guid? CompletedByUserId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Navigation
    public ICollection<BankStatementImport> StatementImports { get; set; } = new List<BankStatementImport>();
    public ICollection<ReconciliationMatch> Matches { get; set; } = new List<ReconciliationMatch>();
    public ICollection<ReconciliationAdjustment> Adjustments { get; set; } = new List<ReconciliationAdjustment>();
}
