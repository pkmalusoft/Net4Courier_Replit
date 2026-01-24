using Net4Courier.Kernel.Entities;

namespace Net4Courier.Finance.Entities;

public enum ImportFormat
{
    CSV = 0,
    Excel = 1,
    OFX = 2,
    QIF = 3
}

public class BankStatementImport : BaseEntity
{
    public long BankReconciliationId { get; set; }
    
    public string FileName { get; set; } = string.Empty;
    public ImportFormat Format { get; set; }
    
    public string? FileHash { get; set; }
    
    public int TotalLines { get; set; }
    public int ImportedLines { get; set; }
    public int SkippedLines { get; set; }
    
    public string? ColumnMapping { get; set; }
    
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public int ImportedByUserId { get; set; }
    
    public string? ErrorLog { get; set; }
    
    public virtual BankReconciliation BankReconciliation { get; set; } = null!;
    public virtual ICollection<BankStatementLine> StatementLines { get; set; } = new List<BankStatementLine>();
}
