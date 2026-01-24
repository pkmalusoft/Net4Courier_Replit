using System.ComponentModel.DataAnnotations;

namespace Server.Modules.BankReconciliation.Models;

public enum ImportFormat
{
    CSV,
    Excel,
    OFX,
    QIF
}

public class BankStatementImport : BaseEntity
{
    public Guid BankReconciliationId { get; set; }
    public BankReconciliation? BankReconciliation { get; set; }

    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    public ImportFormat Format { get; set; }

    [MaxLength(100)]
    public string? FileHash { get; set; } // SHA-256 hash for deduplication

    public int TotalLines { get; set; }
    public int ImportedLines { get; set; }
    public int SkippedLines { get; set; }

    // Column mapping (stored as JSON)
    [MaxLength(2000)]
    public string? ColumnMapping { get; set; }

    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public Guid ImportedByUserId { get; set; }

    [MaxLength(1000)]
    public string? ErrorLog { get; set; }

    // Navigation
    public ICollection<BankStatementLine> StatementLines { get; set; } = new List<BankStatementLine>();
}
